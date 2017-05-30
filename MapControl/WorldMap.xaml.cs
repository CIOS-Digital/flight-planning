using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CIOSDigital.FlightPlan;
using CIOSDigital.MapDB;
using System.Threading;
using System.ComponentModel;

namespace CIOSDigital.MapControl
{
    /// <summary>
    /// Interaction logic for WorldMap.xaml
    /// </summary>
    public partial class WorldMap : UserControl
    {

        public static DependencyProperty DownloadsActiveProperty =
            DependencyProperty.Register("DownloadsActive", typeof(int), typeof(WorldMap));
        public int DownloadsActive {
            get {
                return (int)this.GetValue(DownloadsActiveProperty);
            }
            set {
                this.SetValue(DownloadsActiveProperty, (int)value);
            }
        }

        private static Coordinate Seattle = new Coordinate(47.62m, -122.35m);

        private int LastZoomLevel {
            get; set;
        }

        private int ZoomLevel {
            get {
                return this.ZoomSelector.ZoomLevel;
            }
        }

        private MapType MapType {
            get {
                return this.TypeSelector.MapType;
            }
        }

        private Point Location { get; set; }

        private MapProvider ImageSource { get; set; }
        private Plan ActivePlan { get; set; }

        private bool MouseIsDown { get; set; }
        private Point MousePosition { get; set; }

        public WorldMap()
        {
            InitializeComponent();
            this.MouseIsDown = false;
            this.MousePosition = new Point(0, 0);
            this.Location = new Point(0, 0);

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
            this.ImageSource = SQLiteMap.OpenDB();
            LastZoomLevel = ZoomLevel;
            Point seattle = PixelLocationOf(Seattle, 9);
            PerformScrollBy(new Vector(seattle.X, seattle.Y));
        }

        private static Point PixelLocationOf(Coordinate c, int zoomLevel)
        {
            return PixelLocationOf(c.Latitude, c.Longitude, zoomLevel);
        }

        private static Coordinate LocationOfPixel(Point p, int zoomLevel)
        {
            return LocationOfPixel(p.X, p.Y, zoomLevel);
        }

        const double basePixelsPerScalerLatitude = 58.0 * 359.0;
        const double basePixelsPerDegreeLongitude = 364.0;
        const int baseZoomLevel = 9;
        const double degreesToRadians = Math.PI / 180.0;
        const double radiansToDegrees = 180.0 / Math.PI;
        const double piFourths = Math.PI / 4.0;

        private static Point PixelLocationOf(decimal latitudeDegrees, decimal longitudeDegrees, int zoomLevel)
        {
            double pixelScale = Math.Pow(2.0, (double)zoomLevel - baseZoomLevel);

            double latitudeRadians = degreesToRadians * (double)latitudeDegrees;
            double latitudeScaler = Math.Log(Math.Tan(piFourths + (0.5 * latitudeRadians)));
            double latitudePixels = pixelScale * basePixelsPerScalerLatitude * latitudeScaler;

            double longitudePixels = pixelScale * basePixelsPerDegreeLongitude * (double)longitudeDegrees;

            return new Point(longitudePixels, latitudePixels);
        }

        private static Coordinate LocationOfPixel(double x, double y, int zoomLevel)
        {
            double pixelScale = Math.Pow(2.0, (double)zoomLevel - baseZoomLevel);

            // TODO Clean this up
            decimal latitude = (decimal)(radiansToDegrees * (2 * (-piFourths + Math.Atan(Math.Pow(Math.E, (y / (pixelScale * basePixelsPerScalerLatitude)))))));
            decimal longitude = (decimal)(x / pixelScale / basePixelsPerDegreeLongitude);

            return new Coordinate(latitude, longitude);
        }

        private static decimal DBCoordinateAlignment(int zoomLevel)
        {
            return (decimal)Math.Pow(2, baseZoomLevel - zoomLevel);
        }

        private static Coordinate AlignDBCoordinate(Coordinate input, int zoomLevel)
        {
            decimal alignTo = DBCoordinateAlignment(zoomLevel);
            Func<decimal, decimal> align = x => Math.Round(x / alignTo) * alignTo;
            return new Coordinate(align(input.Latitude), align(input.Longitude));
        }

        private void AddChildAt(decimal lat, decimal lon)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            const double sourceMeasureResolution = 96;
            Coordinate coord = new Coordinate(lat, lon);

            this.DownloadsActive += 1;
            Task<ImageSource> aSource = ImageSource.GetImageAsync(new MapImageSpec(coord, MapType, new Dimension(640, 640), ZoomLevel));
            Point centerLocation = PixelLocationOf(lat, lon, ZoomLevel);

            Image child = new Image();
            child.Tag = coord;
            this.Picture.Children.Add(child);
            aSource.ContinueWith((source) =>
            {
                try
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.DownloadsActive -= 1;
                        if (source.Result != null)
                        {
                            child.Source = source.Result;
                            Panel.SetZIndex(child, (int)(100000 * (90 - ((Coordinate)child.Tag).Latitude)));
                            Canvas.SetLeft(child, -Location.X + centerLocation.X + 0.5 * (source.Result.Width / sourceMeasureResolution));
                            Canvas.SetBottom(child, -Location.Y + centerLocation.Y - 0.5 * (source.Result.Height / sourceMeasureResolution));
                        }
                    });
                } catch (TaskCanceledException tce)
                {
                    // Intentionally ignored; thrown when application exits.
                    tce.Equals(tce);
                }
            });
        }

        private void Root_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.MouseIsDown = true;
            this.MousePosition = e.GetPosition(this);
        }

        private void Root_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.MouseIsDown = false;
        }

        private void Root_MouseLeave(object sender, MouseEventArgs e)
        {
            this.MouseIsDown = false;
        }

        private void Root_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseIsDown)
            {
                Point previous = this.MousePosition;
                this.MousePosition = e.GetPosition(this);
                Vector delta = Point.Subtract(previous, this.MousePosition);
                delta.Y *= -1;
                this.PerformScrollBy(delta);
            }
        }

        private void PerformScrollBy(Vector delta)
        {
            this.Location += delta;

            Image[] children = new Image[Picture.Children.Count];
            for (int i = 0; i < children.Length; i += 1)
            {
                Image child = Picture.Children[i] as Image;
                double x = (double)child.GetValue(Canvas.LeftProperty);
                double y = (double)child.GetValue(Canvas.BottomProperty);
                Canvas.SetLeft(child, x - delta.X);
                Canvas.SetBottom(child, y - delta.Y);
                children[i] = child;
            }
            Picture.UpdateLayout();

            Coordinate near = AlignDBCoordinate(LocationOfPixel(Location, ZoomLevel), ZoomLevel);
            decimal alignment = DBCoordinateAlignment(ZoomLevel);

            Func<decimal, decimal, Tuple<decimal, decimal>> pair = (x, y) => new Tuple<decimal, decimal>(x, y);
            Tuple<decimal, decimal>[] coordinateOffsets =
                Enumerable.Range(-1, 2 + (int)this.ActualHeight / 320)
                          .SelectMany(lat => Math.Abs(near.Latitude) > 48 ? new decimal[] { lat, lat - 0.5m } : new decimal[] { lat })
                          .SelectMany(lat => Enumerable.Range(-1, 2 + (int)(this.ActualWidth / basePixelsPerDegreeLongitude)).Select(lon => pair(lat, lon)))
                          .ToArray();

            foreach (Tuple<decimal, decimal> offset in coordinateOffsets)
            {
                Coordinate coord = new Coordinate(near.Latitude + (offset.Item1 * alignment), near.Longitude + (offset.Item2 * alignment));
                if (!children.Any(i => ((Coordinate)i.Tag) == coord))
                {
                    this.AddChildAt(coord.Latitude, coord.Longitude);
                }
            }
        }

        private void ZoomLevelChanged(object sender, RoutedEventArgs e)
        {
            Location = PixelLocationOf(LocationOfPixel(Location, LastZoomLevel), ZoomLevel);
            Picture.Children.RemoveRange(0, Picture.Children.Count);
            PerformScrollBy(new Vector());
            LastZoomLevel = ZoomLevel;
        }

        private void MapTypeChanged(object sender, RoutedEventArgs e)
        {
            Picture.Children.RemoveRange(0, Picture.Children.Count);
            PerformScrollBy(new Vector());
        }

        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.PerformScrollBy(new Vector(0, 0));
        }
    }
}

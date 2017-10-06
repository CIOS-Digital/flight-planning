using CIOSDigital.FlightPlanner.Database;
using CIOSDigital.FlightPlanner.Model;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CIOSDigital.FlightPlanner.View
{
    public partial class WorldMap : UserControl
    {
        public static readonly DependencyProperty ActivePlanProperty =
            DependencyProperty.Register("ActivePlan", typeof(FlightPlan), typeof(WorldMap));
        public FlightPlan ActivePlan {
            get {
                var plan = this.GetValue(ActivePlanProperty) as FlightPlan;
                if (plan == null)
                {
                    plan = new FlightPlan();
                    ActivePlan = plan;
                }
                return plan;
            }
            set => this.SetValue(ActivePlanProperty, value);
        }

        public static DependencyProperty DownloadsActiveProperty =
            DependencyProperty.Register("DownloadsActive", typeof(int), typeof(WorldMap));
        public int DownloadsActive {
            get => (int)this.GetValue(DownloadsActiveProperty);
            set => this.SetValue(DownloadsActiveProperty, (int)value);
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
        private Point CenterLocation {
            get {
                return new Point(Location.X + this.Width / 2, Location.Y + this.Height / 2);
            }
        }

        private IMapProvider ImageSource { get; set; }

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

            Coordinate coord = new Coordinate(lat, lon);

            this.DownloadsActive += 1;
            Task<ImageSource> aSource = ImageSource.GetImageAsync(new TileSpecifier(coord, MapType, new Size(640, 640), ZoomLevel));
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
                            Panel.SetZIndex(child, (int)(-1000 * ((Coordinate)child.Tag).Latitude));
                            double dx = 0.5 * source.Result.Width;
                            double dy = 0.5 * source.Result.Height;
                            Canvas.SetLeft(child, -Location.X + centerLocation.X - dx);
                            Canvas.SetBottom(child, -Location.Y + centerLocation.Y - dy);
                        }
                    });
                }
                catch (TaskCanceledException tce)
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

            for (int i = 0; i < Picture.Children.Count; i += 1)
            {
                UIElement child = Picture.Children[i];
                if (child is Line)
                {
                    Line l = (Line)child;
                    l.X1 -= delta.X;
                    l.X2 -= delta.X;
                    l.Y1 += delta.Y;
                    l.Y2 += delta.Y;
                }
                else
                {
                    double x = (double)child.GetValue(Canvas.LeftProperty);
                    double y = (double)child.GetValue(Canvas.BottomProperty);
                    Canvas.SetLeft(child, x - delta.X);
                    Canvas.SetBottom(child, y - delta.Y);
                }
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

            Image[] mapImages = Picture.Children.OfType<Image>().ToArray();
            foreach (Tuple<decimal, decimal> offset in coordinateOffsets)
            {
                Coordinate coord = new Coordinate(near.Latitude + (offset.Item1 * alignment), near.Longitude + (offset.Item2 * alignment));
                if (!mapImages.Any(i => ((Coordinate)i.Tag) == coord))
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
            RefreshWaypoints();
            LastZoomLevel = ZoomLevel;
        }

        private void MapTypeChanged(object sender, RoutedEventArgs e)
        {
            {
                Image[] images = Picture.Children.OfType<Image>().ToArray();
                foreach (Image i in images)
                {
                    Picture.Children.Remove(i);
                }
            }
            PerformScrollBy(new Vector());
        }

        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            this.PerformScrollBy(new Vector((e.PreviousSize.Width - e.NewSize.Width) / 2, (e.PreviousSize.Height - e.NewSize.Height) / 2));
            RefreshWaypoints();
        }

        public void RefreshWaypoints()
        {
            WorldMapHandle[] handles = Picture.Children.OfType<WorldMapHandle>().ToArray();
            foreach (UIElement h in handles)
            {
                Picture.Children.Remove(h);
            }
            Line[] lines = Picture.Children.OfType<Line>().ToArray();
            foreach (UIElement l in lines)
            {
                Picture.Children.Remove(l);
            }

            WorldMapHandle previous = null;
            foreach (Coordinate c in ActivePlan)
            {
                Point p = PixelLocationOf(c, ZoomLevel);
                WorldMapHandle h = new WorldMapHandle();
                Picture.Children.Add(h);
                Panel.SetZIndex(h, 500);
                double x = -Location.X + p.X - h.Width * 0.5;
                double y = -Location.Y + p.Y;
                Canvas.SetLeft(h, x);
                Canvas.SetBottom(h, y);
                if (previous != null)
                {
                    Line l = new Line();
                    Picture.Children.Add(l);
                    l.Stroke = new SolidColorBrush(Color.FromArgb(0xA0, 0xFF, 0x0, 0x0));
                    l.StrokeThickness = 4;
                    l.StrokeStartLineCap = PenLineCap.Round;
                    l.StrokeEndLineCap = PenLineCap.Round;
                    l.X1 = previous.Width / 2 + (double)previous.GetValue(Canvas.LeftProperty);
                    l.Y1 = Picture.ActualHeight - (double)previous.GetValue(Canvas.BottomProperty);
                    l.X2 = h.Width / 2 + (double)h.GetValue(Canvas.LeftProperty);
                    l.Y2 = Picture.ActualHeight - (double)h.GetValue(Canvas.BottomProperty);
                    Console.WriteLine("{0},{1} to {2},{3}", l.X1, l.Y1, l.X2, l.Y2);
                }
                previous = h;
            }
            UpdateLayout();
        }
    }
}

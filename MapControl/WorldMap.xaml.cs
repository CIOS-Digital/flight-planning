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

namespace CIOSDigital.MapControl
{
    /// <summary>
    /// Interaction logic for WorldMap.xaml
    /// </summary>
    public partial class WorldMap : UserControl
    {
        private static Coordinate Seattle = new Coordinate(47.62m, -122.35m);

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
        private Coordinate Coordinates { get; set; }

        private MapProvider ImageSource { get; set; }
        private Plan ActivePlan { get; set; }

        private bool MouseIsDown { get; set; }
        private Point MousePosition { get; set; }

        public WorldMap()
        {
            this.MouseIsDown = false;
            this.MousePosition = new Point(0, 0);
            this.Coordinates = WorldMap.Seattle;

            this.ImageSource = SQLiteMap.OpenDB();
            InitializeComponent();
            for (int lat = 50; lat >= 45; lat -= 1)
            {
                for (int lon = -125; lon <= -120; lon += 1)
                {
                    AddChildAt(lat, lon);
                }
            }
            Point translateBy = PixelLocationOf(this.Coordinates, 9);
            PerformScrollBy(new Vector(translateBy.X, -translateBy.Y));
        }

        private static Point PixelLocationOf(Coordinate c, int zoomLevel)
        {
            return PixelLocationOf(c.Latitude, c.Longitude, zoomLevel);
        }

        private static Point PixelLocationOf(decimal latitudeDegrees, decimal longitudeDegrees, int zoomLevel)
        {
            const double basePixelsPerScalerLatitude = 58.0 * 359.0;
            const double basePixelsPerDegreeLongitude = 364.0;
            const double baseZoomLevel = 9.0;
            const double degreesToRadians = Math.PI / 180.0;
            const double piFourths = Math.PI / 4.0;

            double pixelScale = Math.Pow(2.0, (double)zoomLevel - baseZoomLevel);

            double latitudeRadians = degreesToRadians * (double)latitudeDegrees;
            double latitudeScaler = Math.Log(Math.Tan(piFourths + (0.5 * latitudeRadians)));
            double latitudePixels = pixelScale * basePixelsPerScalerLatitude * latitudeScaler;

            double longitudePixels = pixelScale * basePixelsPerDegreeLongitude * (double)longitudeDegrees;
            
            return new Point(longitudePixels, latitudePixels);
        }

        private void AddChildAt(decimal lat, decimal lon)
        {
            const double sourceMeasureResolution = 96;

            ImageSource source = ImageSource.GetImage(new MapImageSpec(new Coordinate(lat, lon), MapType.RoadMap, new Dimension(640, 640), 9));
            if (source == null) { return; }

            Point centerLocation = PixelLocationOf(lat, lon, 9);

            Image child = new Image();
            child.Source = source;
            this.Picture.Children.Add(child);
            Canvas.SetLeft(child, centerLocation.X + 0.5 * (source.Width / sourceMeasureResolution));
            Canvas.SetBottom(child, centerLocation.Y - 0.5 * (source.Height / sourceMeasureResolution));
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
                this.PerformScrollBy(delta);
            }
        }

        private void PerformScrollBy(Vector delta)
        {
            foreach (UIElement child in this.Picture.Children)
            {
                double x = (double)child.GetValue(Canvas.LeftProperty);
                double y = (double)child.GetValue(Canvas.BottomProperty);
                Canvas.SetLeft(child, x - delta.X);
                Canvas.SetBottom(child, y + delta.Y);
            }
            Picture.UpdateLayout();
        }
    }
}

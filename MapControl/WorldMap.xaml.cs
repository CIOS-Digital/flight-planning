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
        private decimal ZoomLevel {
            get {
                return this.ZoomSelector.ZoomLevel;
            }
        }
        private MapType MapType {
            get {
                return this.TypeSelector.MapType;
            }
        }
        private Point Coordinates { get; set; }

        private MapProvider ImageSource { get; set; }
        private Plan ActivePlan { get; set; }

        private bool MouseIsDown { get; set; }
        private Point MousePosition { get; set; }

        public WorldMap()
        {
            this.MouseIsDown = false;
            this.MousePosition = new Point(0, 0);
            this.Coordinates = new Point(47.62, -122.35);
            this.ImageSource = new FileFolderMap();
            InitializeComponent();
            for (int lat = 50; lat >= 20; lat -= 1)
            {
                for (int lon = -125; lon <= -65; lon += 1)
                {
                    AddChildAt(lat, lon);
                }
            }
            this.PerformScrollBy(new Vector(0, -19000));
        }

        private void AddChildAt(decimal lat, decimal lon)
        {
            Image child = new Image();
            ImageSource source = ImageSource.GetImage(new MapImageSpec(new Coordinate(lat, lon), MapType.RoadMap, new Dimension(640, 640), 9.0m));
            if (source == null) { return; }
            child.Source = source;
            this.Picture.Children.Add(child);
            Canvas.SetLeft(child, (int)(lon + 125) * 364);
            double rad = (double)lat * Math.PI / 180;
            double bottom = (double)Math.Log(Math.Tan((Math.PI / 4) + (rad / 2)));
            Canvas.SetBottom(child, (int)(bottom * 58 * 359));
            Console.WriteLine("Added child at {0}, {1} -> {2}, {3}", lat, lon, child.GetValue(Canvas.LeftProperty), child.GetValue(Canvas.BottomProperty));
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

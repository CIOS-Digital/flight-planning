using CIOSDigital.FlightPlanner.Model;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Geometry = Esri.ArcGISRuntime.Geometry.Geometry;
using System.Collections.Generic;
using Esri.ArcGISRuntime.Data;

namespace CIOSDigital.FlightPlanner.View
{
    public partial class WorldMap : UserControl
    {
        public Coordinate mouseCoord { get; set; }
        public Coordinate popupLoc { get; set; }
        public GridOptions gridOptions { get; set; }
        private Dictionary<string, Airport> AirportDict;
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

        public static DependencyProperty MouseCoordinateProperty =
            DependencyProperty.Register("MouseCoord", typeof(Coordinate), typeof(WorldMap));
        public Coordinate MouseCoord {
            get => (Coordinate)this.GetValue(MouseCoordinateProperty);
            set => this.SetValue(MouseCoordinateProperty, (Coordinate)value);
        }

        private static Coordinate Seattle = new Coordinate(47.62, -122.35);

        private MapType MapType => TypeSelector.MapType;

        private Point Location { get; set; }

        private Point MousePosition { get; set; }

        private GraphicsOverlay _sketchOverlay;
        private GraphicsOverlay _airportOverlay;

        public WorldMap()
        {
            InitializeComponent();
            //this.MousePosition = new Point(0, 0);
            this.Location = new Point(0, 0);

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.gridOptions = new GridOptions(true, true, LatitudeLongitudeFormat.DecimalDegrees.ToString());


            Map myMap = new Map(BasemapType.StreetsVector, Seattle.Latitude, Seattle.Longitude, 15);
            myMap = new Map(Basemap.CreateDarkGrayCanvasVector());
            // Assign the map to the MapView
            Esri.ArcGISRuntime.UI.Grid grid;
            grid = new LatitudeLongitudeGrid();
            // Apply the label format setting.
            ((LatitudeLongitudeGrid)grid).LabelFormat = (LatitudeLongitudeGridLabelFormat)Enum.Parse(typeof(LatitudeLongitudeGridLabelFormat), gridOptions.LatLongType);

            // Next, apply the label visibility setting.
            grid.IsLabelVisible = true;
            grid.IsVisible = true;

            // Next, apply the grid color and label color settings for each zoom level.
            for (long level = 0; level < grid.LevelCount; level++)
            {
                // Set the line symbol.
                Symbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid,
                    System.Drawing.Color.Red, 2);
                grid.SetLineSymbol(level, lineSymbol);

                // Set the text symbol.
                Symbol textSymbol = new TextSymbol
                {
                    Color = System.Drawing.Color.Red,
                    OutlineColor = System.Drawing.Color.Red,
                    Size = 16,
                    HaloColor = System.Drawing.Color.White,
                    HaloWidth = 2
                };
                grid.SetTextSymbol(level, textSymbol);
            }

            // Create graphics overlay to display sketch geometry
            _sketchOverlay = new GraphicsOverlay();
            _airportOverlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(_sketchOverlay);
            MyMapView.GraphicsOverlays.Add(_airportOverlay);
            AirportDict = Airport.GetAirpotDict();
            DisplayAirports();


            MyMapView.Map = myMap;
            MyMapView.Grid = grid;
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
            // Set the sketch editor as the page's data context
            DataContext = MyMapView.SketchEditor;
        }

        private bool isDrawing = false;

        private async void toggleDraw_Click(object sender, RoutedEventArgs e)
        {
            if (!isDrawing)
            {
                try
                {
                    if (!isDrawing)
                    {
                        isDrawing = true;
                        // Let the user draw on the map view using the chosen sketch mode
                        SketchCreationMode creationMode = SketchCreationMode.Polyline;
                        Geometry geometry = await MyMapView.SketchEditor.StartAsync(creationMode, true);
                        // Create and add a graphic from the geometry the user drew
                        Graphic graphic = CreateGraphic(geometry, GeometryType.Polyline);
                        var projectedLocation = ((Polyline)geometry).Parts;
                        _sketchOverlay.Graphics.Add(graphic);
                        _sketchOverlay.Graphics.Add(CreateGraphic(geometry, GeometryType.Multipoint));
                        foreach(var point in projectedLocation.SelectMany(x => x.Points))
                        {
                            Geometry myGeometry = GeometryEngine.Project(point, SpatialReferences.Wgs84);

                            // Convert to geometry to a traditional Lat/Long map point
                            AddWaypoint((MapPoint)myGeometry);
                        }

                    }
                }
                catch (TaskCanceledException)
                {

                }
                catch (Exception ex)
                {
                    // Report exceptions
                    MessageBox.Show("Error drawing graphic shape: " + ex.Message);
                }
            }
            else
            {
                if (MyMapView.SketchEditor.CompleteCommand.CanExecute(null))
                {
                    MyMapView.SketchEditor.CompleteCommand.Execute(null);
                    isDrawing = false;
                }
            }
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Get the user-tapped location
            MapPoint mapLocation = e.Location;

            // Project the user-tapped map point location to a geometry
            Esri.ArcGISRuntime.Geometry.Geometry myGeometry = GeometryEngine.Project(mapLocation, SpatialReferences.Wgs84);

            // Convert to geometry to a traditional Lat/Long map point
            MapPoint projectedLocation = (MapPoint)myGeometry;

            // Format the display callout string based upon the projected map point (example: "Lat: 100.123, Long: 100.234")
            string mapLocationDescription = string.Format("Lat: {0:F3} Long:{1:F3}", projectedLocation.Y, projectedLocation.X);

            // Create a new callout definition using the formatted string
            CalloutDefinition myCalloutDefinition = new CalloutDefinition("Location:", mapLocationDescription);

            // Display the callout
            // MyMapView.ShowCalloutAt(mapLocation, myCalloutDefinition);
            double tolerance = 10d; // Use larger tolerance for touch
            int maximumResults = 1; // Only return one graphic  
            bool onlyReturnPopups = false; // Return more than popups
            bool touchedVertix = false;

            try
            {
                // Use the following method to identify graphics in a specific graphics overlay
                IdentifyGraphicsOverlayResult identifyResults = await MyMapView.IdentifyGraphicsOverlayAsync(
                    _airportOverlay,
                    e.Position,
                    tolerance,
                    onlyReturnPopups,
                    maximumResults);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }


            //if (isDrawing && !touchedVertix)
            //{
            //    AddWaypoint(projectedLocation);
            //}
        }

        private void MapTypeChanged(object sender, RoutedEventArgs e)
        {
            switch(MapType){
                case MapType.RoadMap:
                    MyMapView.Map.Basemap = Basemap.CreateNavigationVector();
                    return;
                case MapType.Terrain:
                    MyMapView.Map.Basemap = Basemap.CreateTopographicVector();
                    return;
                case MapType.Hybrid:
                    MyMapView.Map.Basemap = Basemap.CreateTerrainWithLabelsVector();
                    return;
                case MapType.Satellite:
                    MyMapView.Map.Basemap = Basemap.CreateImagery();
                    return;
            }
        }

        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {

          //  this.PerformScrollBy(new Vector((e.PreviousSize.Width - e.NewSize.Width) / 2, (e.PreviousSize.Height - e.NewSize.Height) / 2));
           // RefreshWaypoints();
        }

        public void RefreshGrid()
        {
            Esri.ArcGISRuntime.UI.Grid grid;
            grid = MyMapView.Grid;
            // Apply the label format setting.
            ((LatitudeLongitudeGrid)grid).LabelFormat = (LatitudeLongitudeGridLabelFormat)Enum.Parse(typeof(LatitudeLongitudeGridLabelFormat), gridOptions.LatLongType);
            // Next, apply the label visibility setting.
            grid.IsLabelVisible = gridOptions.GridLabelsEnabled;
            grid.IsVisible = gridOptions.GridEnabled;

            MyMapView.Grid = grid;
        }

        public void DisplayAirports()
        {
            List<Airport> airports = Airport.GetAirports();
            var mapPoints = airports.Select(x => new MapPoint(x.Longitude, x.Latitude)).ToList();
            PointCollection points = new PointCollection(SpatialReferences.Wgs84);
            points.AddPoints(mapPoints);
            var symbol = new SimpleMarkerSymbol()
            {
                Color = System.Drawing.Color.Purple,
                Style = SimpleMarkerSymbolStyle.Circle,
                Size = 5d
            };
            _airportOverlay.Graphics.Add(new Graphic(new Multipoint(points), symbol));
        }

        public void RefreshWaypoints()
        {
            _sketchOverlay.Graphics.Clear();
            // Create a purple simple line symbol
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 5d);
            SimpleMarkerSymbol markerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Blue, 15d);

            // Create a new point collection for polyline
            var mapPoints = ActivePlan.Select(point => new MapPoint(point.coordinate.Longitude, point.coordinate.Latitude)).ToList();

            Esri.ArcGISRuntime.Geometry.PointCollection points = new PointCollection(SpatialReferences.Wgs84);
            points.AddPoints(mapPoints);

            // Add graphic to the graphics overlay

            _sketchOverlay.Graphics.Add(new Graphic(new Multipoint(points), markerSymbol));
            _sketchOverlay.Graphics.Add(new Graphic(new Polyline(points), lineSymbol));
        }

        private Graphic CreateGraphic(Geometry geometry, GeometryType type)
        {
            // Create a graphic to display the specified geometry
            Symbol symbol = null;
            switch (type)
            {
                // Symbolize with a fill symbol
                case GeometryType.Envelope:
                case GeometryType.Polygon:
                    {
                        symbol = new SimpleFillSymbol()
                        {
                            Color = System.Drawing.Color.Red,
                            Style = SimpleFillSymbolStyle.Solid
                        };
                        break;
                    }
                // Symbolize with a line symbol
                case GeometryType.Polyline:
                    {
                        symbol = new SimpleLineSymbol()
                        {
                            Color = System.Drawing.Color.Red,
                            Style = SimpleLineSymbolStyle.Solid,
                            Width = 5d
                        };
                        break;
                    }
                // Symbolize with a marker symbol
                case GeometryType.Point:
                case GeometryType.Multipoint:
                    {

                        symbol = new SimpleMarkerSymbol()
                        {
                            Color = System.Drawing.Color.Blue,
                            Style = SimpleMarkerSymbolStyle.Circle,
                            Size = 15d
                        };
                        break;
                    }
            }

            // pass back a new graphic with the appropriate symbol
            return new Graphic(geometry, symbol);
        }

        Point distance(Coordinate one, Coordinate two)
        {
            Point d = new Point(Math.Abs(one.Latitude - two.Latitude), one.Latitude - two.Latitude);
            return d;
        }

        private void AddWaypoint(MapPoint point, string id = null)
        {
            if (String.IsNullOrEmpty(id))
            {
                var dialog = new PopupText();
                dialog.okButton.Content = "Add";
                Coordinate pos = new Coordinate(point.Y, point.X);
                dialog.LatitudeInput.Text = pos.dmsLatitude;
                dialog.LongitudeInput.Text = pos.dmsLongitude;
                dialog.IDInput.Text = this.ActivePlan.counter.ToString();
                if (dialog.ShowDialog() == true)
                {
                    this.ActivePlan.AppendWaypoint(new Waypoint(dialog.IDInput.Text, pos));
                    if (dialog.IDInput.Text.Equals(this.ActivePlan.counter.ToString()))
                        this.ActivePlan.counter++;
                }
            }
            else
            {
                this.ActivePlan.AppendWaypoint(new Waypoint("W_" + id, popupLoc));
            }
          //  RefreshWaypoints();
        }

        private void AddWaypoint(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                var dialog = new PopupText();
                dialog.okButton.Content = "Add";
                Coordinate pos = new Coordinate(mouseCoord);
                dialog.LatitudeInput.Text = pos.dmsLatitude;
                dialog.LongitudeInput.Text = pos.dmsLongitude;
                dialog.IDInput.Text = this.ActivePlan.counter.ToString();
                if (dialog.ShowDialog() == true)
                {
                    this.ActivePlan.AppendWaypoint(new Waypoint(dialog.IDInput.Text, popupLoc));
                    if (dialog.IDInput.Text.Equals(this.ActivePlan.counter.ToString()))
                        this.ActivePlan.counter++;
                }
            }
            else
            {
                this.ActivePlan.AppendWaypoint(new Waypoint("W_" + id, popupLoc));
            }
           // RefreshWaypoints();
        }

        private void DeleteWaypoint(Waypoint w)
        {
            this.ActivePlan.RemoveWaypoint(w);
            RefreshWaypoints();
        }
    }
}

namespace CIOSDigital.FlightPlanner.Model
{
    public class GridOptions
    {
        public bool GridEnabled;
        public bool GridLabelsEnabled;
        public string LatLongType;
        public GridOptions(bool gridEnabled, bool gridLabelsEnabled, string latLongType)
        {
            GridEnabled = gridEnabled;
            GridLabelsEnabled = gridLabelsEnabled;
            LatLongType = latLongType;
        }
    }
}

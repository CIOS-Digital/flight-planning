namespace CIOSDigital.FlightPlanner.Model
{
    public struct Coordinate
    {
        public decimal Latitude { get; }
        public decimal Longitude { get; }
        public string dmsLatitude { get; }
        public string dmsLongitude { get; }

        public Coordinate(Coordinate coord)
        {
            this.Latitude = coord.Latitude;
            this.Longitude = coord.Longitude;
            int d, m, s;
            d = (int)coord.Latitude;
            m = (int)((coord.Latitude - d) * 60);
            s = (int)((coord.Latitude - d - m / 60) * 3600);
            this.dmsLatitude=  string.Format("{0}°{1}'{2}\"", d, m, s);
            d = (int)coord.Longitude;
            m = (int)((coord.Longitude - d) * 60);
            s = (int)((coord.Longitude - d - m / 60) * 3600);
            this.dmsLongitude = string.Format("{0}°{1}'{2}\"", d, m, s);
        }

        public Coordinate(decimal latitude, decimal longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            int d, m, s;
            d = (int)latitude;
            m = (int)((latitude - d) * 60);
            s = (int)((latitude - d - m / 60) * 3600);
            this.dmsLatitude = string.Format("{0}°{1}'{2}\"", d, m, s);
            d = (int)longitude;
            m = (int)((longitude - d) * 60);
            s = (int)((longitude - d - m / 60) * 3600);
            this.dmsLongitude = string.Format("{0}°{1}'{2}\"", d, m, s);
        }

        public override bool Equals(object obj)
        {
            return (obj as Coordinate? != null)
                ? this.Equals((Coordinate)obj)
                : false;
        }

        public bool Equals(Coordinate other)
        {
            return this.Latitude == other.Latitude
                && this.Longitude == other.Longitude;
        }

        public override int GetHashCode()
        {
            return this.Latitude.GetHashCode() ^ this.Longitude.GetHashCode();
        }

        public static bool operator ==(Coordinate left, Coordinate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Coordinate left, Coordinate right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return decimalToDMS(Latitude) + " " + decimalToDMS(Longitude);
        }

        private string decimalToDMS(decimal degree)
        {
            int d, m, s;
            d = (int)degree;
            m = (int)((degree - d) * 60);
            s = (int)((degree - d - m / 60) * 3600);
            return string.Format("{0}°{1}'{2}\"", d, m, s);
        }
    }
}

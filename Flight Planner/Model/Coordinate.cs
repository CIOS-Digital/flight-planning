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
            string sign;
            d = (int)coord.Latitude;
            m = (int)((coord.Latitude - d) * 60);
            s = (int)((coord.Latitude - d - m / 60) * 3600);
            sign = (d < 0) ? "S" : "N";
            this.dmsLatitude= string.Format("{0}°{1}'{2}\"{3}", System.Math.Abs(d), System.Math.Abs(m), System.Math.Abs(s), sign);

            d = (int)coord.Longitude;
            m = (int)((coord.Longitude - d) * 60);
            s = (int)((coord.Longitude - d - m / 60) * 3600);
            sign = (d < 0) ? "W" : "E";
            this.dmsLongitude = string.Format("{0}°{1}'{2}\"{3}", System.Math.Abs(d), System.Math.Abs(m), System.Math.Abs(s), sign);
        }

        public Coordinate(decimal latitude, decimal longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            int d, m, s;
            string sign;
            d = (int)latitude;
            m = (int)((latitude - d) * 60);
            s = (int)((latitude - d - m / 60) * 3600);
            sign = (d < 0) ? "S" : "N";
            this.dmsLatitude = string.Format("{0}°{1}'{2}\"{3}", System.Math.Abs(d), System.Math.Abs(m), System.Math.Abs(s), sign);
            d = (int)longitude;
            m = (int)((longitude - d) * 60);
            s = (int)((longitude - d - m / 60) * 3600);
            sign = (d < 0) ? "W" : "E";
            this.dmsLongitude = string.Format("{0}°{1}'{2}\"{3}", System.Math.Abs(d), System.Math.Abs(m), System.Math.Abs(s), sign);
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
            return decimalToDMS(Latitude, Longitude);
        }

        private string decimalToDMS(decimal latitude, decimal longitude)
        {
            int d, m, s;
            string sign;
            d = (int)latitude;
            m = (int)((latitude - d) * 60);
            s = (int)((latitude - d - m / 60) * 3600);
            sign = (d < 0) ? "S" : "N";
            string lat = string.Format("{0}°{1}'{2}\"{3}", System.Math.Abs(d), System.Math.Abs(m), System.Math.Abs(s), sign);
            d = (int)longitude;
            m = (int)((longitude - d) * 60);
            s = (int)((longitude - d - m / 60) * 3600);
            sign = (d < 0) ? "W" : "E";
            string longi = string.Format("{0}°{1}'{2}\"{3}", System.Math.Abs(d), System.Math.Abs(m), System.Math.Abs(s), sign);
            return lat + " " + longi;
        }
    }
}

namespace CIOSDigital.FlightPlanner.Model
{
    public struct Coordinate
    {
        private const int precision = 1000000;
        public decimal Latitude { get; }
        public decimal Longitude { get; }
        public string dmsLatitude { get; }
        public string dmsLongitude { get; }

        public Coordinate(Coordinate coord)
        {
            this.Latitude = System.Math.Truncate(coord.Latitude * precision) / precision;
            this.Longitude = System.Math.Truncate(coord.Longitude * precision) / precision;
            int d;
            double m;
            string sign;
            d = (int)coord.Latitude;
            m = (double)(coord.Latitude - d) * 60;
            sign = (d < 0) ? "S" : "N";
            this.dmsLatitude = string.Format("{0}°{1:0.00}'{2}", System.Math.Abs(d), System.Math.Abs(m), sign);

            d = (int)coord.Longitude;
            m = (double)(coord.Longitude - d) * 60;
            sign = (d < 0) ? "W" : "E";
            this.dmsLongitude = string.Format("{0}°{1:0.00}'{2}", System.Math.Abs(d), System.Math.Abs(m), sign);
        }

        public Coordinate(decimal latitude, decimal longitude)
        {
            this.Latitude = System.Math.Truncate(latitude * precision) / precision;
            this.Longitude = System.Math.Truncate(longitude * precision) / precision;
            int d;
            double m;
            string sign;
            d = (int)latitude;
            m = (double)(latitude - d) * 60;
            sign = (d < 0) ? "S" : "N";
            this.dmsLatitude = string.Format("{0}°{1:0.00}'{2}", System.Math.Abs(d), System.Math.Abs(m), sign);
            d = (int)longitude;
            m = (double)(longitude - d) * 60;
            sign = (d < 0) ? "W" : "E";
            this.dmsLongitude = string.Format("{0}°{1:0.00}'{2}", System.Math.Abs(d), System.Math.Abs(m), sign);
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
            int d;
            double m;
            string sign;
            d = (int)latitude;
            m = (double)(latitude - d)*60;
            sign = (d < 0) ? "S" : "N";
            string lat = string.Format("{0}°{1:0.00}'{2}", System.Math.Abs(d), System.Math.Abs(m), sign);
            d = (int)longitude;
            m = (double)(longitude - d) * 60;
            sign = (d < 0) ? "W" : "E";
            string longi = string.Format("{0}°{1:0.00}'{2}", System.Math.Abs(d), System.Math.Abs(m), sign);
            return lat + " " + longi;
        }
    }
}

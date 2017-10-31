namespace CIOSDigital.FlightPlanner.Model
{
    public struct Coordinate
    {
        public decimal Latitude { get; }
        public decimal Longitude { get; }

        public Coordinate(Coordinate coord)
        {
            this.Latitude = coord.Latitude;
            this.Longitude = coord.Longitude;
        }

        public Coordinate(decimal latitude, decimal longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
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
            return string.Format("{0}N, {1}E", Latitude, Longitude);
        }
    }
}

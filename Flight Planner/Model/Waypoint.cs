namespace CIOSDigital.FlightPlanner.Model
{
    public struct Waypoint
    {
        public Coordinate coordinate { get; }
        public string id { get; }

        public Waypoint(string id, Coordinate coord)
        {
            this.id = id;
            this.coordinate = coord;
        }

        public bool Equals(Waypoint other)
        {
            return this.id == other.id
                && this.coordinate.Equals(other.coordinate);
        }

        public override string ToString()
        {
            return id + ": " + this.coordinate;
        }
    }



}

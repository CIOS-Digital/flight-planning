using CIOSDigital.FlightPlanner.Model;
using System.Windows;

namespace CIOSDigital.FlightPlanner.Database
{
    public struct TileSpecifier
    {
        public readonly Coordinate Coordinate;
        public readonly MapType MapType;
        public readonly Size Size;
        public readonly int Zoom;

        public TileSpecifier(Coordinate coordinate, MapType type, Size size, int zoom)
        {
            this.Coordinate = coordinate;
            this.MapType = type;
            this.Size = size;
            this.Zoom = zoom;
        }
    }
}

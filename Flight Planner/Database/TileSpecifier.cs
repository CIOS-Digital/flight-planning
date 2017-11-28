using CIOSDigital.FlightPlanner.Model;
using System;
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

        public bool IsValidCoordinate()
        {
            return Math.Abs(Coordinate.Latitude) < 70 && Math.Abs(Coordinate.Longitude) < 180;
        }

        public override bool Equals(object obj)
        {
            return obj is TileSpecifier other && this == other;
        }

        public override int GetHashCode()
        {
            // TODO: This isn't the "correct" way to merge hashes, but it's good enough for now.
            return Coordinate.GetHashCode() ^ MapType.GetHashCode() ^ Size.GetHashCode() ^ Zoom.GetHashCode();
        }

        public static bool operator==(TileSpecifier left, TileSpecifier right)
        {
            return left.Coordinate == right.Coordinate
                && left.MapType == right.MapType
                && left.Size == right.Size
                && left.Zoom == right.Zoom;
        }

        public static bool operator!=(TileSpecifier left, TileSpecifier right)
        {
            return left.Coordinate != right.Coordinate
                || left.MapType != right.MapType
                || left.Size != right.Size
                || left.Zoom != right.Zoom;
        }
    }
}

using System.Collections.Generic;

namespace BonVoyage
{
    internal class Hex : IHasNeighbours<Hex>
    {
        internal double Latitude { get; }
        internal double Longitude { get; }
        internal double Altitude { get; }
        internal double Bearing { get; }
        internal int X { get; }
        internal int Y { get; }

        private PathFinder Parent;

        internal Hex(double latitude, double longitude, double altitude, double bearing, int x, int y, PathFinder parent)
        {
            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
            Bearing = bearing;
            X = x;
            Y = y;
            Parent = parent;
        }

        public IEnumerable<Hex> Neighbours
        {
            get
            {
                return Parent.GetNeighbours(X, Y);
            }
        }

    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CIOSDigital
{
    public struct Coordinate
    {
        public decimal Latitude { get; }
        public decimal Longitude { get; }

        public Coordinate(decimal latitude, decimal longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
        }
    }
}

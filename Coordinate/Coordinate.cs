using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIOSDigital
{
    public struct Coordinate
    {
        public readonly decimal Latitude;
        public readonly decimal Longitude;
        public string ident;
        public string type;
        public string country;
        public string comment;

        public Coordinate(decimal latitude, decimal longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.ident = "";
            this.type = "";
            this.country = "";
            this.comment = "";
        }
    }
}

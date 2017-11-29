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
            this = new Coordinate(coord.Latitude, coord.Longitude);
        }

        public Coordinate(string latitude, string longitude)
        {
            this.dmsLatitude = latitude.Replace("\'", "");
            this.dmsLongitude = longitude.Replace("\'", "");
            this.Latitude = 0;
            this.Longitude = 0;
            decimal lat = strtodecLattitude(latitude);
            decimal longi = strtodecLongitude(longitude);
            this = new Coordinate(lat, longi);
        }


            public Coordinate(decimal latitude, decimal longitude)
        {
            bool PNW = true;
            this.Latitude = 0;
            this.Longitude = 0;
            if (PNW) {
                this.Latitude = System.Math.Abs(System.Math.Truncate(latitude * precision) / precision);
                this.Longitude = System.Math.Abs(System.Math.Truncate(longitude * precision) / precision) * -1;
            }
            this.dmsLatitude = "";
            this.dmsLongitude = "";
            this.dmsLatitude = dectostringLatitude(this.Latitude);
            this.dmsLongitude = dectostringLongitude(this.Longitude);
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
            return dmsLatitude + " " + dmsLongitude;
        }

        private string decimalToDMS(decimal latitude, decimal longitude)
        {
            return dectostringLatitude(latitude) + " " + dectostringLongitude(longitude);
        }

        private string dectostringLatitude(decimal lat)
        {
            int d;
            double m;
            string sign;
            d = (int)lat;
            m = (double)(lat - d) * 60;
            sign = (d < 0) ? "S" : "N";
            return string.Format("{0}°{1:0.00}'{2}", System.Math.Abs(d), System.Math.Abs(m), sign);
        }

        private string dectostringLongitude(decimal longi)
        {
            int d;
            double m;
            string sign;
            d = (int)longi;
            m = (double)(longi - d) * 60;
            sign = (d < 0) ? "W" : "E";
            return string.Format("{0}°{1:0.00}'{2}", System.Math.Abs(d), System.Math.Abs(m), sign);
        }
        private decimal strtodecLattitude(string lat)
        {
            decimal latitude;
            decimal deg;
            decimal min;
            if (lat.Contains("°"))
            {
                System.Decimal.TryParse(lat.Split('°')[0], out deg);
                System.Decimal.TryParse(lat.Split('°')[1], out min);
            }
            else
            {
                System.Decimal.TryParse(lat, out deg);
                min = 0;
            }
            if ((deg < -90 || deg > 90))
            {
                throw new System.ArgumentOutOfRangeException("lattitude");
            }
            if (deg < 0)
                latitude = deg - min / 60;
            else
                latitude = deg + min / 60;

            return latitude;
        }
        private decimal strtodecLongitude(string longi)
        {
            decimal longitude;
            decimal deg, min;
            if (dmsLongitude.Contains("°"))
            {
                System.Decimal.TryParse(dmsLongitude.Split('°')[0], out deg);
                System.Decimal.TryParse(dmsLongitude.Split('°')[1], out min);
            }
            else
            {
                System.Decimal.TryParse(dmsLongitude, out deg);
                min = 0;
            }
            if ((deg < -180 || deg > 180))
            {
                throw new System.ArgumentOutOfRangeException("longitude");
            }
            if (deg < 0)
                longitude = deg - min / 60;
            else
                longitude = deg + min / 60;

            return longitude;
        }
    }
}

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
            if (PNW)
            {
                this.Latitude = System.Math.Abs(System.Math.Truncate(latitude * precision) / precision);
                this.Longitude = System.Math.Abs(System.Math.Truncate(longitude * precision) / precision) * -1;
            }
            if (this.Latitude < -90 || this.Latitude > 80 || this.Longitude < -180 || this.Longitude > 180)
                throw new System.ArgumentOutOfRangeException("");
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
            if (!lat.Contains("°"))
                lat += "°0";
            if (!lat.Contains("."))
                lat += ".0";
            if (!lat.Contains("'"))
                lat += "'N";
            string[] seperatedString = lat.Split(new char[] { '°', '\'' });
            System.Decimal.TryParse(seperatedString[0], out deg);
            System.Decimal.TryParse(seperatedString[1], out min);
            if ((deg < -90 || deg > 80))
            {
                throw new System.ArgumentOutOfRangeException("latitude");
            }
            latitude = System.Math.Abs(deg) + min / 60;

            if (seperatedString.Length==3 && seperatedString[2] == "S")
                latitude *= -1;

            return latitude;
        }
        private decimal strtodecLongitude(string longi)
        {
            decimal longitude;
            decimal deg, min;
            if (!longi.Contains("°"))
                longi += "°0";
            if (!longi.Contains("."))
                longi += ".0";
            if (!longi.Contains("'"))
                longi += "'W";
            string[] seperatedString = longi.Split(new char[] { '°', '\'' });

            System.Decimal.TryParse(seperatedString[0], out deg);
            System.Decimal.TryParse(seperatedString[1], out min);
            longitude = System.Math.Abs(deg) + min / 60;

            if (seperatedString[2] == "W")
                longitude *= -1;

            if ((deg < -180 || deg > 180))
            {
                throw new System.ArgumentOutOfRangeException("longitude");
            }
    
            return longitude;
        }
    }
}

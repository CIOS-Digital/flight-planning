using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CIOSDigital.FlightPlanner.Model
{
    class Airport
    {
        public string Name;
        public string LocalCode;
        public string InternationalCode;
        public double Latitude;
        public double Longitude;

        public static Airport FromCsv(string csvLine)
        {
            string[] values = csvLine.Split('\t');
            Airport airports = new Airport();
            airports.Name = values[0];
            airports.LocalCode = values[1];
            airports.InternationalCode = values[2];
            airports.Latitude = Convert.ToDouble(values[3]);
            airports.Longitude = Convert.ToDouble(values[4]);

            return airports;
        }

        public static Dictionary<string, Airport> GetAirpotDict()
        {
            var airports = GetAirports();

            //File.ReadAllLines("airports.csv")
            //                       .Select(v => Airport.FromCsv(v))
            //                       .ToDictionary(x => $"{x.Longitude}-{x.Latitude}", x => x);

            var dict = new Dictionary<string, Airport>();
            var dups = new List<string>();
            foreach (var x in airports)
            {
                if (!dict.ContainsKey($"{x.Longitude}-{x.Latitude}"))
                    dict.Add($"{x.Longitude}-{x.Latitude}", x);
                else
                {
                    Console.WriteLine(x.Name);
                    dups.Add(x.Name);
                }
            }



                return dict;

        }

        public static List<Airport> GetAirports()
        {
            return File.ReadAllLines("airports.csv")
                                   .Select(v => Airport.FromCsv(v))
                                   .ToList();
        }
    }
}

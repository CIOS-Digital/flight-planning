using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CIOSDigital.FlightPlan
{
    public class Plan : IEnumerable<Coordinate>
    {
        private readonly List<Coordinate> Waypoints;

        private Plan()
        {
            Waypoints = new List<Coordinate>();
        }

        public static Plan Empty()
        {
            return new Plan();
        }

        public string ToXmlString(uint flightPlanIndex)
        {
            string xmlns = "http://www8.garmin.com/xmlschemas/FlightPlan/v1";

            StringWriter buffer = new StringWriter();
            XmlTextWriter writer = new XmlTextWriter(buffer);
            writer.Formatting = Formatting.Indented;

            writer.WriteStartDocument();
            writer.WriteStartElement("flight-plan", xmlns);
            { // Write created date
                writer.WriteStartElement("created");
                writer.WriteString(string.Format("{0}Z", DateTime.UtcNow.ToString("s")));
                writer.WriteEndElement();
            }

            Func<int, string> getWaypointName = (i) => string.Format("WP{0:0000}", i);
            { // Write table of waypoints
                writer.WriteStartElement("waypoint-table");
                for (int i = 0; i < this.Waypoints.Count; i += 1)
                {
                    Coordinate waypoint = this.Waypoints[i];
                    writer.WriteStartElement("waypoint");
                    writer.WriteElementString("identifier", getWaypointName(i));
                    writer.WriteElementString("type", "USER WAYPOINT");
                    writer.WriteElementString("country-code", "__");
                    writer.WriteElementString("lat", waypoint.Latitude.ToString());
                    writer.WriteElementString("lon", waypoint.Longitude.ToString());
                    writer.WriteElementString("comment", "");
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            { // Write route
                writer.WriteStartElement("route");
                writer.WriteElementString("route-name", "");
                writer.WriteElementString("flight-plan-index", flightPlanIndex.ToString());
                for (int i = 0; i < this.Waypoints.Count; i += 1)
                {
                    Coordinate waypoint = this.Waypoints[i];
                    writer.WriteStartElement("route-point");
                    writer.WriteElementString("waypoint-identifier", getWaypointName(i));
                    writer.WriteElementString("waypoint-type", "USER WAYPOINT");
                    writer.WriteElementString("waypoint-country-code", "__");
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            writer.WriteEndDocument();
            writer.Close();
            return buffer.ToString();
        }

        public void AppendWaypoint(Coordinate c)
        {
            Console.WriteLine("Added waypoint at {0}, {1}", c.Latitude, c.Longitude);
            this.Waypoints.Add(c);
        }

        public IEnumerator<Coordinate> GetEnumerator()
        {
            return ((IEnumerable<Coordinate>)Waypoints).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Coordinate>)Waypoints).GetEnumerator();
        }
    }
}

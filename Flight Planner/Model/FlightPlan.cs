using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Xml;

namespace CIOSDigital.FlightPlanner.Model
{
    public class FlightPlan : IEnumerable<Coordinate>, INotifyCollectionChanged
    {
        private readonly List<Coordinate> Waypoints;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public FlightPlan()
        {
            Waypoints = new List<Coordinate>();
        }

        public void AppendWaypoint(Coordinate c)
        {
            this.Waypoints.Add(c);
            CollectionChanged?.Invoke(Waypoints, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public IEnumerator<Coordinate> GetEnumerator() => Waypoints.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Waypoints.GetEnumerator();

        public void FplWrite(TextWriter writeTo, uint flightPlanIndex)
        {
            string xmlns = "http://www8.garmin.com/xmlschemas/FlightPlan/v1";

            XmlTextWriter writer = new XmlTextWriter(writeTo)
            {
                Formatting = Formatting.Indented,
            };
            
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
        }


        public static FlightPlan FplRead(XmlDocument document)
        {
            FlightPlan plan = new FlightPlan();

            XmlNamespaceManager mgr = new XmlNamespaceManager(document.NameTable);
            mgr.AddNamespace("wpns", document.DocumentElement.NamespaceURI);
            XmlNodeList nodes = document.DocumentElement.SelectNodes("//wpns:waypoint", mgr);

            var idToCoordinate = new Dictionary<string, Coordinate>();
            foreach (XmlNode n in nodes)
            {
                if (Decimal.TryParse(n.SelectSingleNode("wpns:lat", mgr).InnerText, out decimal latitude) 
                    && Decimal.TryParse(n.SelectSingleNode("wpns:lon", mgr).InnerText, out decimal longitude))
                {
                    Coordinate c = new Coordinate(latitude, longitude);
                    idToCoordinate.Add(n.SelectSingleNode("wpns:identifier", mgr).InnerText, c);
                }
            }

            XmlNodeList routes = document.DocumentElement.SelectNodes("//wpns:route-point", mgr);
            foreach (XmlNode n in routes)
            {
                string ident = n.SelectSingleNode("wpns:waypoint-identifier", mgr).InnerText;
                if (idToCoordinate.TryGetValue(ident, out Coordinate c))
                {
                    plan.AppendWaypoint(c);
                }
            }

            return plan;
        }
    }
}

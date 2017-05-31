using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CIOSDigital.FlightPlan
{
    public class Plan : IEnumerable<Coordinate>, INotifyCollectionChanged
    {
        private readonly List<Coordinate> Waypoints;
        
        public event NotifyCollectionChangedEventHandler CollectionChanged;

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
            this.Waypoints.Add(c);
            CollectionChanged?.Invoke(Waypoints, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public IEnumerator<Coordinate> GetEnumerator()
        {
            return ((IEnumerable<Coordinate>)Waypoints).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Coordinate>)Waypoints).GetEnumerator();
        }

        public static Plan XmlLoad(string filename)
        {
            Plan _plan = new FlightPlan.Plan();
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlNamespaceManager mgr = new XmlNamespaceManager(doc.NameTable);
            mgr.AddNamespace("wpns", doc.DocumentElement.NamespaceURI);
            XmlNodeList nodes = doc.DocumentElement.SelectNodes("//wpns:waypoint", mgr);
            Dictionary<string, Coordinate> dic = new Dictionary<string, Coordinate>();
            foreach (XmlNode n in nodes)
            {
                decimal latitude;
                decimal longitude;
                Coordinate c;
                if (Decimal.TryParse(n.SelectSingleNode("wpns:lat", mgr).InnerText, out latitude) && Decimal.TryParse(n.SelectSingleNode("wpns:lon", mgr).InnerText, out longitude))
                {
                    c = new Coordinate(latitude, longitude);
                    // c.ident = n.SelectSingleNode("wpns:identifier", mgr).InnerText;
                    // c.type = n.SelectSingleNode("wpns:type", mgr).InnerText;
                    // c.country = n.SelectSingleNode("wpns:country-code", mgr).InnerText;
                    // c.comment = n.SelectSingleNode("wpns:comment", mgr).InnerText;
                    dic.Add(n.SelectSingleNode("wpns:identifier", mgr).InnerText, c);
                }
            }
            XmlNodeList routes = doc.DocumentElement.SelectNodes("//wpns:route-point", mgr);
            foreach (XmlNode n in routes)
            {
                string ident = n.SelectSingleNode("wpns:waypoint-identifier", mgr).InnerText;
                Coordinate c;
                if (dic.TryGetValue(ident, out c))
                {
                    _plan.AppendWaypoint(c);
                }
            }
            return _plan;
        }
    }
}

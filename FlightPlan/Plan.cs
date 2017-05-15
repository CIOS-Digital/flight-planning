using System;
using System.Collections;
using System.Collections.Generic;
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

        public static Plan XMLLoad(string filename)
        {
            Plan _plan = new FlightPlan.Plan();
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlNamespaceManager mgr = new XmlNamespaceManager(doc.NameTable);
            mgr.AddNamespace("df", doc.DocumentElement.NamespaceURI);
            XmlNodeList nodes = doc.DocumentElement.SelectNodes("//df:waypoint", mgr);
            Dictionary<string, Coordinate> dic = new Dictionary<string, Coordinate>();
            foreach (XmlNode n in nodes)
            {
                decimal latitude;
                decimal longitude;
                Coordinate c;
                if (Decimal.TryParse(n.SelectSingleNode("df:lat", mgr).InnerText, out latitude) && Decimal.TryParse(n.SelectSingleNode("df:lon", mgr).InnerText, out longitude))
                {
                    c = new Coordinate(latitude, longitude);
                    c.ident = n.SelectSingleNode("df:identifier", mgr).InnerText;
                    c.type = n.SelectSingleNode("df:type", mgr).InnerText;
                    c.country = n.SelectSingleNode("df:country-code", mgr).InnerText;
                    c.comment = n.SelectSingleNode("df:comment", mgr).InnerText;
                    dic.Add(c.ident, c);
                }
            }
            XmlNodeList routes = doc.DocumentElement.SelectNodes("//df:route-point", mgr);
            foreach (XmlNode n in routes)
            {
                string ident = n.SelectSingleNode("df:waypoint-identifier", mgr).InnerText;
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

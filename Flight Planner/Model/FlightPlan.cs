using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Xml;
using System.Linq;

namespace CIOSDigital.FlightPlanner.Model
{
    public class FlightPlan : IEnumerable<Waypoint>, INotifyCollectionChanged
    {
        private readonly List<Waypoint> Waypoints;
        protected List<Waypoint> originalWaypoints;
        public string filename;
        public UInt16 counter = 0;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public FlightPlan()
        {
            Waypoints = new List<Waypoint>();
            originalWaypoints = new List<Waypoint>();
        }

        bool WaypointIDExists(Waypoint w)
        {
            foreach (Waypoint pt in this.Waypoints)
            {
                if (pt.id.Equals(w.id))
                {
                    if (!pt.coordinate.Equals(w.coordinate))
                        return true;
                }
            }
            return false;
        }

        public void AppendWaypoint(Waypoint w)
        {
            if (!WaypointIDExists(w))
            {
                this.Waypoints.Add(w);
                CollectionChanged?.Invoke(Waypoints, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            else
            {
                AppendWaypoint(new Waypoint((w.id + this.counter++), w.coordinate));
            }
        }

        public void ModifyWaypoint(int windex, String id)
        {
            if (windex >= 0)
            {
                Waypoints[windex] = new Waypoint(id, Waypoints[windex].coordinate);
                CollectionChanged?.Invoke(Waypoints, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public void ModifyWaypoint(int windex, Coordinate coord)
        {
            if (windex >= 0)
            {
                Waypoints[windex] = new Waypoint(Waypoints[windex].id, coord);
                CollectionChanged?.Invoke(Waypoints, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public void ModifyWaypoint(int windex, String id, Coordinate coord)
        {
            if (windex >= 0)
            {
                Waypoints[windex] = new Waypoint(id, coord);
                CollectionChanged?.Invoke(Waypoints, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }


        public int GetWaypointIndex(Waypoint w)
        {
            return Waypoints.IndexOf(w);
        }

        public void DuplicateWaypoints()
        {
            originalWaypoints = new List<Waypoint>(Waypoints);
        }

        public void RemoveWaypoint(Waypoint w)
        {
            Waypoints.Remove(w);
            CollectionChanged?.Invoke(Waypoints, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool IsModified()
        {
            return !((this.originalWaypoints.Count == this.Waypoints.Count) && !this.originalWaypoints.Except(this.Waypoints).Any());
        }

        public IEnumerator<Waypoint> GetEnumerator() => Waypoints.GetEnumerator();
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

            { // Write table of waypoints
                writer.WriteStartElement("waypoint-table");
                for (int i = 0; i < this.Waypoints.Count; i += 1)
                {
                    Waypoint waypoint = this.Waypoints[i];
                    writer.WriteStartElement("waypoint");
                    writer.WriteElementString("identifier", waypoint.id);
                    writer.WriteElementString("type", "USER WAYPOINT");
                    writer.WriteElementString("country-code", "__");
                    writer.WriteElementString("lat", waypoint.coordinate.Latitude.ToString());
                    writer.WriteElementString("lon", waypoint.coordinate.Longitude.ToString());
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
                    Waypoint waypoint = this.Waypoints[i];
                    writer.WriteStartElement("route-point");
                    writer.WriteElementString("waypoint-identifier", waypoint.id);
                    writer.WriteElementString("waypoint-type", "USER WAYPOINT");
                    writer.WriteElementString("waypoint-country-code", "__");
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            writer.WriteEndDocument();
            writer.Close();
        }


        public static int FplRead(XmlDocument document, FlightPlan plan)        {

            plan.filename = new Uri(document.BaseURI).LocalPath;
            try
            {
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
                    if (!idToCoordinate.ContainsKey(n.SelectSingleNode("wpns:identifier", mgr).InnerText))
                        idToCoordinate.Add(n.SelectSingleNode("wpns:identifier", mgr).InnerText, c);
                }
            }

            XmlNodeList routes = document.DocumentElement.SelectNodes("//wpns:route-point", mgr);

                foreach (XmlNode n in routes)
                {
                    string ident = n.SelectSingleNode("wpns:waypoint-identifier", mgr).InnerText;
                    if (idToCoordinate.TryGetValue(ident, out Coordinate c))
                    {
                        plan.AppendWaypoint(new Waypoint(ident, c));
                    }
                }
            } catch (NullReferenceException) {
                return -1;
            }
            plan.DuplicateWaypoints();
            return 0;
        }
        public void Move(Waypoint w, Direction d)
        {
            var old = this.Waypoints.IndexOf(w);
            
            switch (d)
            {
                case (Direction.Up):
                    {
                        if (old != 0)
                        {
                            this.Waypoints.RemoveAt(old);
                            this.Waypoints.Insert(old - 1, w);
                        }
                            break;
                        
                    }
                case (Direction.Down):
                    {
                        if (old != this.Waypoints.Count-1)
                        {
                            this.Waypoints.RemoveAt(old);
                            this.Waypoints.Insert(old + 1, w);
                        }
                        break;
                    }
            }
            CollectionChanged?.Invoke(Waypoints, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}

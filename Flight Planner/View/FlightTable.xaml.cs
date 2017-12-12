using CIOSDigital.FlightPlanner.Model;
using System.Windows;
using System.Windows.Controls;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CIOSDigital.FlightPlanner.View
{
    public partial class FlightTable : UserControl
    {
        public static readonly DependencyProperty ActivePlanProperty =
            DependencyProperty.Register("ActivePlan", typeof(FlightPlan), typeof(FlightTable));

        public FlightPlan ActivePlan
        {
            get => this.GetValue(ActivePlanProperty) as FlightPlan;
            set => this.SetValue(ActivePlanProperty, value);
        }

        public FlightTable()
        {
            InitializeComponent();
        }

        private void DeleteSelectedClick(object sender, RoutedEventArgs e)
        {
            List<Waypoint> toDeleteList = new List<Waypoint>();
            foreach (Waypoint item in this.Table.SelectedItems)
            {
                toDeleteList.Add(item);
            }
            foreach (Waypoint item in toDeleteList)
            {
                ActivePlan.RemoveWaypoint(item);
            }
        }

        //This looks bad.... but it works?
        private void MoveSelectedClick(object sender, RoutedEventArgs e)
        {
            Direction dir;
            bool reversed;
            if (sender.Equals(DownButton))
            {
                reversed = true;
                dir = Direction.Down;
            }
            else
            {
                reversed = false;
                dir = Direction.Up;
            }

            int count = this.Table.SelectedItems.Count;

            if (count > 1)
            {
                if (ActivePlan.GetWaypointIndex((Waypoint)this.Table.SelectedItems[0]) > ActivePlan.GetWaypointIndex((Waypoint)this.Table.SelectedItems[1]))
                {
                    reversed = !reversed;
                }
            }
            if (reversed)
            {
                for (int i = count - 1; i >= 0; i--)
                {
                    this.ActivePlan.Move((Waypoint)this.Table.SelectedItems[i], dir);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    this.ActivePlan.Move((Waypoint)this.Table.SelectedItems[i], dir);
                }
            }
        }

        private void ModifySelectedClick(object sender, RoutedEventArgs e)
        {
            List<Waypoint> ModList = new List<Waypoint>();
            foreach (Waypoint item in this.Table.SelectedItems)
            {
                ModList.Add(item);
            }
            foreach (Waypoint item in ModList)
            {
                int windex = ActivePlan.GetWaypointIndex(item);
                var dialog = new PopupText();
                dialog.okButton.Content = "Modify";
                dialog.IDInput.Text = item.id;
                dialog.LatitudeInput.Text = item.coordinate.dmsLatitude;
                dialog.LongitudeInput.Text = item.coordinate.dmsLongitude;

                if (dialog.ShowDialog() == true)
                {
                    Coordinate c;
                    try
                    {
                        c = new Coordinate(dialog.LatitudeInput.Text, dialog.LongitudeInput.Text);
                    }
                    catch (System.ArgumentOutOfRangeException)
                    {
                        MessageBox.Show("Latitude/Longitude values are out of range");
                        return;
                    }
                    this.ActivePlan.ModifyWaypoint(windex, dialog.IDText, c);
                }
            }
        }
    }
}

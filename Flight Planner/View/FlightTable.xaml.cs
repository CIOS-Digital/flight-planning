using CIOSDigital.FlightPlanner.Model;
using System.Windows;
using System.Windows.Controls;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CIOSDigital.FlightPlanner.View
{
    public partial class FlightTable : UserControl
    {
        public static readonly DependencyProperty ActivePlanProperty =
            DependencyProperty.Register("ActivePlan", typeof(FlightPlan), typeof(FlightTable));

        public FlightPlan ActivePlan {
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
            foreach (Waypoint item in This.Table.SelectedItems)
            {
                toDeleteList.Add(item);
            }
            foreach (Waypoint item in toDeleteList)
            {
                ActivePlan.RemoveWaypoint(item);
            }
        }

        private void MoveSelectedUpClick(object sender, RoutedEventArgs e)
        {
            if (this.Table.SelectedItem != null)
            {
                this.ActivePlan.Move((Waypoint)this.Table.SelectedItem, Direction.Up);
            }
        }
        private void MoveSelectedDownClick(object sender, RoutedEventArgs e)
        {
            if (this.Table.SelectedItem != null)
            {
                this.ActivePlan.Move((Waypoint)this.Table.SelectedItem, Direction.Down);
            }
        }
    }
}

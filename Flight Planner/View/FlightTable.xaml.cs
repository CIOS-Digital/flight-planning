using CIOSDigital.FlightPlanner.Model;
using System.Windows;
using System.Windows.Controls;

namespace CIOSDigital.FlightPlanner.View
{
    public partial class FlightTable : UserControl
    {
        public static readonly DependencyProperty ActivePlanProperty =
            DependencyProperty.Register("ActivePlan", typeof(Plan), typeof(FlightTable));

        public Plan ActivePlan {
            get => this.GetValue(ActivePlanProperty) as Plan;
            set => this.SetValue(ActivePlanProperty, value);
        }

        public FlightTable()
        {
            InitializeComponent();
        }
    }
}

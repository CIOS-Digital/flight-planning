using CIOSDigital.FlightPlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CIOSDigital.FlightPlanner
{
    public partial class FlightTable : UserControl
    {
        public static readonly DependencyProperty ActivePlanProperty =
            DependencyProperty.Register("ActivePlan", typeof(Plan), typeof(FlightTable));
        public Plan ActivePlan {
            get {
                return this.GetValue(ActivePlanProperty) as Plan;
            }
            set {
                this.SetValue(ActivePlanProperty, value);
            }
        }

        public FlightTable()
        {
            InitializeComponent();
        }
    }
}

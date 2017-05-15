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
using System.Xml;

namespace CIOSDigital.FlightPlanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty ActivePlanProperty =
            DependencyProperty.Register("ActivePlan", typeof(Plan), typeof(MainWindow));
        public Plan ActivePlan {
            get {
                return this.GetValue(ActivePlanProperty) as Plan;
            }
            set {
                this.SetValue(ActivePlanProperty, value);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddWaypoint_Click(object sender, RoutedEventArgs e)
        {
            if (this.ActivePlan == null)
            {
                this.ActivePlan = Plan.Empty();
            }
            decimal latitude, longitude;
            if (Decimal.TryParse(LatitudeInput.Text, out latitude) && Decimal.TryParse(LongitudeInput.Text, out longitude))
            {
                this.ActivePlan.AppendWaypoint(new Coordinate(latitude, longitude));
            }
            this.FlightTable.Refresh();
        }

        private void OpenItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.ActivePlan == null)
            {
                this.ActivePlan = Plan.Empty();
            } else
            {
                Console.WriteLine("saving plan");
                //save plan
            }

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".fpl";
            dlg.Filter = "Flight Plan Files (*.fpl)|*.fpl";
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                Console.WriteLine(filename);
                this.FlightTable.ActivePlan = FlightPlan.Plan.XMLLoad(filename);
                this.FlightTable.Refresh();
            }
        }
    }
}

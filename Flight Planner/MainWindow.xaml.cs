using CIOSDigital.FlightPlan;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
            this.ActivePlan = Plan.Empty();
            // this.ActivePlan.CollectionChanged += (o, e) => this.FlightTable.Refresh();
            this.ActivePlan.CollectionChanged += (o, e) => this.Map.RefreshWaypoints();
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
            Map.RefreshWaypoints();
        }

        private void SaveItem_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Store the currently opened file path, if any
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = ".fpl";
            dlg.Filter = "Flight Plan Files (*.fpl)|*.fpl";
            bool fileSelected = dlg.ShowDialog(this).GetValueOrDefault(false);
            if (fileSelected)
            {
                string filename = dlg.FileName;
                using (StreamWriter writer = new StreamWriter(new FileStream(filename, FileMode.Create, FileAccess.Write), Encoding.UTF8))
                {
                    // HACK: The flight plan index needs to be calculated or stored somewhere, not a constant.
                    writer.Write(this.ActivePlan.ToXmlString(1));
                }
            }
        }

        private void OpenItem_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Proper metric to check for unsaved changes.

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".fpl";
            dlg.Filter = "Flight Plan Files (*.fpl)|*.fpl";
            bool fileSelected = dlg.ShowDialog(this).GetValueOrDefault(false);

            if (fileSelected)
            {
                string filename = dlg.FileName;
                this.ActivePlan = Plan.XmlLoad(filename);
                Map.RefreshWaypoints();
            }
        }
    }
}

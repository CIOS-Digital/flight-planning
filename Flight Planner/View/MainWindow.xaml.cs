using CIOSDigital.FlightPlanner.Model;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Xml;

namespace CIOSDigital.FlightPlanner.View
{
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty ActivePlanProperty =
            DependencyProperty.Register("ActivePlan", typeof(FlightPlan), typeof(MainWindow));

        public FlightPlan ActivePlan {
            get => this.GetValue(ActivePlanProperty) as FlightPlan;
            set => this.SetValue(ActivePlanProperty, value);
        }

        public MainWindow()
        {
            InitializeComponent();
            this.ActivePlan = new FlightPlan();
            // this.ActivePlan.CollectionChanged += (o, e) => this.FlightTable.Refresh();
            this.ActivePlan.CollectionChanged += (o, e) => this.Map.RefreshWaypoints();
        }

        private void AddWaypoint_Click(object sender, RoutedEventArgs e)
        {
            if (this.ActivePlan == null)
            {
                this.ActivePlan = new FlightPlan();
            }
            if (Decimal.TryParse(LatitudeInput.Text, out decimal latitude)
                && Decimal.TryParse(LongitudeInput.Text, out decimal longitude))
            {
                this.ActivePlan.AppendWaypoint(new Coordinate(latitude, longitude));
            }
            Map.RefreshWaypoints();
        }

        private void SaveItem_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Store the currently opened file path, if any
            SaveFileDialog dlg = new SaveFileDialog()
            {
                DefaultExt = ".fpl",
                Filter = "Flight Plan Files (*.fpl)|*.fpl",
            };
            bool fileSelected = dlg.ShowDialog(this).GetValueOrDefault(false);
            if (fileSelected)
            {
                string filename = dlg.FileName;
                using (StreamWriter writer = new StreamWriter(new FileStream(filename, FileMode.Create, FileAccess.Write), Encoding.UTF8))
                {
                    // HACK: The flight plan index needs to be calculated or stored somewhere, not a constant.
                    this.ActivePlan.FplWrite(writer, 1);
                }
            }
        }

        private void OpenItem_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Proper metric to check for unsaved changes.

            OpenFileDialog dlg = new OpenFileDialog()
            {
                DefaultExt = ".fpl",
                Filter = "Flight Plan Files (*.fpl)|*.fpl",
            };
            bool fileSelected = dlg.ShowDialog(this).GetValueOrDefault(false);

            if (fileSelected)
            {
                string filename = dlg.FileName;
                XmlDocument fplDocument = new XmlDocument();
                fplDocument.Load(filename);
                this.ActivePlan = FlightPlan.FplRead(fplDocument);
                Map.RefreshWaypoints();
            }
        }
    }
}

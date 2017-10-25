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
                Coordinate c = new Coordinate(latitude, longitude);
                this.ActivePlan.AppendWaypoint(new Waypoint(IDInput.Text, c));
            }
            Map.RefreshWaypoints();
        }

        private void SaveItem_Click(object sender, RoutedEventArgs e)
        {
            SavePlan(false);
        }


        private void SaveAsItem_Click(object sender, RoutedEventArgs e)
        {
            SavePlan(true);
        }

        private bool PromptSave()
        {
            if (this.ActivePlan.IsModified())
            {
                //This message is ugly
                MessageBoxResult messageBoxResult = MessageBox.Show("File has been modified since last save", "would you like to save first?", MessageBoxButton.YesNoCancel);
                if (messageBoxResult == MessageBoxResult.Cancel)
                    return false;
                if (messageBoxResult == MessageBoxResult.Yes)
                    SavePlan(false);
            }
            return true;
        }

        private void OpenItem_Click(object sender, RoutedEventArgs e)
        {
            if (!PromptSave())
                return;
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

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void This_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!PromptSave())
            {
                e.Cancel = true;
            }
        }

        void SavePlan(bool saveNew)
        {
            string filename = this.ActivePlan.filename;
            bool fileSelected = true;
            if (saveNew || String.IsNullOrEmpty(filename))
            {
                SaveFileDialog dlg = new SaveFileDialog()
                {
                    DefaultExt = ".fpl",
                    Filter = "Flight Plan Files (*.fpl)|*.fpl",
                };
                fileSelected = dlg.ShowDialog(this).GetValueOrDefault(false);
                this.ActivePlan.filename = dlg.FileName;
                filename = dlg.FileName;
            }

            if (fileSelected)
            {
                this.ActivePlan.DuplicateWaypoints();
                using (StreamWriter writer = new StreamWriter(new FileStream(filename, FileMode.Create, FileAccess.Write), Encoding.UTF8))
                {
                    // HACK: The flight plan index needs to be calculated or stored somewhere, not a constant.
                    this.ActivePlan.FplWrite(writer, 1);
                }
            }
        }
    }
}

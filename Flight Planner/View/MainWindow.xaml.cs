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
            set {
                this.SetValue(ActivePlanProperty, value);
                value.CollectionChanged += (o, e) =>
                    this.Map.RefreshWaypoints();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.ActivePlan = new FlightPlan();
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
                string id = IDInput.Text;
                if (String.IsNullOrEmpty(id))
                {
                    id = "W_" + this.ActivePlan.counter.ToString();
                    this.ActivePlan.counter++;
                }
                this.ActivePlan.AppendWaypoint(new Waypoint(id, c));
            }
            LatitudeInput.Clear();
            LongitudeInput.Clear();
            IDInput.Clear();
            IDInput.Focus();
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
                this.ActivePlan = new FlightPlan();
                int result = FlightPlan.FplRead(fplDocument, this.ActivePlan);
                if (result < 0)
                    MessageBox.Show("Failed to parse flight plan");
                else
                    Map.RefreshWaypoints();
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var about = AboutWindow.Instance;
            about.Show();
            about.Activate();
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

        private void NewItem_Click(object sender, RoutedEventArgs e)
        {
            if (!PromptSave())
            {
                return;
            }
            this.ActivePlan = new FlightPlan();
            Map.RefreshWaypoints();
        }

        private void Input_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Return)
            {
                AddWaypoint_Click(sender, e);
            }
        }

    }
}

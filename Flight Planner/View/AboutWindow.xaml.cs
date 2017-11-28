using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace CIOSDigital.FlightPlanner.View
{
    public partial class AboutWindow : Window
    {
        private static AboutWindow instance = null;

        public static AboutWindow Instance {
            get {
                instance = instance ?? new AboutWindow();
                return instance;
            }
        }

        private AboutWindow()
        {
            InitializeComponent();
            this.Closed += (s, e) => instance = null;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var link = (Hyperlink)sender;
            Process.Start(link.NavigateUri.ToString());
        }
    }
}

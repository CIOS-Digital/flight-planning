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
using System.Windows.Shapes;

namespace CIOSDigital.FlightPlanner.View
{
    /// <summary>
    /// Interaction logic for PopupText.xaml
    /// </summary>
    public partial class PopupText : Window
    {
        public PopupText()
        {
            App.Current.MainWindow.Opacity = 0.7;
            InitializeComponent();
            IDInput.Focus();
            IDInput.SelectAll();
        }

        public String IDText
        {
            get { return IDInput.Text; }
            set { IDInput.Text = value; }
        }

        public String LatitudeText
        {
            get { return LatitudeInput.Text; }
            set { LatitudeInput.Text = value; }
        }

        public String LongitudeText
        {
            get { return LongitudeInput.Text; }
            set { LongitudeInput.Text = value; }
        }

        private void OkButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                e.Handled = true;
                DialogResult = true;

            } else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                DialogResult = false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.Current.MainWindow.Opacity = 1;
        }
    }
}

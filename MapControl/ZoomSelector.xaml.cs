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

namespace CIOSDigital.MapControl
{
    /// <summary>
    /// Interaction logic for ZoomSelector.xaml
    /// </summary>
    public partial class ZoomSelector : UserControl
    {
        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyProperty.Register("ZoomLevel", typeof(int), typeof(ZoomSelector));
        public int ZoomLevel {
            get {
                return (int)this.GetValue(ZoomLevelProperty);
            }
            set {
                double val = (double)value;
                double bounded = Math.Min(ZoomSlider.Maximum, Math.Max(ZoomSlider.Minimum, val));
                this.SetValue(ZoomLevelProperty, (int)bounded);
            }
        }

        public ZoomSelector()
        {
            InitializeComponent();
        }

        private void ZoomIn(object sender, RoutedEventArgs args)
        {
            this.ZoomLevel += (int)this.ZoomSlider.TickFrequency;
        }

        private void ZoomOut(object sender, RoutedEventArgs args)
        {
            this.ZoomLevel -= (int)this.ZoomSlider.TickFrequency;
        }
    }
}

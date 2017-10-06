using System;
using System.Windows;
using System.Windows.Controls;

namespace CIOSDigital.FlightPlanner.View
{
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
                if (this != null && ZoomLevelChanged != null)
                {
                    this.ZoomLevelChanged.Invoke(this, new RoutedEventArgs());
                }
            }
        }

        public event RoutedEventHandler ZoomLevelChanged;

        public ZoomSelector()
        {
            InitializeComponent();
            this.ZoomLevel = 9;
        }

        private void SliderChanged(object sender, RoutedEventArgs args)
        {
            ZoomLevelChanged?.Invoke(this, args);
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

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
using CIOSDigital.MapDB;

namespace CIOSDigital.MapControl
{
    /// <summary>
    /// Interaction logic for MapTypeSelector.xaml
    /// </summary>
    public partial class MapTypeSelector : UserControl
    {

        public static readonly DependencyProperty MapTypeProperty =
            DependencyProperty.Register("MapType", typeof(MapType), typeof(ZoomSelector));
        public MapType MapType {
            get {
                return (MapType)this.GetValue(MapTypeProperty);
            }
            set {
                MapType current = (MapType)value;
                MapType previous = this.MapType;
                if (previous != current)
                {
                    this.SetValue(MapTypeProperty, (MapType)value);
                    MapTypeChanged?.Invoke(this, new RoutedEventArgs());
                    foreach (Button b in this.Container.Children.OfType<Button>())
                    {
                        b.IsEnabled = current != (MapType)b.Tag;
                    }
                }
            }
        }

        public event RoutedEventHandler MapTypeChanged;

        public MapTypeSelector()
        {
            InitializeComponent();
            this.MapType = (MapType)this.DefaultButton.Tag;
        }

        private void ButtonClicked(object sender, RoutedEventArgs e)
        {
            if (e.Source is Button)
            {
                this.MapType = (MapType)(e.Source as Button).Tag;
            }
        }
    }
}

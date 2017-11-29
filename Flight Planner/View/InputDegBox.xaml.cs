using System.Windows.Controls;
using System.Windows.Input;

namespace CIOSDigital.FlightPlanner.View
{
    /// <summary>
    /// Interaction logic for InputDegBox.xaml
    /// </summary>
    public partial class InputDegBox : TextBox
    {
        public InputDegBox()
        {
            InitializeComponent();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var txtBox = (System.Windows.Controls.TextBox)sender;
            string deg = "°";
            //space, period
            if (e.Key == System.Windows.Input.Key.Space)
            {
                if (!txtBox.Text.Contains(deg))
                {
                    if (txtBox.CaretIndex > 0 && char.IsDigit(txtBox.Text[txtBox.CaretIndex - 1]))
                    {
                        int pos = txtBox.CaretIndex;
                        txtBox.Text = txtBox.Text.Insert(pos, deg);
                        txtBox.CaretIndex = pos+1;
                    }
                }
                e.Handled = true;
            } else if (e.Key == Key.OemPeriod)
            {
                if (txtBox.Text.Contains(deg) && !txtBox.Text.Contains(".")) { 
                    if (txtBox.CaretIndex > 0 && char.IsDigit(txtBox.Text[txtBox.CaretIndex - 1]))
                    {
                        if (!txtBox.Text.Contains("'") || txtBox.CaretIndex < txtBox.Text.IndexOf("'"))
                        {
                            int pos = txtBox.CaretIndex;
                            txtBox.Text = txtBox.Text.Insert(pos, ".");
                            txtBox.CaretIndex = pos + 1;
                        }
                    }
                }
                e.Handled = true;
            } else if (e.Key == System.Windows.Input.Key.OemMinus)
            {
                if (!txtBox.Text.Contains("-"))
                {
                    if (txtBox.CaretIndex != 0)
                    {
                        e.Handled = true;
                    }
                    else
                    {
                        int pos = txtBox.CaretIndex;
                        txtBox.Text = txtBox.Text.Insert(pos, "-");
                        txtBox.CaretIndex = pos + 1;
                    }
                }
                e.Handled = true;
            } else if (e.Key == Key.OemQuotes)
            {
                if (txtBox.Text.Contains(deg) && !txtBox.Text.Contains("'"))
                {
                    if (txtBox.CaretIndex > 0 && char.IsDigit(txtBox.Text[txtBox.CaretIndex - 1]))
                    {
                        int pos = txtBox.CaretIndex;
                        txtBox.Text = txtBox.Text.Insert(pos, "'");
                        txtBox.CaretIndex = pos + 1;
                    }
                }
                e.Handled = true;
            } else if ((e.Key >= System.Windows.Input.Key.D0 && e.Key <= System.Windows.Input.Key.D9) || (e.Key >= System.Windows.Input.Key.NumPad0 && e.Key <= System.Windows.Input.Key.NumPad9))
            {
                if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift) || System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift))
                {
                    e.Handled = true;
                }

            }
            else if (e.Key == System.Windows.Input.Key.Delete || e.Key == System.Windows.Input.Key.Back || e.Key == System.Windows.Input.Key.Right || e.Key == System.Windows.Input.Key.Left || e.Key == System.Windows.Input.Key.Home || e.Key == System.Windows.Input.Key.End || e.Key == System.Windows.Input.Key.Tab)
            {

            }
            else
            {
                e.Handled = true;
            }
        }
    }
}

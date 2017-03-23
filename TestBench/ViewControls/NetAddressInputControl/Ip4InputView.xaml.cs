using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace Iit.Fibertest.TestBench
{
    /// <summary>
    /// Interaction logic for Ip4InputView.xaml
    /// </summary>
    public partial class Ip4InputView
    {
        public Ip4InputView()
        {
            InitializeComponent();
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ((TextBox) sender).SelectAll();
        }

        private void TextBox_GotMouseCapture(object sender, MouseEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsInputedTextAllowed(e.Text);
        }

        private bool IsInputedTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }
    }
}

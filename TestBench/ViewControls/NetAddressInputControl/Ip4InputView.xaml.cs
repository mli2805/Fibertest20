using System.Text.RegularExpressions;
using System.Windows;
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
            if (e.Text == @".")
            {
                MoveFocus();
                e.Handled = true;
            }
            else
                e.Handled = !IsInputedTextAllowed(e.Text);
        }

        private bool IsInputedTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        private void StackPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MoveFocus();

                e.Handled = true;
            }
        }

        private static void MoveFocus()
        {
            TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
            UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;
            keyboardFocus?.MoveFocus(tRequest);
        }
    }
}

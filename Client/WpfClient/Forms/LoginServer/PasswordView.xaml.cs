using System.Windows;

namespace Iit.Fibertest.Client
{
    /// <summary>
    /// Interaction logic for PasswordView.xaml
    /// </summary>
    public partial class PasswordView
    {
        public PasswordView()
        {
            InitializeComponent();
        }

        void ShowPassword(object sender, RoutedEventArgs e)
        {
            PwdTextBox.Text = PwdBox.Password;
            PwdBox.Visibility = Visibility.Hidden;
        }

        void HidePassword(object sender, RoutedEventArgs e)
        {
            PwdTextBox.Text = "";
            PwdBox.Visibility = Visibility.Visible;
        }
    }
}

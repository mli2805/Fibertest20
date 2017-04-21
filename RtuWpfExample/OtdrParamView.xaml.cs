using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RtuWpfExample
{
    /// <summary>
    /// Interaction logic for OtdrParamView.xaml
    /// </summary>
    public partial class OtdrParamView
    {
        public OtdrParamView()
        {
            InitializeComponent();
        }

        private void RadioButtonOnChecked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            RbCount.Foreground  = rb.Name == "RbCount" ? Brushes.Black : Brushes.DarkGray;
            CbCounts.Foreground = rb.Name == "RbCount" ? Brushes.Black : Brushes.DarkGray;
            RbTime.Foreground   = rb.Name == "RbCount" ? Brushes.DarkGray : Brushes.Black;
            CbTimes.Foreground  = rb.Name == "RbCount" ? Brushes.DarkGray : Brushes.Black;
        }
    }
}

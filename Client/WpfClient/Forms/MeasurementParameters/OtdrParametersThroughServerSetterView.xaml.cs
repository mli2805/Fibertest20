using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Iit.Fibertest.Client
{
    /// <summary>
    /// Interaction logic for OtdrParametersThroughServerSetterView.xaml
    /// </summary>
    public partial class OtdrParametersThroughServerSetterView
    {
        public OtdrParametersThroughServerSetterView()
        {
            InitializeComponent();
        }

        private void RadioButtonOnChecked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            RbCount.Foreground  = rb.Name == @"RbCount" ? Brushes.Black : Brushes.DarkGray;
            CbCounts.Foreground = rb.Name == @"RbCount" ? Brushes.Black : Brushes.DarkGray;
            RbTime.Foreground   = rb.Name == @"RbCount" ? Brushes.DarkGray : Brushes.Black;
            CbTimes.Foreground  = rb.Name == @"RbCount" ? Brushes.DarkGray : Brushes.Black;
        }
    }
}

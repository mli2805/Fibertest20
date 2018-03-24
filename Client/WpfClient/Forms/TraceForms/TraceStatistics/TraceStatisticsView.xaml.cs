using System.Windows.Input;

namespace Iit.Fibertest.Client
{
    /// <summary>
    /// Interaction logic for TraceStatisticsView.xaml
    /// </summary>
    public partial class TraceStatisticsView
    {
        public TraceStatisticsView()
        {
            InitializeComponent();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}

using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RtuWpfExample
{
    /// <summary>
    /// Interaction logic for RftsEventsOneLevelView.xaml
    /// </summary>
    public partial class RftsEventsOneLevelView
    {
        public RftsEventsOneLevelView()
        {
            InitializeComponent();
        }

        private void EventsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            DataGridRow row = e.Row;
            var rowView = row.Item as DataRowView;
            if (rowView == null)
                return;
            var name = (string) rowView.Row.ItemArray[0];
            if (name.StartsWith(" "))
            {
                row.FontWeight = FontWeights.Bold;
                row.Background = Brushes.LightCyan;
            }
        }
    }
}

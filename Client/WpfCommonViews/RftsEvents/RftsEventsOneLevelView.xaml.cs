using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Iit.Fibertest.WpfCommonViews
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
            if (!(row.Item is DataRowView rowView))
                return;
            var name = (string) rowView.Row.ItemArray[0];
            if (name.StartsWith(@" "))
            {
                row.FontWeight = FontWeights.Bold;
                row.Background = Brushes.LightCyan;
            }
        }
    }
}

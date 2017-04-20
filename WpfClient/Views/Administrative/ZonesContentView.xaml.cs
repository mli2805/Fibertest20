using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Iit.Fibertest.Client
{
    /// <summary>
    /// Interaction logic for ZonesContentView.xaml
    /// </summary>
    public partial class ZonesContentView
    {
        public ZonesContentView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataGrid.Columns[2].IsReadOnly = true;
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.CellStyle = new Style(typeof(DataGridCell));
            if ((string) e.Column.Header == StringResources.Resources.SID_Default_Zone)
            {
                e.Column.CellStyle.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Colors.LightGray)));
                e.Column.CellStyle.Setters.Add(new Setter(ForegroundProperty, new SolidColorBrush(Colors.DarkGray)));
            }
        }
    }
}

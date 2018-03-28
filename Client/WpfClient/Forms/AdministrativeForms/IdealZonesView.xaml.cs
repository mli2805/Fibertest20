using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Iit.Fibertest.Client
{
    /// <summary>
    /// Interaction logic for IdealZonesView.xaml
    /// </summary>
    public partial class IdealZonesView
    {
        public IdealZonesView()
        {
            InitializeComponent();
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            DataGridRow row = e.Row;
            if (!(row.Item is ObjectToZonesModel lineModel))
                return;

            var name = lineModel.ObjectTitle;
            if (!name.StartsWith(@"  "))
            {
                row.FontWeight = FontWeights.Bold;
                row.Background = Brushes.LightCyan;
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox) sender;
            var str = (string) checkBox.Tag;
            var tag = int.Parse(str);

            var vm = (IdealZonesViewModel)DataContext;
            vm.OnClick(tag);
        }

        private void F()
        {
            var vm = (IdealZonesViewModel)DataContext;

            var datagrid = new DataGrid();
            var columntTitle = new DataGridTextColumn()
            {
                Header = "Title",
                Width = 200,
                Binding = new Binding("ObjectTitle"),
            };
            datagrid.Columns.Add(columntTitle);

            foreach (var zone in vm.ReadModel.Zones)
            {
                var columnZone = new DataGridTemplateColumn()
                {
                    Header = zone.Title,
                };
                datagrid.Columns.Add(columnZone);
            }

            datagrid.ItemsSource = vm.Rows;
            datagrid.SelectedItem = vm.SelectedRow;
        }
    }
}

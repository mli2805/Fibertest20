
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Iit.Fibertest.Client
{
    /// <summary>
    /// Interaction logic for ObjectsToZonesView.xaml
    /// </summary>
    public partial class ObjectsToZonesView
    {
        public ObjectsToZonesView()
        {
            InitializeComponent();
        }

      
        // реагирует только на начало изменения ячейки, далее - нет
        private void Datagrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            //            var vm = (ObjectsToZonesViewModel)DataContext;

            //            vm.OnCellValueChanged(e.Column.DisplayIndex, e.Row.GetIndex());
        }

        private void Datagrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            DataGridRow row = e.Row;
            if (!(row.Item is DataRowView rowView))
                return;

            var name = (string)rowView.Row.ItemArray[0];
            if (!name.StartsWith(@" "))
            {
                row.FontWeight = FontWeights.Bold;
                row.Background = Brushes.LightCyan;
            }
        }

        // возникает когда изменяется, то какая ячейка считается текущей, а не когда значение текущей меняется
        private void Datagrid_CurrentCellChanged(object sender, System.EventArgs e)
        {
            //            var vm = (ObjectsToZonesViewModel)DataContext;

            //            if (MyDatagrid.SelectedCells.Count == 0) return;

            //            vm.OnCellValueChanged(MyDatagrid.SelectedCells[0].Column.DisplayIndex, MyDatagrid.Items.IndexOf(MyDatagrid.SelectedCells[0].Item));

        }
    }
}

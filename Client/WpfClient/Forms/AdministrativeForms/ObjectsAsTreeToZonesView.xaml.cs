using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Iit.Fibertest.Client
{
    /// <summary>
    /// Interaction logic for ObjectsAsTreeToZonesView.xaml
    /// </summary>
    public partial class ObjectsAsTreeToZonesView
    {
        public ObjectsAsTreeToZonesView()
        {
            InitializeComponent();
            // DataContext is not set yet
        }

        private void Window_Activated(object sender, System.EventArgs e)
        {
            // Yes, DataContext is set now!
            if (DataContext == null) return;
            var vm = (ObjectsAsTreeToZonesViewModel)DataContext;
            vm.ConstructDataGrid(MainDataGrid);
        }

        private void MainDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            DataGridRow row = e.Row;
            if (!(row.Item is ObjectToZonesModel lineModel))
                return;

            if (!lineModel.ObjectTitle.StartsWith(@"  "))
            {
                row.FontWeight = FontWeights.Bold;
                row.Background = Brushes.LightCyan;
            }
        }

       
    }
}

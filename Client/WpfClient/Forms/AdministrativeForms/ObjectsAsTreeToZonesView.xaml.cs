using System.Windows.Controls;
using System.Windows.Data;

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
            var vm = (ObjectsAsTreeToZonesViewModel) DataContext;
            vm.ConstructDataGrid(MainDataGrid);
        }
    }
}

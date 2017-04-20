namespace Iit.Fibertest.Client
{
    /// <summary>
    /// Interaction logic for ExampleView.xaml
    /// </summary>
    public partial class ObjectsToZonesView
    {
        public ObjectsToZonesView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            DataGrid.Columns[0].IsReadOnly = true;
            DataGrid.Columns[1].IsReadOnly = true;
        }
    }
}

using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System;

namespace Iit.Fibertest.Client
{
    /// <summary>
    /// Interaction logic for RtuAccidentsView.xaml
    /// </summary>
    public partial class RtuAccidentsView
    {
        public RtuAccidentsView()
        {
            InitializeComponent();
        }

        private void OnSelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            Selector selector = sender as Selector;
            if ( selector is DataGrid dataGrid && selector.SelectedItem != null && dataGrid.SelectedIndex >= 0 )
            {
                try
                {
                    dataGrid.ScrollIntoView( selector.SelectedItem );
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }
    }
}

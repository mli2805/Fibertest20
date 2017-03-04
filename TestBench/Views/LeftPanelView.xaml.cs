namespace Iit.Fibertest.TestBench
{
    /// <summary>
    /// Interaction logic for LeftPanelView.xaml
    /// </summary>
    public partial class LeftPanelView
    {
        public LeftPanelView()
        {
            InitializeComponent();
        }

        private void Expand_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (var item in MainTreeView.Items)
                SetExpandTo(true, (Leaf)item);
        }

        private void SetExpandTo(bool expand, Leaf root)
        {
            foreach (var child in root.Children)
            {
                SetExpandTo(expand, child);
            }
            root.IsExpanded = expand;
        }

        private void Collapse_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (var item in MainTreeView.Items)
                SetExpandTo(false, (Leaf)item);
        }
        private void TogglePorts_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (TogglePorts.IsChecked == true)
                HidePorts();
            if (TogglePorts.IsChecked == false)
               ShowPorts();
        }

        private void HidePorts()
        {
            foreach (var item in MainTreeView.Items)
            {
                if (item is RtuLeaf)
                    ((RtuLeaf)item).RemoveFreePorts();
            }
        }
        private void ShowPorts()
        {
            foreach (var item in MainTreeView.Items)
            {
                if (item is RtuLeaf)
                    ((RtuLeaf)item).RestoreFreePorts();
            }
        }

    }
}

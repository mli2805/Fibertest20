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
                SetExpandTo(true, (RtuLeaf)item);
        }

        private void SetExpandTo(bool expand, Leaf root)
        {
            IPortOwner portOwner = root as IPortOwner;
            if (portOwner == null)
                return;

            foreach (var child in portOwner.ChildrenPorts.Children)
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

    }
}

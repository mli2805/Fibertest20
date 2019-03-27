using System.Windows;

namespace Iit.Fibertest.Client
{
    /// <summary>
    /// Interaction logic for TreeOfRtuView.xaml
    /// </summary>
    public partial class TreeOfRtuView
    {
        public TreeOfRtuView()
        {
            InitializeComponent();
        }
        private void Expand_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in MainTreeView.Items)
                SetExpandTo(true, (RtuLeaf)item);
        }

        private void SetExpandTo(bool expand, Leaf root)
        {
            IPortOwner portOwner = root as IPortOwner;
            if (portOwner == null)
                return;

            foreach (var child in portOwner.ChildrenImpresario.Children)
            {
                SetExpandTo(expand, child);
            }
            root.IsExpanded = expand;
        }

        private void Collapse_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in MainTreeView.Items)
                SetExpandTo(false, (Leaf)item);
        }

        private void BringIntoView_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class TreeOfRtuViewModel : PropertyChangedBase
    {
        private readonly ChildrenViews _childrenViews;
        public TreeOfRtuModel TreeOfRtuModel { get; set; }
        public FreePorts FreePorts { get; }

        public TreeOfRtuViewModel( TreeOfRtuModel treeOfRtuModel, FreePorts freePorts, ChildrenViews childrenViews,
             EventArrivalNotifier eventArrivalNotifier)
        {
            _childrenViews = childrenViews;
            TreeOfRtuModel = treeOfRtuModel;
            TreeOfRtuModel.RefreshStatistics();

            FreePorts = freePorts;
            FreePorts.AreVisible = true;

            eventArrivalNotifier.PropertyChanged += _eventArrivalNotifier_PropertyChanged;
        }

        public void ChangeFreePortsVisibility()
        {
            FreePorts.AreVisible = !FreePorts.AreVisible;
        }

        private void _eventArrivalNotifier_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            TreeOfRtuModel.RefreshStatistics();
        }

        public void CloseChildren()
        {
            _childrenViews.ShouldBeClosed = true;
        }

        public void CollapseAll()
        {
            foreach (var leaf in TreeOfRtuModel.Tree)
            {
                leaf.IsExpanded = false;
            }
        }
      
    }
}

using System.Windows.Media;
using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class TreeOfRtuViewModel : PropertyChangedBase
    {
        private readonly ChildrenViews _childrenViews;
        public TreeOfRtuModel TreeOfRtuModel { get; set; }
        public FreePorts FreePorts { get; }

        private string _textToFind;

        public string TextToFind
        {
            get { return _textToFind; }
            set
            {
                if (value == _textToFind) return;
                _textToFind = value;
                NotifyOfPropertyChange();
                Find();
                NotifyOfPropertyChange(nameof(Found));
            }
        }

        private int _foundCounter;
        public string Found => _foundCounter == 0 ? "" : _foundCounter.ToString();

        public TreeOfRtuViewModel(TreeOfRtuModel treeOfRtuModel, FreePorts freePorts, ChildrenViews childrenViews,
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

        private void _eventArrivalNotifier_PropertyChanged(object sender,
            System.ComponentModel.PropertyChangedEventArgs e)
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

        private void Find()
        {
            ExtinguishRtus();
            if (string.IsNullOrEmpty(TextToFind)) return;

            foreach (var leaf in TreeOfRtuModel.Tree)
            {
                if (!(leaf is RtuLeaf rtuLeaf)) continue;
                FindLeaves(rtuLeaf);
            }
        }

        private void FindLeaves(IPortOwner portOwner)
        {
            var leaf = (Leaf)portOwner;
            if (leaf.Name.Contains(TextToFind))
            {
                leaf.BackgroundBrush = Brushes.LightGoldenrodYellow;
                _foundCounter++;
            }
            foreach (var child in portOwner.ChildrenImpresario.Children)
            {
                if (child is IPortOwner subPortOwner)
                    FindLeaves(subPortOwner);
                if (child.Name.Contains(TextToFind))
                {
                    child.BackgroundBrush = Brushes.LightGoldenrodYellow;
                    leaf.BackgroundBrush = Brushes.LightGoldenrodYellow;
                    _foundCounter++;
                }
            }
        }

        public void ClearButton()
        {
            TextToFind = "";
            ExtinguishRtus();
        }

        private void ExtinguishRtus()
        {
            _foundCounter = 0;
            foreach (var leaf in TreeOfRtuModel.Tree)
            {
                leaf.BackgroundBrush = Brushes.White;
                ExtinguishLeaves((IPortOwner)leaf);
            }
        }

        private void ExtinguishLeaves(IPortOwner portOwner)
        {
            foreach (var child in portOwner.ChildrenImpresario.EffectiveChildren)
            {
                if (child is IPortOwner subPortOwner)
                    ExtinguishLeaves(subPortOwner);
                child.BackgroundBrush = Brushes.White;
            }
        }
    }
}
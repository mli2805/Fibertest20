using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TreeOfRtuViewModel : PropertyChangedBase
    {
        private readonly Model _reaModel;
        private readonly CurrentlyHiddenRtu _currentlyHiddenRtu;
        public TreeOfRtuModel TreeOfRtuModel { get; set; }
        public FreePorts FreePorts { get; }

        public TreeOfRtuViewModel(Model reaModel, TreeOfRtuModel treeOfRtuModel, FreePorts freePorts, 
            CurrentlyHiddenRtu currentlyHiddenRtu, EventArrivalNotifier eventArrivalNotifier)
        {
            _reaModel = reaModel;
            _currentlyHiddenRtu = currentlyHiddenRtu;
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

        public void ShowAllGraph()
        {
            _currentlyHiddenRtu.Collection.Clear();
        }

        public void HideAllGraph()
        {
            _currentlyHiddenRtu.Collection.Clear();
            foreach (var rtu in _reaModel.Rtus)
                _currentlyHiddenRtu.Collection.Add(rtu.Id);
        }
    }
}

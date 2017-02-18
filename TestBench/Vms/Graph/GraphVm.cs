using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public partial class GraphVm : PropertyChangedBase
    {
        public ObservableCollection<NodeVm> Nodes { get; }
        public ObservableCollection<FiberVm> Fibers { get; }
        public ObservableCollection<RtuVm> Rtus { get; }
        public ObservableCollection<EquipmentVm> Equipments { get; }
        public ObservableCollection<TraceVm> Traces { get; }
        public ObservableCollection<MarkerVm> MarkerVms { get; }

        private string _currentMousePosition;
        public string CurrentMousePosition
        {
            get { return _currentMousePosition; }
            set
            {
                if (value.Equals(_currentMousePosition)) return;
                _currentMousePosition = value;
                NotifyOfPropertyChange();
            }
        }

        private object _request;
        public object Request
        {
            get { return _request; }
            set
            {
                if (Equals(value, _request)) return;
                _request = value;
                NotifyOfPropertyChange();
            }
        }

        public GraphVm()
        {
            Nodes = new ObservableCollection<NodeVm>();
            Fibers = new ObservableCollection<FiberVm>();
            Rtus = new ObservableCollection<RtuVm>();
            Equipments = new ObservableCollection<EquipmentVm>();
            Traces = new ObservableCollection<TraceVm>();
            MarkerVms = new ObservableCollection<MarkerVm>();

            IsEquipmentVisible = true;
        }

    }
}
using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public class GraphVm : PropertyChangedBase
    {
        public ObservableCollection<NodeVm> Nodes { get; }
        public ObservableCollection<FiberVm> Edges { get; }

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

        private object _command;
        public object Command
        {
            get { return _command; }
            set
            {
                if (Equals(value, _command)) return;
                _command = value;
                NotifyOfPropertyChange();
            }
        }

        public GraphVm()
        {
            Nodes = new ObservableCollection<NodeVm>();
            Edges = new ObservableCollection<FiberVm>();
        }

    }
}
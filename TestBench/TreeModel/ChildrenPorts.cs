using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public sealed class ChildrenPorts : PropertyChangedBase
    {
        private readonly FreePortsToggleButton _freePortsToggleButton;

        public ChildrenPorts(ObservableCollection<Leaf> children, FreePortsToggleButton freePortsToggleButton)
        {
            Children = children;

            _freePortsToggleButton = freePortsToggleButton;
            freePortsToggleButton.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(FreePortsToggleButton.State))
                    NotifyOfPropertyChange(nameof(EffectiveChildren));
            };
        }

        public ObservableCollection<Leaf> EffectiveChildren
            => _freePortsToggleButton.State == FreePortsDisplayMode.Show ? Children :  new ObservableCollection<Leaf>(Children.Where(c=>!(c is PortLeaf)));

        public ObservableCollection<Leaf> Children { get; } 
    }
}
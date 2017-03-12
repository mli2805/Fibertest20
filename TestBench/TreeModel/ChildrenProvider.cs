using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public sealed class ChildrenProvider : PropertyChangedBase
    {
        private readonly FreePortsToggleButton _freePortsToggleButton;

        public ObservableCollection<Leaf> Children { get; }

        public ObservableCollection<Leaf> EffectiveChildren
            => _freePortsToggleButton.State == FreePortsDisplayMode.Show ? Children :  new ObservableCollection<Leaf>(Children.Where(c=>!(c is PortLeaf)));

        public ChildrenProvider(FreePortsToggleButton freePortsToggleButton)
        {
            Children = new ObservableCollection<Leaf>();

            _freePortsToggleButton = freePortsToggleButton;
            freePortsToggleButton.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(FreePortsToggleButton.State))
                    NotifyOfPropertyChange(nameof(EffectiveChildren));
            };
        }
    }
}
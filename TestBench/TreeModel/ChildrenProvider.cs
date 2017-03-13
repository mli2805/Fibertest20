using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public sealed class ChildrenProvider : PropertyChangedBase
    {
        private readonly FreePortsVisibility _freePortsVisibility;

        public ObservableCollection<Leaf> Children { get; }

        public ObservableCollection<Leaf> EffectiveChildren
            => _freePortsVisibility.State == FreePortsVisibilityState.Show ? Children :  new ObservableCollection<Leaf>(Children.Where(c=>!(c is PortLeaf)));

        public ChildrenProvider(FreePortsVisibility freePortsVisibility)
        {
            Children = new ObservableCollection<Leaf>();

            _freePortsVisibility = freePortsVisibility;
            freePortsVisibility.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(FreePortsVisibility.State))
                    NotifyOfPropertyChange(nameof(EffectiveChildren));
            };
        }
    }
}
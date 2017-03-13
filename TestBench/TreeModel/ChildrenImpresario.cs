using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public sealed class ChildrenImpresario : PropertyChangedBase
    {
        private readonly FreePorts _freePorts;

        public ObservableCollection<Leaf> Children { get; }

        public ObservableCollection<Leaf> EffectiveChildren
            => _freePorts.AreVisible ? Children :  new ObservableCollection<Leaf>(Children.Where(c=>!(c is PortLeaf)));

        public ChildrenImpresario(FreePorts freePorts)
        {
            Children = new ObservableCollection<Leaf>();

            _freePorts = freePorts;
            freePorts.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(FreePorts.AreVisible))
                    NotifyOfPropertyChange(nameof(EffectiveChildren));
            };
        }
    }
}
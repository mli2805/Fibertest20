using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class TraceStepByStepViewModel : Screen
    {
        private readonly GraphReadModel _graphReadModel;
        public ObservableCollection<string> Nodes { get; set;   }

        public TraceStepByStepViewModel(GraphReadModel graphReadModel)
        {
            _graphReadModel = graphReadModel;
        }

        public void Initialize(Guid rtuId)
        {
            Nodes = new ObservableCollection<string>();
            var rtu = _graphReadModel.Rtus.First(r => r.Node.Id == rtuId);
            Nodes.Add(rtu.Title);
        }

        public void Accept()
        {
            TryClose();
        }
    }
}

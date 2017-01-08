using System;
using System.ComponentModel;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public sealed class MapViewModel : IDataErrorInfo
    {
        private readonly Aggregate _aggregate;

        public MapViewModel(Aggregate aggregate)
        {
            _aggregate = aggregate;
        }

        #region Node
        public void AddNode()
        {
            _aggregate.When(new AddNode { Id = Guid.NewGuid() });
        }

        public void AddNodeIntoFiber(Guid fiberId)
        {
            _aggregate.When(new AddNodeIntoFiber() { Id = Guid.NewGuid(), FiberId = fiberId });
        }
        #endregion

        public void AddFiber(Guid left, Guid right)
        {
            Error = _aggregate.When(new AddFiber()
            {
                Id = Guid.NewGuid(),
                Node1 = left,
                Node2 = right
            });
        }

        public void AddFiberWithNodes(Guid left, Guid right, int intermediateNodeCount, EquipmentType equipmentType)
        {
            var cmd = new AddFiberWithNodes()
            {
                Id = Guid.NewGuid(),
                Node1 = left,
                Node2 = right,
                IntermediateNodesCount = intermediateNodeCount,
                EquipmentInIntermediateNodesType = equipmentType,
            };

            if (_aggregate.When(cmd) != null)
                return;
        }

        public void AddRtuAtGpsLocation()
        {
            _aggregate.When(new AddRtuAtGpsLocation() { Id = Guid.NewGuid(), NodeId = Guid.NewGuid() });
        }

        public string this[string columnName]
        {
            get { throw new NotImplementedException(); }
        }

        public string Error { get; private set; }
    }
}
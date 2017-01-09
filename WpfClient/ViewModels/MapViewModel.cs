using System;
using System.ComponentModel;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public class GpsCoors
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
    public sealed class MapViewModel : IDataErrorInfo
    {
        private readonly Aggregate _aggregate;
        private readonly ReadModel _readModel;
        private GpsCoors _currentMousePosition = new GpsCoors();

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
        public void MoveNode(Guid id)
        {
            var cmd = new MoveNode() { Id = id, Latitude = _currentMousePosition.Latitude, Longitude = _currentMousePosition.Longitude };
            _aggregate.When(cmd);
        }
        #endregion

        #region Fiber
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
        #endregion

        public void AddRtuAtGpsLocation()
        {
            _aggregate.When(new AddRtuAtGpsLocation() { Id = Guid.NewGuid(), NodeId = Guid.NewGuid(), Latitude = _currentMousePosition.Latitude, Longitude = _currentMousePosition.Longitude} );
        }
        public void AddEquipmentAtGpsLocation()
        {
            _aggregate.When(new AddEquipmentAtGpsLocation() { Id = Guid.NewGuid(), NodeId = Guid.NewGuid(), Latitude = _currentMousePosition.Latitude, Longitude = _currentMousePosition.Longitude } );
        }

        public void DefineTrace()
        {
            var rtuNodeId = Guid.NewGuid();
            var lastNodeId = Guid.NewGuid();
            new PathFinder(_readModel).FindPath(rtuNodeId, lastNodeId);
        }

        public string this[string columnName]
        {
            get { throw new NotImplementedException(); }
        }

        public string Error { get; private set; }
    }
}
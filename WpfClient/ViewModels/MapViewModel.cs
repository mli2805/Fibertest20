using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
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

        public MapViewModel(Aggregate aggregate, ReadModel readModel)
        {
            _aggregate = aggregate;
            _readModel = readModel;
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

        public void RemoveNode(Guid id)
        {
            if (_readModel.Traces.Any(t => t.Nodes.Last() == id))
                return; // It's prohibited to remove last node from trace
            if (_readModel.Traces.Any(t => t.Nodes.Contains(id) && t.HasBase))
                return; // It's prohibited to remove any node from trace with base ref
            _aggregate.When(new RemoveNode() {Id = id});
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
        public void RemoveFiber(Guid fiberId)
        {
            var cmd = new RemoveFiber() { Id = fiberId };
            _aggregate.When(cmd);
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

        public void DefineTrace(Guid rtuNodeId, Guid lastNodeId)
        {
            var path = new PathFinder(_readModel).FindPath(rtuNodeId, lastNodeId);
            if (path == null)
            {
                var windowManager = IoC.Get<IWindowManager>();
                var addEquipmentViewModel = new ErrorNotificationViewModel("Path couldn't be found");
                windowManager.ShowDialog(addEquipmentViewModel);
            }
            else
            {
                var nodes = path.ToList();
                var equipments = CollectEquipmentForTrace(nodes);
                if (equipments == null)
                    return;

                var windowManager = IoC.Get<IWindowManager>();
                var addEquipmentViewModel = new AddTraceViewModel(_readModel, _aggregate, nodes, equipments);
                windowManager.ShowDialog(addEquipmentViewModel);
            }
        }

        //TODO: требуется реальное наполнение c запросами пользователю и проверкой, что последний узел содержит оборудование
        private List<Guid> CollectEquipmentForTrace(List<Guid> nodes)
        {
            var equipments = new List<Guid>();
            foreach (var nodeId in nodes)
            {
                var equipment = _readModel.Equipments.FirstOrDefault(e => e.NodeId == nodeId);
                equipments.Add(equipment?.Id ?? Guid.Empty);
            }
            return equipments;
        }


        public string this[string columnName]
        {
            get { throw new NotImplementedException(); }
        }

        public string Error { get; private set; }
    }
}
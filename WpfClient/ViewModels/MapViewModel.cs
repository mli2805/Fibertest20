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
                var windowManager = IoC.Get<IWindowManager>();
                var addEquipmentViewModel = new AddTraceViewModel(_readModel, _aggregate, path.ToList());
                windowManager.ShowDialog(addEquipmentViewModel);
            }
        }

        public string this[string columnName]
        {
            get { throw new NotImplementedException(); }
        }

        public string Error { get; private set; }
    }
}
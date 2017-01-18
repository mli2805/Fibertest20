using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public sealed class MapViewModel : IDataErrorInfo
    {
        public readonly Aggregate Aggregate;
        public readonly ReadModel ReadModel;
        private GpsLocation _currentMousePosition = new GpsLocation();
        private readonly IWindowManager _windowManager;
        public AddTraceViewModel AddTraceViewModel;

        public MapViewModel(
            Aggregate aggregate, ReadModel readModel,
            IWindowManager windowManager)
        {
            Aggregate = aggregate;
            ReadModel = readModel;
            _windowManager = windowManager;
        }

        #region Node
        public void AddNode()
        {
            Aggregate.When(new AddNode { Id = Guid.NewGuid() });
        }


        public bool IsAddNodeIntoFiberMenuItemEnabled(int x, int y)
        {
            // как будто пользователь кликнул на файбере правой кнопкой
            // должно быть построено попап меню, в котором надо решить
            // кнопка Добавить узел в отрезок/трассы доступна или нет
            //            Guid fiberId = GetFiberFromMouse(x, y);
            Guid fiberId = Guid.NewGuid();

            return !ReadModel.IsFiberContainedInTraceWithBase(fiberId);
        }

        /// <summary>
        /// узел (GPS location) будет добавлен во все трассы , поэтому если  хоть одна с базовой - отклоняем
        /// 
        /// а еще пользователь может сразу добавить оборудование в узел
        /// и выбрать какие трассы должны использовать это оборудование, может быть ни одна
        /// </summary>
        /// <param name="fiberId"></param>
        public void AddNodeIntoFiber(Guid fiberId)
        {
            var center = GetFiberCenter(fiberId);

            var userChoice = EquipmentType.CableReserve;
            var equipmentId = userChoice == EquipmentType.None ? Guid.Empty : Guid.NewGuid();
            var userList = new List<Guid>(); // список трасс куда должно быть добавлено оборудование

            var result = Aggregate.When(new AddNodeIntoFiber
            {
                Id = Guid.NewGuid(),
                FiberId = fiberId,
                NewFiberId1 = Guid.NewGuid(),
                NewFiberId2 = Guid.NewGuid(),
                Latitude = center.Latitude,
                Longitude = center.Longitude,
                EqType = userChoice,
                EquipmentId = equipmentId,
                TracesConsumingEquipment = userList
            });
            if (result != null)
            {
                var errorNotificationViewModel = new ErrorNotificationViewModel(result);
                _windowManager.ShowDialog(errorNotificationViewModel);
            }
        }

        private GpsLocation GetFiberCenter(Guid fiberId)
        {
            var fiber = ReadModel.Fibers.Single(f => f.Id == fiberId);
            var node1 = ReadModel.Nodes.Single(n => n.Id == fiber.Node1);
            var node2 = ReadModel.Nodes.Single(n => n.Id == fiber.Node2);
            return new GpsLocation { Latitude = (node1.Latitude + node2.Latitude) / 2, Longitude = (node1.Longitude + node2.Longitude) / 2 };
        }

        public void MoveNode(Guid id)
        {
            var cmd = new MoveNode { Id = id, Latitude = _currentMousePosition.Latitude, Longitude = _currentMousePosition.Longitude };
            Aggregate.When(cmd);
        }

        public void RemoveNode(Guid id)
        {
            if (ReadModel.Traces.Any(t => t.Nodes.Last() == id))
                return; // It's prohibited to remove last node from trace
            if (ReadModel.Traces.Any(t => t.Nodes.Contains(id) && t.HasBase))
                return; // It's prohibited to remove any node from trace with base ref

            var dictionary = ReadModel.Traces.Where(t => t.Nodes.Contains(id)).ToDictionary(trace => trace.Id, trace => Guid.NewGuid());
            Aggregate.When(new RemoveNode { Id = id, TraceFiberPairForDetour = dictionary });
        }
        #endregion

        #region Fiber
        public void AddFiber(Guid left, Guid right)
        {
            Error = Aggregate.When(new AddFiber
            {
                Id = Guid.NewGuid(),
                Node1 = left,
                Node2 = right
            });
        }

        public string AddFiberWithNodes(Guid left, Guid right, int intermediateNodeCount, EquipmentType equipmentType)
        {
            var cmd = new AddFiberWithNodes
            {
                Node1 = left,
                Node2 = right,
                IntermediateNodesCount = intermediateNodeCount,
                EquipmentInIntermediateNodesType = equipmentType
            };

            return Aggregate.When(cmd);
        }
        public void RemoveFiber(Guid fiberId)
        {
            var cmd = new RemoveFiber { Id = fiberId };
            Aggregate.When(cmd);
        }

        #endregion

        #region Rtu
        public void AddRtuAtGpsLocation()
        {
            Aggregate.When(new AddRtuAtGpsLocation
            {
                Id = Guid.NewGuid(),
                NodeId = Guid.NewGuid(),
                Latitude = _currentMousePosition.Latitude,
                Longitude = _currentMousePosition.Longitude
            });
        }

        public void RemoveRtu(Guid rtuId)
        {
            if (ReadModel.Traces.Any(t => t.RtuId == rtuId)) // эта проверка должна быть еще раньше - при показе контекстного меню РТУ
                return;
            Aggregate.When(new RemoveRtu { Id = rtuId });
        }
        #endregion

        #region Equipment
        public void AddEquipmentAtGpsLocation(EquipmentType type)
        {
            Aggregate.When(new AddEquipmentAtGpsLocation
            {
                Id = Guid.NewGuid(),
                NodeId = Guid.NewGuid(),
                Type = type,
                Latitude = _currentMousePosition.Latitude,
                Longitude = _currentMousePosition.Longitude
            });
        }
        #endregion

        #region Trace

        public void DefineTraceClick()
        {
            // использование этого варианта требует сделать ReadModel и Aggregate public
            this.DefineTrace(_windowManager, ReadModel.Rtus.First().NodeId, ReadModel.Equipments.Last().NodeId);

            // при использовании этого варианта ReadModel и Aggregate остаются private, но не понятно как тестировать запуск формы AddTraceViewModel
//            MapTraceDefineProcess.DefineTrace(ReadModel, Aggregate, _windowManager, ReadModel.Rtus.First().Id, ReadModel.Nodes.Last().Id);
        }

        public void AttachTrace(AttachTrace cmd)
        {
            Aggregate.When(cmd);
        }

        public void DetachTrace(DetachTrace cmd)
        {
            Aggregate.When(cmd);
        }
        #endregion


        public string this[string columnName]
        {
            get { throw new NotImplementedException(); }
        }

        public string Error { get; private set; }
    }
}
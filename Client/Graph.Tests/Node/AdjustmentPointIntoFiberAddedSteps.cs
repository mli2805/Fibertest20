using System;
using System.Globalization;
using System.Linq;
using Autofac;
using FluentAssertions;
using GMap.NET;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class AdjustmentPointIntoFiberAddedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginAsRoot(Answer.Yes);
        private Guid _leftNodeId;
        private Guid _rightNodeId;
        private Guid _fiberId, _fiberId1, _fiberId2;
        private Guid _adjustmentPointId;

        private string _initialGpsLength;

        [Given(@"Задан отрезок")]
        public void GivenЗаданОтрезок()
        {
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode, Latitude = 55, Longitude = 30 }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _leftNodeId = _sut.ReadModel.Nodes.Last().NodeId;
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode, Latitude = 55.5, Longitude = 30.5 }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _rightNodeId = _sut.ReadModel.Nodes.Last().NodeId;
            _sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { FiberId = _fiberId, NodeId1 = _leftNodeId, NodeId2 = _rightNodeId }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _fiberId = _sut.ReadModel.Fibers.Last().FiberId;
        }

        [Given(@"Запоминаем его GPS длину")]
        public void GivenЗапоминаемЕгоGpsДлину()
        {
            var vm = _sut.ClientScope.Resolve<FiberUpdateViewModel>();
            vm.Initialize(_fiberId).Wait();
            _initialGpsLength = vm.GpsLength;
        }

        [When(@"Добавляем точку привязки в произвольном месте отрезка")]
        public void WhenДобавляемТочкуПрявязкиВПроизвольномМестеОтрезка()
        {
            _sut.GraphReadModel.GrmNodeRequests.AddNodeIntoFiber(new RequestAddNodeIntoFiber()
                { FiberId = _fiberId, Position = new PointLatLng(55.1, 30.1), InjectionType = EquipmentType.AdjustmentPoint}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _adjustmentPointId = _sut.ReadModel.Nodes.Last().NodeId;
            _fiberId1 = _sut.ReadModel.Fibers[0].FiberId;
            _fiberId2 = _sut.ReadModel.Fibers[1].FiberId;
        }

        [Then(@"При просмотре любого из двух получившихся отрезков показывает первоначальную длину")]
        public void ThenПриПросмотреЛюбогоИзДвухПолучившихсяОтрезковПоказываетПервоначальнуюДлину()
        {
            foreach (var fiber in _sut.ReadModel.Fibers)
            {
                var vm = _sut.ClientScope.Resolve<FiberUpdateViewModel>();
                vm.Initialize(fiber.FiberId).Wait();
                vm.GpsLength.Should().Be(_initialGpsLength);
            }
        }

        [When(@"Двигаем точку привязки")]
        public void WhenДвигаемТочкуПривязки()
        {
            _sut.GraphReadModel.GrmNodeRequests.MoveNode(new MoveNode() { NodeId = _adjustmentPointId, Latitude = 55.2, Longitude = 30.1 }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"При просмотре любого из двух отрезков показывает одинаковую длину больше первоначальной")]
        public void ThenПриПросмотреЛюбогоИзДвухОтрезковПоказываетОдинаковуюДлинуБольшеПервоначальной()
        {
            var initialGpsInt = int.Parse(_initialGpsLength, NumberStyles.Any, CultureInfo.CurrentUICulture);

            var vm1 = _sut.ClientScope.Resolve<FiberUpdateViewModel>();
            vm1.Initialize(_fiberId1).Wait();
            var gpsInt1 = int.Parse(vm1.GpsLength, NumberStyles.Any, CultureInfo.CurrentUICulture);

            var vm2 = _sut.ClientScope.Resolve<FiberUpdateViewModel>();
            vm2.Initialize(_fiberId2).Wait();
            var gpsInt2 = int.Parse(vm2.GpsLength, NumberStyles.Any, CultureInfo.CurrentUICulture);

            gpsInt1.Should().Be(gpsInt2);
            (gpsInt1 - initialGpsInt > 0).Should().BeTrue();
        }

    }
}

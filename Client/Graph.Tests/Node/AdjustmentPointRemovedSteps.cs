using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class AdjustmentPointRemovedSteps
    {
        private SystemUnderTest _sut = new SystemUnderTest().LoginOnEmptyBaseAsRoot();
        private Guid _fiberId;
        private Guid _n1, _n2, _nodeWithPointId, _middleNodeForDeletion;

        [Given(@"Существует два узела и отрезок между ними")]
        public void GivenСуществуетДваУзелаИОтрезокМеждуНими()
        {
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            _sut.Poller.EventSourcingTick().Wait();

            _n1 = _sut.GraphReadModel.Data.Nodes.Last().Id;
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _n2 = _sut.GraphReadModel.Data.Nodes.Last().Id;

            _sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber(){FiberId = Guid.NewGuid(), NodeId1 = _n1, NodeId2 = _n2}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _fiberId = _sut.GraphReadModel.Data.Fibers.Last().Id;
        }

        [Given(@"На отрезке добавлена точка привязки")]
        public void GivenНаОтрезкеДобавленаТочкаПривязки()
        {
            _sut.GraphReadModel.GrmNodeRequests.AddNodeIntoFiber(new RequestAddNodeIntoFiber(){FiberId = _fiberId, InjectionType = EquipmentType.AdjustmentPoint}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _nodeWithPointId = _sut.GraphReadModel.Data.Nodes.Last().Id;
        }

        [When(@"Пользователь удаляет точку привязки")]
        public void WhenПользовательУдаляетТочкуПривязки()
        {
            _sut.GraphReadModel.GrmNodeRequests.RemoveNode(_nodeWithPointId, EquipmentType.AdjustmentPoint).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Узел с точкой удаляется, но между узлами создается новое волокно")]
        public void ThenУзелСТочкойУдаляетсяНоМеждуУзламиСоздаетсяНовоеВолокно()
        {
            _sut.GraphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == _nodeWithPointId).Should().BeNull();
            _sut.GraphReadModel.Data.Fibers.FirstOrDefault(f => f.Node1.Id == _n1 && f.Node2.Id == _n2).Should().NotBeNull();
            _sut.GraphReadModel.Data.Fibers.Count.Should().Be(1);

            _sut.ReadModel.Nodes.FirstOrDefault(n => n.NodeId == _nodeWithPointId).Should().BeNull();
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.NodeId1 == _n1 && f.NodeId2 == _n2).Should().NotBeNull();
            _sut.ReadModel.Fibers.Count.Should().Be(1);
        }

        [When(@"Пользователь удаляет крайний узел")]
        public void WhenПользовательУдаляетКрайнийУзел()
        {
            _sut.GraphReadModel.GrmNodeRequests.RemoveNode(_n2, EquipmentType.EmptyNode).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Удаляются узел, точка привязки и оба куска волокна")]
        public void ThenУдаляютсяУзелТочкаПривязкиИОбаКускаВолокна()
        {
            _sut.GraphReadModel.Data.Nodes.Count.Should().Be(1);
            _sut.GraphReadModel.Data.Nodes[0].Id.Should().Be(_n1);

            _sut.ReadModel.Nodes.Count.Should().Be(1);
            _sut.ReadModel.Nodes[0].NodeId.Should().Be(_n1);
        }

        [Given(@"Между точкой и крайним узлом добавлен еще узел")]
        public void GivenМеждуТочкойИКрайнимУзломДобавленЕщеУзел()
        {
            var fiber = _sut.ReadModel.Fibers.First(f => f.NodeId1 == _n1 && f.NodeId2 == _nodeWithPointId ||
                                                         f.NodeId2 == _n1 && f.NodeId1 == _nodeWithPointId);

            _sut.GraphReadModel.GrmNodeRequests.AddNodeIntoFiber(new RequestAddNodeIntoFiber() { FiberId = fiber.FiberId, InjectionType = EquipmentType.EmptyNode }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _middleNodeForDeletion = _sut.GraphReadModel.Data.Nodes.Last().Id;
        }

        [When(@"Удаляем этот узел")]
        public void WhenУдаляемЭтотУзел()
        {
            _sut.GraphReadModel.GrmNodeRequests.RemoveNode(_middleNodeForDeletion, EquipmentType.EmptyNode).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Удаляются все волокна и точка привязки")]
        public void ThenУдаляютсяВсеВолокнаИТочкаПривязки()
        {
            _sut.GraphReadModel.Data.Fibers.Count.Should().Be(0);
            _sut.GraphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == _nodeWithPointId).Should().BeNull();
        }

    }
}

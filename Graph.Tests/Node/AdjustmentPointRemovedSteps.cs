using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class AdjustmentPointRemovedSteps
    {
        private SystemUnderTest _sut = new SystemUnderTest();
        private Guid _fiberId;
        private Guid _n1, _n2, _nodeWithPointId;

        [Given(@"Существует два узела и отрезок между ними")]
        public void GivenСуществуетДваУзелаИОтрезокМеждуНими()
        {
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            _sut.Poller.EventSourcingTick().Wait();

            _n1 = _sut.GraphReadModel.Nodes.Last().Id;
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _n2 = _sut.GraphReadModel.Nodes.Last().Id;

            _sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber(){Id = Guid.NewGuid(), Node1 = _n1, Node2 = _n2}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _fiberId = _sut.GraphReadModel.Fibers.Last().Id;
        }

        [Given(@"На отрезке добавлена точка привязки")]
        public void GivenНаОтрезкеДобавленаТочкаПривязки()
        {
            _sut.GraphReadModel.GrmNodeRequests.AddNodeIntoFiber(new RequestAddNodeIntoFiber(){FiberId = _fiberId, InjectionType = EquipmentType.AdjustmentPoint}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _nodeWithPointId = _sut.GraphReadModel.Nodes.Last().Id;
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
            _sut.GraphReadModel.Nodes.FirstOrDefault(n => n.Id == _nodeWithPointId).Should().BeNull();
            _sut.GraphReadModel.Fibers.FirstOrDefault(f => f.Node1.Id == _n1 && f.Node2.Id == _n2).Should().NotBeNull();
            _sut.GraphReadModel.Fibers.Count.Should().Be(1);

            _sut.ReadModel.Nodes.FirstOrDefault(n => n.Id == _nodeWithPointId).Should().BeNull();
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.Node1 == _n1 && f.Node2 == _n2).Should().NotBeNull();
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
            _sut.GraphReadModel.Nodes.Count.Should().Be(1);
            _sut.GraphReadModel.Nodes[0].Id.Should().Be(_n1);

            _sut.ReadModel.Nodes.Count.Should().Be(1);
            _sut.ReadModel.Nodes[0].Id.Should().Be(_n1);
        }

    }
}

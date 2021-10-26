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
    public sealed class FiberRemovedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginAsRoot(Answer.Yes);
        private Guid _leftNodeId;
        private Guid _rightNodeId;
        private Guid _fiberId;
        

        [Given(@"Есть два узла и отрезок между ними")]
        public void GivenЕстьДваУзлаИОтрезокМеждуНими()
        {
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _leftNodeId = _sut.ReadModel.Nodes.Last().NodeId;
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _rightNodeId = _sut.ReadModel.Nodes.Last().NodeId;
            _sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() {NodeId1 = _leftNodeId, NodeId2 = _rightNodeId}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _fiberId = _sut.ReadModel.Fibers.Last().FiberId;
        }

        [When(@"Пользователь кликает удалить отрезок")]
        public void WhenПользовательКликаетУдалитьОтрезок()
        {
            _sut.GraphReadModel.GrmFiberRequests.RemoveFiber(new RemoveFiber() {FiberId = _fiberId}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Отрезок удаляется")]
        public void ThenОтрезокУдаляется()
        {
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.FiberId == _fiberId).Should().Be(null);
        }
    }
}

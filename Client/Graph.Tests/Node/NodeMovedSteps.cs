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
    public sealed class NodeMovedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginAsRoot();

        private Guid _nodeId;
        private int _cutOff;

        [Given(@"Создан узел")]
        public void GivenNodeAdded()
        {
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode, Latitude = 55.1, Longitude = 30.1 }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _cutOff = _sut.CurrentEventNumber;
            _nodeId = _sut.ReadModel.Nodes.Last().NodeId;
        }

        [When(@"Пользователь подвинул узел")]
        public void WhenUserMovedNode()
        {
            _sut.GraphReadModel.GrmNodeRequests.MoveNode(new MoveNode() {NodeId = _nodeId, Latitude = 55.2, Longitude = 30.2 }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Новые координаты должны быть сохранены")]
        public void ThenНовыеКоординатыДолжныБытьСохранены()
        {
            _sut.CurrentEventNumber.Should().BeGreaterThan(_cutOff);
        }

    }
}

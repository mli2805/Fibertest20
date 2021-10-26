using FluentAssertions;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph.Requests;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class NodeAddedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginAsRoot(Answer.Yes);
        private int _cutOff;
      

        [When(@"Пользователь кликает добавить узел")]
        public void WhenUserClicksAddNode()
        {
            _cutOff = _sut.ReadModel.Nodes.Count;
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode }).Wait();
        }

        [Then(@"Новый узел сохраняется")]
        public void ThenTheNewNodeGetSaved()
        {
            _sut.Poller.EventSourcingTick().Wait();
            _sut.ReadModel.Nodes.Count.Should().BeGreaterThan(_cutOff);
        }

    }
}
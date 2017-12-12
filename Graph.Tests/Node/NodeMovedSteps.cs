using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class NodeMovedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();

        private Guid _nodeId;
        private int _cutOff;

        [Given(@"Создан узел")]
        public void GivenNodeAdded()
        {
            _sut.ShellVm.ComplyWithRequest(new AddNode()).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _cutOff = _sut.CurrentEventNumber;
            _nodeId = _sut.ReadModel.Nodes.Last().Id;
        }

        [When(@"Пользователь подвинул узел")]
        public void WhenUserMovedNode()
        {
            _sut.ShellVm.ComplyWithRequest(new MoveNode() {Id = _nodeId}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Новые координаты должны быть сохранены")]
        public void ThenНовыеКоординатыДолжныБытьСохранены()
        {
            _sut.CurrentEventNumber.Should().BeGreaterThan(_cutOff);
        }

    }
}

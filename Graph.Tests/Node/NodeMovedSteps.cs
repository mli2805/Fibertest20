using System;
using System.Linq;
using FluentAssertions;
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
            _sut.MapVm.AddNode();
            _sut.Poller.Tick();
            _cutOff = _sut.CurrentEventNumber;
            _nodeId = _sut.ReadModel.Nodes.Single().Id;
        }

        [When(@"Пользователь подвинул узел")]
        public void WhenUserMovedNode()
        {
            _sut.MapVm.MoveNode(_nodeId);
            _sut.Poller.Tick();
        }

        [Then(@"Новые координаты должны быть сохранены")]
        public void ThenНовыеКоординатыДолжныБытьСохранены()
        {
            _sut.CurrentEventNumber.Should().BeGreaterThan(_cutOff);
        }

    }
}

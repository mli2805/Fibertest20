using System;
using System.Linq;
using Caliburn.Micro;
using FluentAssertions;
using Iit.Fibertest.WpfClient.ViewModels;
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
            _sut.Map.AddNode();
            _sut.Poller.Tick();
            _nodeId = _sut.ReadModel.Nodes.Single().Id;
        }

        [When(@"Пользователь подвинул узел")]
        public void WhenUserMovedNode()
        {
            _sut.Map.MoveNode(_nodeId);
        }

        [Then(@"Новые координаты должны быть сохранены")]
        public void ThenНовыеКоординатыДолжныБытьСохранены()
        {
            _sut.CurrentEventNumber.Should().BeGreaterThan(_cutOff);
        }

    }
}

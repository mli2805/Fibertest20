using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class NodeMovedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private MapViewModel _mapViewModel;

        private Guid _nodeId;
        private int _cutOff;

        public NodeMovedSteps()
        {
            _mapViewModel = new MapViewModel(_sut.Aggregate, _sut.ReadModel);
        }

        [Given(@"Создан узел")]
        public void GivenNodeAdded()
        {
            _mapViewModel.AddNode();
            _sut.Poller.Tick();
            _nodeId = _sut.ReadModel.Nodes.Single().Id;
        }

        [When(@"Пользователь подвинул узел")]
        public void WhenUserMovedNode()
        {
            _mapViewModel.MoveNode(_nodeId);
        }

        [Then(@"Новые координаты должны быть сохранены")]
        public void ThenНовыеКоординатыДолжныБытьСохранены()
        {
            _sut.CurrentEventNumber.Should().BeGreaterThan(_cutOff);
        }

    }
}

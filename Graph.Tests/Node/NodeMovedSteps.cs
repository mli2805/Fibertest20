using System;
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
            _mapViewModel = new MapViewModel(_sut.Aggregate);
        }

        [Given(@"Создан узел")]
        public void GivenNodeAdded()
        {
            _nodeId = _mapViewModel.AddNode();
        }

        [When(@"Пользователь подвинул узел")]
        public void WhenUserMovedNode()
        {
            _sut.MoveNode(_nodeId);
        }

        [Then(@"Новые координаты должны быть сохранены")]
        public void ThenНовыеКоординатыДолжныБытьСохранены()
        {
            _sut.CurrentEventNumber.Should().BeGreaterThan(_cutOff);
        }

    }
}

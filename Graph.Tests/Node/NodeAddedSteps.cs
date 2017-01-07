using FluentAssertions;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class NodeAddedSteps
    {
        private MapViewModel _mapViewModel;
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private int _cutOff;

        public NodeAddedSteps()
        {
            _mapViewModel = new MapViewModel(_sut.Aggregate);

        }

        [When(@"Пользователь кликает добавить узел")]
        public void WhenUserClicksAddNode()
        {
            _mapViewModel.AddNode();
        }

        [Then(@"Новый узел сохраняется")]
        public void ThenTheNewNodeGetSaved()
        {
            _sut.Poller.Tick();
            _sut.CurrentEventNumber.Should().BeGreaterThan(_cutOff);
        }

    }
}
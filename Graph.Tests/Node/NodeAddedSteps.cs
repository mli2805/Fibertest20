using Caliburn.Micro;
using FluentAssertions;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class NodeAddedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private int _cutOff;
      

        [When(@"Пользователь кликает добавить узел")]
        public void WhenUserClicksAddNode()
        {
            _sut.Map.AddNode();
        }

        [Then(@"Новый узел сохраняется")]
        public void ThenTheNewNodeGetSaved()
        {
            _sut.Poller.Tick();
            _sut.CurrentEventNumber.Should().BeGreaterThan(_cutOff);
        }

    }
}
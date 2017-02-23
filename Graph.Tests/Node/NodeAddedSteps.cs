using FluentAssertions;
using Iit.Fibertest.Graph;
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
            _cutOff = _sut.ReadModel.Nodes.Count;
            _sut.ShellVm.ComplyWithRequest(new AddNode()).Wait();
        }

        [Then(@"Новый узел сохраняется")]
        public void ThenTheNewNodeGetSaved()
        {
            _sut.Poller.Tick();
            _sut.ReadModel.Nodes.Count.Should().BeGreaterThan(_cutOff);
        }

    }
}
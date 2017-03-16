using FluentAssertions;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuAtGpsLocationAddedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private int _rtuCutOff;

        [When(@"Пользователь кликает добавить РТУ")]
        public void WhenUserClicksAddRtu()
        {
            _sut.Poller.Tick();
            _rtuCutOff = _sut.ReadModel.Rtus.Count;
            _sut.ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation()).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"Новый РТУ сохраняется")]
        public void ThenNewRtuPersisted()
        {
            _sut.ReadModel.Rtus.Count.Should().Be(_rtuCutOff+1);
        }


    }
}

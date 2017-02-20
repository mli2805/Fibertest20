using FluentAssertions;
using Iit.Fibertest.Graph.Commands;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuAtGpsLocationAddedSteps
    {
        private readonly SystemUnderTest2 _sut = new SystemUnderTest2();
        private int _rtuCutOff;

        [When(@"Пользователь кликает добавить РТУ")]
        public void WhenUserClicksAddRtu()
        {
            _sut.Poller.Tick();
            _rtuCutOff = _sut.ReadModel.Rtus.Count;
            _sut.ShellVm.ComplyWithRequest(new AddRtuAtGpsLocation()).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"Новый РТУ сохраняется")]
        public void ThenNewRtuPersisted()
        {
            _sut.ReadModel.Rtus.Count.Should().Be(_rtuCutOff+1);
        }


    }
}

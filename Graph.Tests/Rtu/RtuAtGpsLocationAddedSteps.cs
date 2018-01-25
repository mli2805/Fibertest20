using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
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
            _sut.Poller.EventSourcingTick().Wait();
            _rtuCutOff = _sut.ReadModel.Rtus.Count;
            _sut.ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation()).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Новый РТУ сохраняется")]
        public void ThenNewRtuPersisted()
        {
            _sut.ReadModel.Rtus.Count.Should().Be(_rtuCutOff+1);
            var rtu = _sut.ReadModel.Rtus.Last();
            var rtuLeaf = (RtuLeaf)_sut.ShellVm.TreeOfRtuModel.Tree.GetById(rtu.Id);
            rtuLeaf.MainChannelState.Should().Be(RtuPartState.NotSetYet);
            rtuLeaf.ReserveChannelState.Should().Be(RtuPartState.NotSetYet);
            rtuLeaf.TreeOfAcceptableMeasParams.Should().BeNull();
        }


    }
}

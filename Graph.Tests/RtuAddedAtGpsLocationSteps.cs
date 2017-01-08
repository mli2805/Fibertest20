using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuAddedAtGpsLocationSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private readonly MapViewModel _vm;
        private Guid _rtuId;

        public RtuAddedAtGpsLocationSteps()
        {
            _vm = new MapViewModel(_sut.Aggregate);

        }

        [When(@"Пользователь кликает добавить РТУ")]
        public void WhenUserClicksAddRtu()
        {
            _vm.AddRtuAtGpsLocation();
            _sut.Poller.Tick();
            _rtuId = _sut.ReadModel.Rtus.Single().Id;
        }

        [Then(@"Новый РТУ сохраняется")]
        public void ThenNewRtuPersisted()
        {
            _sut.ReadModel.Rtus.Single().Id.Should().Be(_rtuId);
        }


    }
}

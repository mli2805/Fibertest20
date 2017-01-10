using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuAtGpsLocationAddedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private readonly MapViewModel _vm;

        public RtuAtGpsLocationAddedSteps()
        {
            _vm = new MapViewModel(_sut.Aggregate, _sut.ReadModel);

        }

        [When(@"Пользователь кликает добавить РТУ")]
        public void WhenUserClicksAddRtu()
        {
            _vm.AddRtuAtGpsLocation();
            _sut.Poller.Tick();
        }

        [Then(@"Новый РТУ сохраняется")]
        public void ThenNewRtuPersisted()
        {
            _sut.ReadModel.Rtus.Count.Should().Be(1);
        }


    }
}

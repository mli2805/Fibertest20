using FluentAssertions;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuAtGpsLocationAddedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();

        [When(@"Пользователь кликает добавить РТУ")]
        public void WhenUserClicksAddRtu()
        {
            _sut.Map.AddRtuAtGpsLocation();
            _sut.Poller.Tick();
        }

        [Then(@"Новый РТУ сохраняется")]
        public void ThenNewRtuPersisted()
        {
            _sut.ReadModel.Rtus.Count.Should().Be(1);
        }


    }
}

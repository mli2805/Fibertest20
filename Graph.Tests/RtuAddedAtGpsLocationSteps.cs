using FluentAssertions;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuAddedAtGpsLocationSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private int _cutOff;

        [When(@"Пользователь кликает добавить РТУ")]
        public void WhenUserClicksAddRtu()
        {
            _sut.AddRtuAtGpsLocation();
        }

        [Then(@"Новый РТУ сохраняется")]
        public void ThenNewRtuPersisted()
        {
            _sut.CurrentEventNumber.Should().BeGreaterThan(_cutOff);
        }


    }
}

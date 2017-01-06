using System;
using FluentAssertions;
using Iit.Fibertest.Graph.Commands;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceAddedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private int _cutOff;

        [When(@"Пользователь подтверждает создание трассы")]
        public void WhenПользовательПодтверждаетСозданиеТрассы()
        {
            var cmd = new AddTrace() {Id = Guid.NewGuid()};
            _sut.AddTrace(cmd);
        }

        [Then(@"Новая трасса сохраняется")]
        public void ThenНоваяТрассаСохраняется()
        {
            _sut.CurrentEventNumber.Should().BeGreaterThan(_cutOff);
        }

    }
}

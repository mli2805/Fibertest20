using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph.Commands;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceDetachedSteps
    {
        private readonly SystemUnderTest2 _sut = new SystemUnderTest2();
        private Guid _traceId;

        [Given(@"Создана трассу")]
        public void GivenСозданаТрассу()
        {
            _sut.CreateTraceRtuEmptyTerminal();
            _sut.Poller.Tick();
            _traceId = _sut.ReadModel.Traces.Last().Id;
        }

        [Given(@"Трасса присоединена к порту РТУ")]
        public void GivenТрассаПрисоединенаКПортуРту()
        {
            var cmd2 = new AttachTrace()
            {
                Port = 3,
                TraceId = _traceId
            };
            _sut.ShellVm.ComplyWithRequest(cmd2).Wait();
            _sut.Poller.Tick();
        }

        [When(@"Пользователь отсоединяет трассу")]
        public void WhenПользовательОтсоединяетТрассу()
        {
            var cmd = new DetachTrace() {TraceId = _traceId};
            _sut.ShellVm.ComplyWithRequest(cmd).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"Трасса отсоединена")]
        public void ThenТрассаОтсоединена()
        {
            _sut.ReadModel.Traces.Single(t => t.Id == _traceId).Port.Should().Be(-1);
        }

    }
}

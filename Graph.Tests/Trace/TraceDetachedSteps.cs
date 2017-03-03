using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceDetachedSteps
    {
        private readonly SutForTraceSimpleOperations _sut = new SutForTraceSimpleOperations();
        private Guid _traceId;

        [Given(@"Создана трассу")]
        public void GivenСозданаТрассу()
        {
            _traceId = _sut.CreateTraceRtuEmptyTerminal().Id;
        }

        [Given(@"Трасса присоединена к порту РТУ")]
        public void GivenТрассаПрисоединенаКПортуРту()
        {
            _sut.InitializeRtu(_sut.ReadModel.Traces.First(t => t.Id == _traceId).RtuId, 8);
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

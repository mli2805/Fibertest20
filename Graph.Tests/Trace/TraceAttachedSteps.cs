using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph.Commands;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceAttachedSteps
    {
        private readonly SystemUnderTest2 _sut = new SystemUnderTest2();
        private Guid _traceId;
        private int _portNumber;

        [Given(@"Создаем трассу")]
        public void GivenЕстьТрасса()
        {
            _sut.CreateTraceRtuEmptyTerminal();
            _sut.Poller.Tick();
            _traceId = _sut.ReadModel.Traces.Last().Id;
        }

        [When(@"Пользователь присоедининяет трассу к порту РТУ")]
        public void WhenПользовательПрисоедининяетТрассуКПортуРту()
        {
            _portNumber = 3;
            _traceId = _sut.ReadModel.Traces.Single().Id;
            var cmd = new AttachTrace()
            {
                Port = _portNumber,
                TraceId = _traceId
            };
            _sut.ShellVm.ComplyWithRequest(cmd).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"Трасса присоединяется к порту РТУ")]
        public void ThenТрассаПрисоединяетсяКПортуРту()
        {
            _sut.ReadModel.Traces.Single(t => t.Id == _traceId).Port.Should().Be(_portNumber);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Iit.Fibertest.Graph.Commands;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceAttachedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _traceId;
        private int _portNumber;

        [Given(@"Существует трасса")]
        public void GivenСуществуетТрасса()
        {
            _traceId = Guid.NewGuid();
            var cmd = new AddTrace() { Id = _traceId };
            _sut.AddTrace(cmd);
        }

        [When(@"Пользователь присоедининяет трассу к порту РТУ")]
        public void WhenПользовательПрисоедининяетТрассуКПортуРТУ()
        {
            _portNumber = 3;
            var cmd = new AttachTrace()
            {
                Port = _portNumber,
                TraceId = _traceId
            };
            _sut.AttachTrace(cmd);
        }

        [Then(@"Трасса присоединяется к порту РТУ")]
        public void ThenТрассаПрисоединяетсяКПортуРТУ()
        {
            _sut.ReadModel.Traces.Single(t => t.Id == _traceId).Port.Should().Be(_portNumber);
        }

    }
}

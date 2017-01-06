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
    public sealed class TraceDetachedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _traceId;

        [Given(@"Есть трасса присоединенная к порту РТУ")]
        public void GivenЕстьТрассаПрисоединеннаяКПортуРТУ()
        {
            _traceId = Guid.NewGuid();
            var cmd1 = new AddTrace() { Id = _traceId };
            _sut.AddTrace(cmd1);

            var cmd2 = new AttachTrace()
            {
                Port = 3,
                TraceId = _traceId
            };
            _sut.AttachTrace(cmd2);
        }

        [When(@"Пользователь отсоединяет трассу")]
        public void WhenПользовательОтсоединяетТрассу()
        {
            var cmd = new DetachTrace() {TraceId = _traceId};
            _sut.DetachTrace(cmd);
        }

        [Then(@"Трасса отсоединена")]
        public void ThenТрассаОтсоединена()
        {
            _sut.ReadModel.Traces.Single(t => t.Id == _traceId).Port.Should().Be(-1);
        }

    }
}

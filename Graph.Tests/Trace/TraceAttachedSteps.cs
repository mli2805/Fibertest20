using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceAttachedSteps
    {
//        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private readonly SystemUnderTest _sut;
        private Guid _traceId;
        private int _portNumber;

        private readonly MapViewModel _vm;

        public TraceAttachedSteps(SystemUnderTest sut)
        {
            _sut = sut;
            _vm = new MapViewModel(_sut.Aggregate, _sut.ReadModel);
        }


        [When(@"Пользователь присоедининяет трассу к порту РТУ")]
        public void WhenПользовательПрисоедининяетТрассуКПортуРТУ()
        {
            _portNumber = 3;
            _traceId = _sut.ReadModel.Traces.Single().Id;
            var cmd = new AttachTrace()
            {
                Port = _portNumber,
                TraceId = _traceId
            };
            _vm.AttachTrace(cmd);
            _sut.Poller.Tick();
        }

        [Then(@"Трасса присоединяется к порту РТУ")]
        public void ThenТрассаПрисоединяетсяКПортуРТУ()
        {
            _sut.ReadModel.Traces.Single(t => t.Id == _traceId).Port.Should().Be(_portNumber);
        }

    }
}

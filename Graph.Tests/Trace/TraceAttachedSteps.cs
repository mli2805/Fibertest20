using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceAttachedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _traceId;
        private int _portNumber;

        private readonly MapViewModel _vm;
        private Guid _nodeForRtuId;
        private Guid _firstNodeId;

        public TraceAttachedSteps()
        {
            _vm = new MapViewModel(_sut.Aggregate);
        }

        [Given(@"Есть трасса1")]
        public void GivenЕстьТрасса()
        {
            _vm.AddRtuAtGpsLocation();
            _sut.Poller.Tick();
            _nodeForRtuId = _sut.ReadModel.Nodes.Single().Id;
            _vm.AddNode();
            _vm.AddNode();
            _sut.Poller.Tick();
            _firstNodeId = _sut.ReadModel.Nodes[1].Id;
            var secondNodeId = _sut.ReadModel.Nodes.Last().Id;
            _vm.AddFiber(_nodeForRtuId, _firstNodeId);
            _vm.AddFiber(_firstNodeId, secondNodeId);
            _sut.Poller.Tick();
            var addTraceViewModel = new AddTraceViewModel(_sut.ReadModel, _sut.Aggregate, new List<Guid>() { _nodeForRtuId, _firstNodeId, secondNodeId });
            addTraceViewModel.Save();
            _sut.Poller.Tick();
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

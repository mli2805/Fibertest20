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
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _traceId;
        private int _portNumber;

        private readonly MapViewModel _vm;

        public TraceAttachedSteps()
        {
            _vm = new MapViewModel(_sut.Aggregate, _sut.ReadModel);
        }

        [Given(@"Есть трасса1")]
        public void GivenЕстьТрасса()
        {
            _vm.AddRtuAtGpsLocation();
            _sut.Poller.Tick();
            var nodeForRtuId = _sut.ReadModel.Nodes.Single().Id;
            _vm.AddNode();
            _vm.AddNode();
            _sut.Poller.Tick();
            var firstNodeId = _sut.ReadModel.Nodes[1].Id;
            var secondNodeId = _sut.ReadModel.Nodes.Last().Id;
            _vm.AddFiber(nodeForRtuId, firstNodeId);
            _vm.AddFiber(firstNodeId, secondNodeId);
            _sut.Poller.Tick();
            var equipments = new List<Guid>();
            var addTraceViewModel = new AddTraceViewModel(_sut.ReadModel, _sut.Aggregate, new List<Guid>() { nodeForRtuId, firstNodeId, secondNodeId }, equipments);
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

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
    public sealed class TraceDetachedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _traceId;
        private int _portNumber;

        private readonly MapViewModel _vm;
        private Guid _nodeForRtuId;
        private Guid _firstNodeId;

        public TraceDetachedSteps()
        {
            _vm = new MapViewModel(_sut.Aggregate, _sut.ReadModel);
        }

        [Given(@"Есть трасса присоединенная к порту РТУ")]
        public void GivenЕстьТрассаПрисоединеннаяКПортуРТУ()
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
            var equipments = new List<Guid>();
            var addTraceViewModel = new AddTraceViewModel(_sut.ReadModel, _sut.Aggregate, new List<Guid>() { _nodeForRtuId, _firstNodeId, secondNodeId }, equipments);
            addTraceViewModel.Save();
            _sut.Poller.Tick();

            var cmd2 = new AttachTrace()
            {
                Port = 3,
                TraceId = _traceId
            };
            _vm.AttachTrace(cmd2);
        }

        [When(@"Пользователь отсоединяет трассу")]
        public void WhenПользовательОтсоединяетТрассу()
        {
            var cmd = new DetachTrace() {TraceId = _traceId};
            _vm.DetachTrace(cmd);
        }

        [Then(@"Трасса отсоединена")]
        public void ThenТрассаОтсоединена()
        {
            _sut.ReadModel.Traces.Single(t => t.Id == _traceId).Port.Should().Be(-1);
        }

    }
}

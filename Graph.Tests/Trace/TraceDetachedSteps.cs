using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceDetachedSteps
    {
        private readonly SutForTraceAttach _sut = new SutForTraceAttach();
        private Guid _traceId;
        Guid _rtuId;
        int _portNumber;
        private RtuLeaf _rtuLeaf;

        [Given(@"Создана трассу инициализирован РТУ")]
        public void GivenСозданаТрассуИнициализированРту()
        {
            _rtuLeaf = _sut.TraceCreatedAndRtuInitialized(out _traceId, out _rtuId);
        }

        [Given(@"Трасса присоединена к порту РТУ")]
        public void GivenТрассаПрисоединенаКПортуРту()
        {
            _portNumber = 3;
            _sut.AttachTraceTo(_traceId, _rtuLeaf, _portNumber, Answer.Yes);
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
                (_sut.ShellVm.MyLeftPanelViewModel.TreeReadModel.Tree.GetById(_rtuId).Children[_portNumber - 1] is
                    PortLeaf).Should().BeTrue();
        }

    }
}

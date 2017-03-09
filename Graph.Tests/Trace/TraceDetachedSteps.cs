using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceDetachedSteps
    {
        private readonly SutForTraceAttach _sut = new SutForTraceAttach();
        private Guid _traceId;
        private Guid _rtuId;
        private int _portNumber = 3;
        private RtuLeaf _rtuLeaf;
        private OtauLeaf _otauLeaf;

        [Given(@"Создана трассу инициализирован РТУ")]
        public void GivenСозданаТрассуИнициализированРту()
        {
            _rtuLeaf = _sut.TraceCreatedAndRtuInitialized(out _traceId, out _rtuId);
        }

        [Given(@"Трасса присоединена к порту РТУ")]
        public void GivenТрассаПрисоединенаКПортуРту()
        {
            _sut.AttachTraceTo(_traceId, _rtuLeaf, _portNumber, Answer.Yes);
        }

        [Given(@"Подключен переключатель")]
        public void GivenПодключенПереключатель()
        {
            _otauLeaf = _sut.AttachOtauToRtu(_rtuLeaf, 3);
        }

        [Given(@"Трасса присоединена к порту переключателя")]
        public void GivenТрассаПрисоединенаКПортуПереключателя()
        {
            _sut.AttachTraceTo(_traceId, _otauLeaf, _portNumber, Answer.Yes);
        }

        [When(@"Пользователь отсоединяет трассу")]
        public void WhenПользовательОтсоединяетТрассу()
        {
            var traceLeaf = (TraceLeaf)_sut.ShellVm.MyLeftPanelViewModel.TreeReadModel.Tree.GetById(_traceId);
            traceLeaf.DetachTraceAction(null);
            _sut.Poller.Tick();
        }

        [Then(@"Трасса отсоединена")]
        public void ThenТрассаОтсоединена()
        {
            _sut.ReadModel.Traces.Single(t => t.Id == _traceId).Port.Should().BeLessThan(1);

            var portLeaf = _rtuLeaf.Children[_portNumber - 1] as PortLeaf;
            portLeaf.Should().NotBeNull();
            portLeaf?.Name.Should().Be(string.Format(Resources.SID_Port_N, _portNumber));
        }

        [Then(@"Трасса отсоединена от переключателя")]
        public void ThenТрассаОтсоединенаОтПереключателя()
        {
            _sut.ReadModel.Traces.Single(t => t.Id == _traceId).Port.Should().BeLessThan(1);
            var portLeaf = _otauLeaf.Children[_portNumber - 1] as PortLeaf;
            portLeaf.Should().NotBeNull();
            portLeaf?.Name.Should().
                Be(string.Format(Resources.SID_Port_N_on_otau, _portNumber, _otauLeaf.FirstPortNumber + _portNumber - 1));
        }

    }
}

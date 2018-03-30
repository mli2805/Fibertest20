using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.StringResources;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceDetachedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _traceId;
        private int _portNumber = 3;
        private RtuLeaf _rtuLeaf;
        private TraceLeaf _traceLeaf;
        private OtauLeaf _otauLeaf;

        [Given(@"Создана трассу инициализирован РТУ")]
        public void GivenСозданаТрассуИнициализированРту()
        {
            _rtuLeaf = _sut.TraceCreatedAndRtuInitialized(out _traceId, out _);
            _traceLeaf = (TraceLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_traceId);
        }

        [Given(@"Трасса присоединена к порту РТУ")]
        public void GivenТрассаПрисоединенаКПортуРту()
        {
            _traceLeaf = _sut.AttachTraceTo(_traceId, _rtuLeaf, _portNumber, Answer.Yes);
        }

        [Given(@"Подключен переключатель")]
        public void GivenПодключенПереключатель()
        {
            _otauLeaf = _sut.AttachOtau(_rtuLeaf, 3);
        }

        [Given(@"Трасса присоединена к порту переключателя")]
        public void GivenТрассаПрисоединенаКПортуПереключателя()
        {
            _traceLeaf = _sut.AttachTraceTo(_traceId, _otauLeaf, _portNumber, Answer.Yes);
        }

        [When(@"Пользователь отсоединяет трассу")]
        public void WhenПользовательОтсоединяетТрассу()
        {
            _traceLeaf.MyContextMenu.First(i =>  i?.Header == Resources.SID_Unplug_trace).Command.Execute(_traceLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Трасса отсоединена")]
        public void ThenТрассаОтсоединена()
        {
            _sut.ReadModel.Traces.Single(t => t.TraceId == _traceId).Port.Should().BeLessThan(1);

            var portLeaf = _rtuLeaf.ChildrenImpresario.Children[_portNumber - 1] as PortLeaf;
            portLeaf.Should().NotBeNull();
            portLeaf?.Name.Should().Be(string.Format(Resources.SID_Port_N, _portNumber));
        }

        [Then(@"Трасса отсоединена от переключателя")]
        public void ThenТрассаОтсоединенаОтПереключателя()
        {
            _sut.ReadModel.Traces.Single(t => t.TraceId == _traceId).Port.Should().BeLessThan(1);
            var portLeaf = _otauLeaf.ChildrenImpresario.Children[_portNumber - 1] as PortLeaf;
            portLeaf.Should().NotBeNull();
            portLeaf?.Name.Should().
                Be(string.Format(Resources.SID_Port_N, _portNumber));
        }

    }
}

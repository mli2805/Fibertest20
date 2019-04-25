using System;
using System.Linq;
using System.Windows.Media;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class BopEventAddedSteps
    {
        private SystemUnderTest _sut = new SystemUnderTest();
        private Guid _rtuId;
        private Iit.Fibertest.Graph.Trace _trace;
        private RtuLeaf _rtuLeaf;
        private OtauLeaf _otauLeaf;
        private string _otauIp;
        private int _otauTcpPort;

        [Given(@"Есть инициализированный RTU")]
        public void GivenЕстьИнициализированныйRtu()
        {
            _trace = _sut.CreateTraceRtuEmptyTerminal();
            _rtuId = _trace.RtuId;
            _rtuLeaf = _sut.SetNameAndAskInitializationRtu(_rtuId);
            _rtuLeaf.BopState.Should().Be(RtuPartState.NotSetYet);
        }

        [When(@"Пользователь присоединяет OTAU с адресом (.*) (.*)")]
        public void WhenПользовательПрисоединяетOTAUСАдресом_(string p0, int p1)
        {
            _otauIp = p0;
            _otauTcpPort = p1;
            _otauLeaf = _sut.AttachOtau(_rtuLeaf, 3, _otauIp, _otauTcpPort);
        }

        [Then(@"У OTAU зеленый квадрат и у RTU на месте для БОПа зеленый квадрат")]
        public void ThenУotauЗеленыйКвадратИуrtuНаМестеДляБоПаЗеленыйКвадрат()
        {
            _sut.ReadModel.Otaus.First().IsOk.Should().Be(true);
            _rtuLeaf.BopState.Should().Be(RtuPartState.Ok);
            _otauLeaf.OtauState.Should().Be(RtuPartState.Ok);
        }


        [When(@"Приходит сообщение OTAU (.*) исправен")]
        public void WhenПриходитСообщениеOTAU_Исправен(string p0)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BopStateHandler(model));
            OtauStateMsmqCome(p0, true);
        }

        [When(@"Приходит сообщение OTAU (.*) НЕисправен")]
        public void WhenПриходитСообщениеOTAU_НЕисправен(string p0)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BopStateHandler(model));
            OtauStateMsmqCome(p0, false);
        }

        private void OtauStateMsmqCome(string serial, bool isOk)
        {
            var dto = new BopStateChangedDto()
            {
                RtuId = _rtuId,
                Serial = serial,
                IsOk = isOk,
            };
            _sut.MsmqMessagesProcessor.ProcessBopStateChanges(dto).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }


        [Then(@"Событие отражается в обеих таблицах на вкладке сетевых событий БОП")]
        public void ThenСобытиеОтражаетсяВОбеихТаблицахНаВкладкеСетевыхСобытийБоп()
        {
            var rows = _sut.ShellVm.BopNetworkEventsDoubleViewModel.ActualBopNetworkEventsViewModel.Rows;
            rows.Count.Should().Be(1);
            rows.First().OtauIp.Should().Be(_otauIp);
            rows.First().StateBrush.Should().Be(Brushes.Red);

            rows = _sut.ShellVm.BopNetworkEventsDoubleViewModel.AllBopNetworkEventsViewModel.Rows;
            rows.Count.Should().Be(1);
            rows.First().OtauIp.Should().Be(_otauIp);
            rows.First().StateBrush.Should().Be(Brushes.Red);
        }

        [Then(@"В актуальных аварийное событие пропадает а во всех появляется событие ок")]
        public void ThenВАктуальныхАварийноеСобытиеПропадаетАВоВсехПоявляетсяСобытиеОк()
        {
            var rows = _sut.ShellVm.BopNetworkEventsDoubleViewModel.ActualBopNetworkEventsViewModel.Rows;
            rows.Count.Should().Be(0);

            rows = _sut.ShellVm.BopNetworkEventsDoubleViewModel.AllBopNetworkEventsViewModel.Rows;
            rows.Count.Should().Be(2);
            rows.First().StateBrush.Should().Be(Brushes.Red);
            rows.Last().StateBrush.Should().Be(Brushes.Transparent);
        }


        [Then(@"Открывается форма Состояние БОП - в поле состояние - авария")]
        public void ThenОткрываетсяФормаСостояниеБОП_ВПолеСостояние_Авария()
        {
            var bopStateViewsManager = _sut.ClientScope.Resolve<BopStateViewsManager>();
            bopStateViewsManager.LaunchedViews.Count.Should().Be(1);
            var pair = bopStateViewsManager.LaunchedViews.First();
            pair.Key.Should().Be(_rtuId);
            var vm = pair.Value;
            vm.BopState.Should().Be(Resources.SID_Bop_breakdown);
        }

        [Then(@"На форме Состояние БОП в поле состояние становится ОК")]
        public void ThenНаФормеСостояниеБопвПолеСостояниеСтановитсяОк()
        {
            var bopStateViewsManager = _sut.ClientScope.Resolve<BopStateViewsManager>();
            bopStateViewsManager.LaunchedViews.Count.Should().Be(1);
            var pair = bopStateViewsManager.LaunchedViews.First();
            pair.Key.Should().Be(_rtuId);
            var vm = pair.Value;
            vm.BopState.Should().Be(Resources.SID_OK_BOP);
        }


        [Then(@"В дереве у RTU для БОПа красный квадрат и у самого OTAU красный квадрат")]
        public void ThenВДеревеУrtuДляБоПаКрасныйКвадратИуСамогоOtauКрасныйКвадрат()
        {
            _rtuLeaf.BopState.Should().Be(RtuPartState.Broken);
            _otauLeaf.OtauState.Should().Be(RtuPartState.Broken);
        }
    }
}
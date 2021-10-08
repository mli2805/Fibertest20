using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    // ReSharper disable once InconsistentNaming
    public sealed class BaseRefRtu400Steps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Iit.Fibertest.Graph.Trace _trace;
        private RtuLeaf _rtuLeaf;
        private TraceLeaf _traceLeaf;

        [Given(@"Инициализирован VeexRtu и на нем неприсоединенная трасса")]
        public void GivenИнициализированVeexRtuИНаНемНеприсоединеннаяТрасса()
        {
            _trace = _sut.CreateTraceRtuEmptyTerminal();
            _traceLeaf = (TraceLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(_trace.TraceId);
            _rtuLeaf = (RtuLeaf)_traceLeaf.Parent;
            _sut.SetNameAndAskInitializationRtu(_rtuLeaf.Id, @"1.1.1.1", "", 80);
        }

        // такое название, чтобы подчеркнуть, что пользователь может задать базовые как через десктопный так и через вэб клиент
        // а тестируется только от момента прихода dto на сервер до применения к Модели
        [When(@"Пользователь присылает на сервер базовые")]
        public void WhenПользовательПрисылаетНаСерверБазовые()
        {
            _sut.AssignBaseRef(_traceLeaf, SystemUnderTest.Base1625, SystemUnderTest.Base1625, null, Answer.Yes);
            _sut.Poller.EventSourcingTick().Wait();
        }

        // такое название, чтобы подчеркнуть, что пользователь может подать команду как через десктопный так и через вэб клиент
        // а тестируется только от момента прихода dto на сервер до применения к Модели
        [When(@"Пользователь присылает на сервер команду присоединить трассу к порту")]
        public void WhenПользовательПрисылаетНаСерверКомандуПрисоединитьТрассуКПорту()
        {
            var dto = new AttachTraceDto()
            {
                RtuMaker = RtuMaker.VeEX,
                TraceId = _trace.TraceId,
                OtauPortDto = new OtauPortDto()
                {
                    IsPortOnMainCharon = true,
                    OpticalPort = 7,
                }
            };
            _sut.WcfServiceCommonC2D.AttachTraceAndSendBaseRefs(dto).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"В таблице виикс-тестов появляется две записи")]
        public void ThenВТаблицеВиикс_ТестовПоявляетсяДвеЗаписи()
        {
            _sut.ReadModel.VeexTests.Count.ShouldBeEquivalentTo(2);
        }

        [When(@"Пользователь присылает на сервер команду отсоединитть трассу от порта")]
        public void WhenПользовательПрисылаетНаСерверКомандуОтсоединиттьТрассуОтПорта()
        {
            // new instance of traceLeaf created when trace attached
            var traceLeaf = (TraceLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(_trace.TraceId);
            traceLeaf.MyContextMenu.First(i =>  i?.Header == Resources.SID_Unplug_trace).Command.Execute(_traceLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Пользователь очищает базовые у трассы")]
        public void WhenПользовательОчищаетБазовыеУТрассы()
        {
            _sut.AssignBaseRef(_traceLeaf, "", "", null, Answer.Yes);
        }

        [Then(@"В таблице виикс-тестов не остается записей")]
        public void ThenВТаблицеВиикс_ТестовНеОстаетсяЗаписей()
        {
            _sut.ReadModel.VeexTests.Count.ShouldBeEquivalentTo(0);
        }
    }
}

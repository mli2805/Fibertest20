using FluentAssertions;
using Iit.Fibertest.Client;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class OtauOnRtu4000Steps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginOnEmptyBaseAsRoot();
        private Iit.Fibertest.Graph.Trace _trace;
        private RtuLeaf _rtuLeaf;
        private OtauLeaf _otauLeaf;
        private TraceLeaf _traceLeaf;

        [Given(@"Существует инициализированный VeexRTU с неприсоединенной трассой")]
        public void GivenСуществуетИнициализированныйVeexRtuсНеприсоединеннойТрассой()
        {
            _trace = _sut.CreateTraceRtuEmptyTerminal();
            _traceLeaf = (TraceLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(_trace.TraceId);
            _rtuLeaf = (RtuLeaf)_traceLeaf.Parent;
            _sut.SetNameAndAskInitializationRtu(_rtuLeaf.Id, @"1.1.1.1", "", 80);
        }

        [Given(@"Задаем трассе базовые")]
        public void GivenЗадаемТрассеБазовые()
        {
            _sut.AssignBaseRef(_traceLeaf, SystemUnderTest.Base1625, SystemUnderTest.Base1625, null, Answer.Yes);
            _sut.Poller.EventSourcingTick().Wait();
        }


        [When(@"Подключаем доп переключатель")]
        public void WhenПодключаемДопПереключатель()
        {
            _otauLeaf = _sut.AttachOtau(_rtuLeaf, 5, "192.168.96.237", 4001);
        }

        [When(@"Подключем трассу к порту на доп переключателе")]
        public void WhenПодключемТрассуКПортуНаДопПереключателе()
        {
            _traceLeaf =_sut.AttachTraceTo(_trace.TraceId, _otauLeaf, 2, Answer.Yes);
        }

        [Then(@"В таблице виикс-тестов будут две записи")]
        public void ThenВТаблицеВиикс_ТестовБудутДвеЗаписи()
        {
            _sut.ReadModel.VeexTests.Count.ShouldBeEquivalentTo(2);
        }

        [When(@"Отключаем доп переключатель")]
        public void WhenОтключаемДопПереключатель()
        {
            // _otauLeaf.MyContextMenu.First(i => i.Header == Resources.SID_Remove)
                // .Command.Execute(_otauLeaf);

            _otauLeaf.RemoveOtau().Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"В таблице виикс-тестов нет записей")]
        public void ThenВТаблицеВиикс_ТестовНетЗаписей()
        {
            _sut.ReadModel.VeexTests.Count.ShouldBeEquivalentTo(0);
        }

    }
}

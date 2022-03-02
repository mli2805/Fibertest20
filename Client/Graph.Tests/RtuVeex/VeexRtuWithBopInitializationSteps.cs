using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class VeexRtuWithBopInitializationSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginOnEmptyBaseAsRoot();
        private Iit.Fibertest.Graph.Trace _trace;
        private Iit.Fibertest.Graph.Rtu _rtu;
        private RtuLeaf _rtuLeaf;
        private OtauLeaf _otauLeaf;
        private TraceLeaf _traceLeaf;
        private Guid _childOtauId;

        [Given(@"Существует инициализированный Veex RTU с неприсоед трассой с базовыми")]
        public void GivenСуществуетИнициализированныйVeexRtuсНеприсоедТрассойСБазовыми()
        {
            _trace = _sut.CreateTraceRtuEmptyTerminal();
            _traceLeaf = (TraceLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(_trace.TraceId);
            _rtu = _sut.ReadModel.Rtus.Last();
            _rtuLeaf = (RtuLeaf)_traceLeaf.Parent;
            _sut.SetNameAndAskInitializationRtu(_rtuLeaf.Id, @"1.1.1.1", "", 80);

            _sut.AssignBaseRef(_traceLeaf, SystemUnderTest.Base1625, SystemUnderTest.Base1625, null, Answer.Yes);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Подключаем дополн переключатель")]
        public void WhenПодключаемДополнПереключатель()
        {
            _otauLeaf = _sut.AttachOtau(_rtuLeaf, 5, "192.168.96.237", 4001);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"В списке переключателей есть один с заполненным VeexRtuMainOtauId для этого RTU")]
        public void ThenВСпискеПереключателейЕстьОдинСЗаполненнымVeexRtuMainOtauIdДляЭтогоRtu()
        {
            _sut.ReadModel.Otaus.FirstOrDefault(o => o.VeexRtuMainOtauId.StartsWith("S1")).Should().NotBe(null);
        }

        [Then(@"есть один переключатель второго уровня для этого RTU")]
        public void ThenЕстьОдинПереключательВторогоУровняДляЭтогоRtu()
        {
            _childOtauId = _sut.ReadModel.Otaus.First(o => o.RtuId == _rtu.Id && o.VeexRtuMainOtauId == null).Id;
        }

        [When(@"Подключаем трассу к доп переключателю")]
        public void WhenПодключаемТрассуКДопПереключателю()
        {
            _traceLeaf =_sut.AttachTraceTo(_trace.TraceId, _otauLeaf, 2, Answer.Yes);
        }

        [Then(@"У трассы и VeexTestов Id доп переключателя")]
        public void ThenУТрассыИVeexTestовIdДопПереключателя()
        {
            _trace.OtauPort.OtauId.ShouldBeEquivalentTo(_childOtauId.ToString());
            _sut.ReadModel.VeexTests.First().OtauId.ShouldBeEquivalentTo(_childOtauId.ToString());
        }


        [When(@"Переинициализируем данный RTU")]
        public void WhenПереинициализируемДанныйRtu()
        {
            _sut.ReInitializeRtu(_rtu, _rtuLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Id доп переключателя не меняется")]
        public void ThenIdДопПереключателяНеМеняется()
        {
            _sut.ReadModel.Otaus.First(o => o.RtuId == _rtu.Id && o.VeexRtuMainOtauId == null).Id.ShouldBeEquivalentTo(_childOtauId);
        }

        [When(@"Заменяем сломанный RTU новым без прописанного доп переключателя")]
        public void WhenЗаменяемСломанныйRtuНовымБезПрописанногоДопПереключателя()
        {
            var childOtau = _sut.FakeVeexRtuModel.Otaus.First(o => o.id.StartsWith("S2"));
            _sut.FakeVeexRtuModel.DeleteOtau($"otaus/{childOtau.id}");
        }

        [Then(@"Дочерний переключатель получает новый Id")]
        public void ThenДочернийПереключательПолучаетНовыйId()
        {
            var newChildOtauId = _sut.ReadModel.Otaus.First(o => o.RtuId == _rtu.Id && o.VeexRtuMainOtauId == null).Id;
            newChildOtauId.Should().Be(_childOtauId);
        }

    }
}

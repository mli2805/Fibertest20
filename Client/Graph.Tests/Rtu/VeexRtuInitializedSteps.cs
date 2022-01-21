using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph.Requests;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class VeexRtuInitializedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginOnEmptyBaseAsRoot();
        private Iit.Fibertest.Graph.Rtu _rtu;
        private RtuLeaf _rtuLeaf;
        private Iit.Fibertest.Graph.Trace _trace;

        [Given(@"На карту добавлен RTU")]
        public void GivenНаКартуДобавленRtu()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Yes));
            _sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 });
            _sut.Poller.EventSourcingTick().Wait();
            _rtu = _sut.ReadModel.Rtus.Last();
            _rtuLeaf = (RtuLeaf)_sut.TreeOfRtuModel.GetById(_rtu.Id);
        }

        [When(@"Инициализируем этот RTU")]
        public void WhenИнициализируемЭтотRtu()
        {
            _sut.SetNameAndAskInitializationRtu(_rtu.Id, "11.11.11.11", null, 80);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Переинициализируем этот RTU")]
        public void WhenПереинициализируемЭтотRtu()
        {
            // main otau is broken - view should be mocked
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BopStateHandler(model));
            _sut.ReInitializeRtu(_rtu, _rtuLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"В списке переключателей появляется главный переключатель этого RTU")]
        public void ThenВСпискеПереключателейПоявляетсяГлавныйПереключательЭтогоRtu()
        {
            var mainOtau = _sut.ReadModel.Otaus.First(o => o.RtuId == _rtu.Id && o.VeexRtuMainOtauId == _rtu.MainVeexOtau.id);
            mainOtau.Should().NotBe(null);
            mainOtau.IsOk.Should().BeTrue();
        }

        [Then(@"В дереве у RTU квадратик переключателя (.*)")]
        public void ThenВДеревеУrtuКвадратикПереключателя(string p0)
        {
            var bopState = p0 == "зеленый" ? RtuPartState.Ok : RtuPartState.Broken;
            _rtuLeaf.BopState.ShouldBeEquivalentTo(bopState);
        }

        [Then(@"В дереве у этого RTU портов - (.*)")]
        public void ThenВДеревеУЭтогоRTUПортов_(int p0)
        {
            _rtuLeaf.ChildrenImpresario.Children.Count.ShouldBeEquivalentTo(p0);
        }

        [Given(@"Создаем трассу")]
        public void GivenСоздаемТрассу()
        {
            _trace = _sut.AddOneMoreTrace(_rtu.NodeId);
        }

        [Given(@"Присоединяем ее к (.*) порту")]
        public void GivenПрисоединяемЕеКПорту(int p0)
        {
            _sut.AttachTraceTo(_trace.TraceId, _rtuLeaf, p0, Answer.Yes);
        }

        [Given(@"У RTU сломался основной переключатель")]
        public void GivenУrtuСломалсяОсновнойПереключатель()
        {
            _sut.FakeVeexRtuModel.Otaus[0].connected = false;
        }

      

        [Then(@"В событиях боп строка об аварии")]
        public void ThenВСобытияхБопСтрокаОбАварии()
        {
            var bopEvent = _sut.ShellVm.BopNetworkEventsDoubleViewModel.ActualBopNetworkEventsViewModel.Rows[0];
            bopEvent.RtuId.ShouldBeEquivalentTo(_rtu.Id);
        }

    }
}

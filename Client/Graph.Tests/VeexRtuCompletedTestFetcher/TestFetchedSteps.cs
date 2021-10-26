using System.Linq;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TestFetchedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginAsRoot();
        private FakeVeexRtuModel _fakeVeexRtuModel;
        private Iit.Fibertest.Graph.Trace _trace;
        private Iit.Fibertest.Graph.Trace _traceOnBop;
        private TraceLeaf _traceLeaf;
        private TraceLeaf _traceLeafOnBop;
        private Iit.Fibertest.Graph.Rtu _rtu;
        private RtuLeaf _rtuLeaf;
        private OtauLeaf _otauLeaf;

        [Given(@"Инициализирован вииксовский RTU")]
        public void GivenИнициализированВииксовскийRtu()
        {
            _trace = _sut.CreateTraceRtuEmptyTerminal();
            _traceLeaf = (TraceLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(_trace.TraceId);
            _rtuLeaf = (RtuLeaf)_traceLeaf.Parent;
            _rtu = _sut.ReadModel.Rtus.First(r => r.Id == _rtuLeaf.Id);
            _traceOnBop = _sut.AddOneMoreTrace(_rtu.NodeId);

            _traceLeaf = (TraceLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(_trace.TraceId);
            _traceLeafOnBop = (TraceLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(_traceOnBop.TraceId);

            _fakeVeexRtuModel = _sut.Container.Resolve<FakeVeexRtuModel>();
            _sut.SetNameAndAskInitializationRtu(_rtuLeaf.Id, @"1.1.1.1", "", 80);
        }

        [Given(@"К нему подключен доп переключатель")]
        public void GivenКНемуПодключенДопПереключатель()
        {
            _otauLeaf = _sut.AttachOtau(_rtuLeaf, 5, "192.168.96.237", 4001);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Переинициализируем RTU")]
        public void WhenПереинициализируемRtu()
        {
            _sut.FakeWindowManager.RegisterHandler(m => m is MyMessageBoxViewModel);
            _sut.FakeWindowManager.RegisterHandler(model => 
                _sut.RtuInitializeHandler(model, _rtu.MainChannel.Ip4Address, "", _rtu.MainChannel.Port));
            _rtuLeaf.MyContextMenu.First(i => i?.Header == Resources.SID_Network_settings).Command.Execute(_rtuLeaf);

            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Возвращает RTU с доп переключателем и общим количеством портов")]
        public void ThenВозвращаетRtuсДопПереключателемИОбщимКоличествомПортов()
        {
            _rtu.Children[5].IsOk.ShouldBeEquivalentTo(true);
            _rtu.FullPortCount.ShouldBeEquivalentTo(48);
        }


        [Given(@"Трасса с базовыми подключена к основному")]
        public void GivenТрассаСБазовымиПодключенаКОсновному()
        {
            _sut.AssignBaseRef(_traceLeaf, SystemUnderTest.Base1625, SystemUnderTest.Base1625, null, Answer.Yes);
            _sut.Poller.EventSourcingTick().Wait();
            _traceLeaf =_sut.AttachTraceTo(_trace.TraceId, _rtuLeaf, 3, Answer.Yes);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Given(@"Трасса с базовыми подключена к дополнительному")]
        public void GivenТрассаСБазовымиПодключенаКДополнительному()
        {
            _sut.AssignBaseRef(_traceLeafOnBop, SystemUnderTest.Base1625, SystemUnderTest.Base1625, null, Answer.Yes);
            _sut.Poller.EventSourcingTick().Wait();
            _traceLeafOnBop =_sut.AttachTraceTo(_traceOnBop.TraceId, _otauLeaf, 9, Answer.Yes);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Извлекаем порцию результатов измерений с RTU")]
        public void WhenИзвлекаемПорциюРезультатовИзмеренийСrtu()
        {
            _fakeVeexRtuModel.AddOkTest(_sut.ReadModel, _trace.TraceId, BaseRefType.Fast);
            _fakeVeexRtuModel.AddOkTest(_sut.ReadModel, _traceOnBop.TraceId, BaseRefType.Fast);
            _fakeVeexRtuModel.AddOkTest(_sut.ReadModel, _trace.TraceId, BaseRefType.Precise);
            _fakeVeexRtuModel.AddOkTest(_sut.ReadModel, _traceOnBop.TraceId, BaseRefType.Precise);
            _fakeVeexRtuModel.SetSorBytesToReturn(SystemUnderTest.Base1625);
            _sut.VeexCompletedTestsFetcher.Tick().Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Первые рефки по трассам выкачиваются и сохраняются в статистике")]
        public void ThenПервыеРефкиПоТрассамВыкачиваютсяИСохраняютсяВСтатистике()
        {
            _trace.State.ShouldBeEquivalentTo(FiberState.Ok);
            _traceOnBop.State.ShouldBeEquivalentTo(FiberState.Ok);
        }

        [When(@"Приходит результат с проблемой доп переключателя")]
        public void WhenПриходитРезультатСПроблемойДопПереключателя()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BopStateHandler(model));
            _fakeVeexRtuModel.ClearTests();
            _fakeVeexRtuModel.AddFailedBopTest(_sut.ReadModel, _traceOnBop.TraceId, _otauLeaf.Id, BaseRefType.Fast);
            _fakeVeexRtuModel.AddFailedBopTest(_sut.ReadModel, _traceOnBop.TraceId, _otauLeaf.Id, BaseRefType.Precise);
            _sut.VeexCompletedTestsFetcher.Tick().Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Появляется форма авария БОП и запись на форме событий БОП")]
        public void ThenПоявляетсяФормаАварияБопиЗаписьНаФормеСобытийБоп()
        {
            _sut.ShellVm.BopNetworkEventsDoubleViewModel.ActualBopNetworkEventsViewModel.Rows.Count.ShouldBeEquivalentTo(1);
        }
       
        [When(@"Приходит успешное измерение трассы на доп переключателе")]
        public void WhenПриходитУспешноеИзмерениеТрассыНаДопПереключателе()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BopStateHandler(model));
            _fakeVeexRtuModel.ClearTests();
            _fakeVeexRtuModel.AddOkTest(_sut.ReadModel, _traceOnBop.TraceId, BaseRefType.Precise);
            _sut.VeexCompletedTestsFetcher.Tick().Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Появляется форма БОП починен и очищается запись на форме актуальных событий БОП")]
        public void ThenПоявляетсяФормаБопПочиненИОчищаетсяЗаписьНаФормеАктуальныхСобытийБоп()
        {
            _sut.ShellVm.BopNetworkEventsDoubleViewModel.ActualBopNetworkEventsViewModel.Rows.Count.ShouldBeEquivalentTo(0);
        }

    }
}

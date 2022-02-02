using System.Linq;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class BrokenRtuInitializationSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginOnEmptyBaseAsRoot();
        private Iit.Fibertest.Graph.Rtu _rtu;
        private Iit.Fibertest.Graph.Trace _trace;
        private RtuLeaf _rtuLeaf;
        private OtauLeaf _otauLeaf;

        private TraceLeaf _traceLeaf;
        [Given(@"Есть проиниченный RTU с трассой")]
        public void GivenЕстьПроиниченныйRtuсТрассой()
        {
            _trace = _sut.CreateTraceRtuEmptyTerminal();
            _traceLeaf = (TraceLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(_trace.TraceId);
            _rtu = _sut.ReadModel.Rtus.Last();
            _rtuLeaf = (RtuLeaf)_traceLeaf.Parent;
            _sut.SetNameAndAskInitializationRtu(_rtuLeaf.Id, @"1.1.1.1", "", 80);
        }

        [Given(@"Трасса подключена и заданы базовые")]
        public void GivenТрассаПодключенаИЗаданыБазовые()
        {
            _traceLeaf = _sut.AttachTraceTo(_trace.TraceId, _rtuLeaf, 3, Answer.Yes);

            _sut.AssignBaseRef(_traceLeaf, SystemUnderTest.Base1625, SystemUnderTest.Base1625, null, Answer.Yes);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Запускаем на мониторинг")]
        public void WhenЗапускаемНаМониторинг()
        {
            _sut.ApplyMoniSettings(_rtuLeaf, MonitoringState.On);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Квадратик мониторинга у RTU и трассы становится (.*)")]
        public void ThenКвадратикМониторингаУrtuиТрассыСтановится(string p0)
        {
            var pictogramUri = p0 == "голубым"
                ? @"pack://application:,,,/Resources/LeftPanel/BlueSquare.png"
                : @"pack://application:,,,/Resources/LeftPanel/GreySquare.png";
            _traceLeaf.BaseRefsSet.MonitoringPictogram.ShouldBeEquivalentTo(pictogramUri);
            _rtuLeaf.MonitoringPictogram.Should().Be(pictogramUri);
        }

        [Given(@"RTU сломался и заменен не файбертестовским в режиме прямого подключения")]
        public void GivenRtuСломалсяИЗамененНеФайбертестовскимВРежимеПрямогоПодключения()
        {
            _sut.FakeVeexRtuModel.Type = "generic";
            _sut.FakeVeexRtuModel.Monitoring = "disabled";
            _sut.FakeVeexRtuModel.ProxyMode = "enabled";
        }

        [When(@"Переинициализируем его")]
        public void WhenПереинициализируемЕго()
        {
            _sut.ReInitializeRtu(_rtu, _rtuLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Подключаем доп перекл")]
        public void WhenПодключаемДопПерекл()
        {
            _otauLeaf = _sut.AttachOtau(_rtuLeaf, 7);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Переподключаем трассу на порт доп переключателя")]
        public void WhenПереподключаемТрассуНаПортДопПереключателя()
        {
            _traceLeaf.MyContextMenu.First(i => i?.Header == Resources.SID_Unplug_trace).Command.Execute(_traceLeaf);
            _sut.Poller.EventSourcingTick().Wait();
            _traceLeaf = _sut.AttachTraceTo(_trace.TraceId, _otauLeaf, 3, Answer.Yes);
            _sut.Poller.EventSourcingTick().Wait();
            _trace.State.Should().NotBe(FiberState.NotJoined);
            _traceLeaf.TraceState.Should().NotBe(FiberState.NotJoined);
        }

      
        [Given(@"Возвращаем RTU с выключенным мониторингом")]
        public void GivenВозвращаемRtuсВыключеннымМониторингом()
        {
            _sut.FakeVeexRtuModel.Monitoring = "disabled";
        }

        [When(@"Переводим в автоматический режим")]
        public void WhenПереводимВАвтоматическийРежим()
        {
            var rtuLeafActions = _sut.ClientScope.Resolve<RtuLeafActions>();
            rtuLeafActions.StartMonitoring(_rtuLeaf).Wait();

            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Переводим в ручной режим")]
        public void WhenПереводимВРучнойРежим()
        {
            var rtuLeafActions = _sut.ClientScope.Resolve<RtuLeafActions>();
            rtuLeafActions.StopMonitoring(_rtuLeaf).Wait();
            
            _sut.Poller.EventSourcingTick().Wait();
        }

    }
}

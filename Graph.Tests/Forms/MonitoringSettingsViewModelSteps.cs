using System.Linq;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Client.MonitoringSettings;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class MonitoringSettingsViewModelSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Iit.Fibertest.Graph.Rtu _rtu;
        private RtuLeaf _rtuLeaf;
        private OtauLeaf _otauLeaf;
        private TraceLeaf _traceLeaf1, _traceLeaf2;
        private MonitoringSettingsViewModel _vm;

        [Given(@"Создан RTU")]
        public void GivenСозданRtu()
        {
            _rtu = _sut.CreateRtuA();
            _rtuLeaf = (RtuLeaf)_sut.TreeOfRtuModel.GetById(_rtu.Id);
        }

        [Then(@"Пункт Настройки мониторинга недоступен")]
        public void ThenПунктНастройкиМониторингаНедоступен()
        {
            var monitoringSettingsItem =
                _rtuLeaf.MyContextMenu.First(i => i?.Header == Resources.SID_Monitoring_settings);
            monitoringSettingsItem.Command.CanExecute(null).Should().BeFalse();
        }

        [Given(@"RTU успешно инициализирован")]
        public void GivenRtuУспешноИнициализирован()
        {
            _sut.FakeD2RWcfManager.SetFakeInitializationAnswer(waveLength: @"SM1550");
            _rtuLeaf = _sut.SetNameAndAskInitializationRtu(_rtuLeaf.Id);
        }

        [Given(@"И к порту (.*) подключеной дополнительный переключатель")]
        public void GivenИкПортуПодключенойДополнительныйПереключатель(int p0)
        {
            _otauLeaf = _sut.AttachOtau(_rtuLeaf, p0);
        }

        [Given(@"Создана трасса подключена к (.*) порту RTU и заданы базовые")]
        public void GivenСозданаТрассаПодключенаКПортуRtuиЗаданыБазовые(int p0)
        {
            var trace = _sut.SetTrace(_rtu.NodeId, @"trace1");
            _sut.AttachTraceTo(trace.TraceId, _rtuLeaf, p0, Answer.Yes);
            _traceLeaf1 = (TraceLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(trace.TraceId);

            var vm = _sut.ClientScope.Resolve<BaseRefsAssignViewModel>();
            vm.Initialize(trace);
            vm.PreciseBaseFilename = SystemUnderTest.Base1550Lm4YesThresholds;
            vm.FastBaseFilename = SystemUnderTest.Base1550Lm4YesThresholds;
            var baseRefs = vm.PrepareDto(trace).BaseRefs;

            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            var baseRefChecker = _sut.ClientScope.Resolve<BaseRefsChecker>();
            baseRefChecker.IsBaseRefsAcceptable(baseRefs, trace).Should().BeTrue();

            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            vm.Save().Wait();
        }

        [Given(@"Еще трасса только с точной базовой подключена к (.*) порту БОПа")]
        public void GivenЕщеТрассаТолькоСТочнойБазовойПодключенаКПортуБоПа(int p0)
        {
            var trace = _sut.SetTrace(_rtu.NodeId, @"trace2");
            _sut.AttachTraceTo(trace.TraceId, _otauLeaf, p0, Answer.Yes);
            _traceLeaf2 = (TraceLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(trace.TraceId);

            var vm = _sut.ClientScope.Resolve<BaseRefsAssignViewModel>();
            vm.Initialize(trace);
            vm.PreciseBaseFilename = SystemUnderTest.Base1550Lm4YesThresholds;
            var baseRefs = vm.PrepareDto(trace).BaseRefs;

            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            var baseRefChecker = _sut.ClientScope.Resolve<BaseRefsChecker>();
            baseRefChecker.IsBaseRefsAcceptable(baseRefs, trace).Should().BeTrue();

            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            vm.Save().Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Given(@"Задаем второй трассе быструю базовую")]
        public void GivenЗадаемВторойТрассеБыструюБазовую()
        {
            var trace = _sut.ReadModel.Traces.First(t => t.TraceId == _traceLeaf2.Id);
            var vm = _sut.ClientScope.Resolve<BaseRefsAssignViewModel>();
            vm.Initialize(trace);
            vm.FastBaseFilename = SystemUnderTest.Base1550Lm4YesThresholds;
            var baseRefs = vm.PrepareDto(trace).BaseRefs;
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            var baseRefChecker = _sut.ClientScope.Resolve<BaseRefsChecker>();
            baseRefChecker.IsBaseRefsAcceptable(baseRefs, trace).Should().BeTrue();

            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            vm.Save().Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }


        [When(@"Пользователь открывает Настройки мониторинга - вторая трасса недоступна для включения в цикл мониторинга")]
        public void WhenПользовательОткрываетНастройкиМониторинга_ВтораяТрассаНедоступнаДляВключенияВЦиклМониторинга()
        {
            _vm = _sut.ClientScope.Resolve<MonitoringSettingsViewModel>(new NamedParameter(@"rtuLeaf", _rtuLeaf));
            _vm.Model.Charons.Count.Should().Be(2);
            _vm.Model.Charons[0].Ports[2].TraceTitle.Should().Be(_traceLeaf1.Title);
            _vm.Model.Charons[0].Ports[2].Duration.Should().Be(@"15 / 15 sec");
            _vm.Model.Charons[0].Ports[2].IsIncluded.Should().BeFalse();
            _vm.Model.Charons[0].Ports[2].IsReadyForMonitoring.Should().BeTrue();
            _vm.Model.Charons[1].Ports[3].TraceTitle.Should().Be(_traceLeaf2.Title);
            _vm.Model.Charons[1].Ports[3].Duration.Should().Be(@"0 / 15 sec");
            _vm.Model.Charons[1].Ports[3].IsIncluded.Should().BeFalse();
            _vm.Model.Charons[1].Ports[3].IsReadyForMonitoring.Should().BeFalse();
        }

        [Then(@"Теперь вторую трассу разрешено включить в цикл мониторинга")]
        public void ThenТеперьВторуюТрассуРазрешеноВключитьВЦиклМониторинга()
        {
            _vm.Close();
            _vm = _sut.ClientScope.Resolve<MonitoringSettingsViewModel>(new NamedParameter(@"rtuLeaf", _rtuLeaf));
            _vm.Model.Charons[1].Ports[3].Duration.Should().Be(@"15 / 15 sec");
            _vm.Model.Charons[1].Ports[3].IsReadyForMonitoring.Should().BeTrue();

        }

        [When(@"Пользователь включает автоматический режим и жмет применить")]
        public void WhenПользовательВключаетАвтоматическийРежимИЖметПрименить()
        {
            _vm.Model.IsMonitoringOn = true;

            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            _vm.Apply();
        }

        [Then(@"Сообщение что не задана ни одна трасса для мониторинга")]
        public void ThenСообщениеЧтоНеЗаданаНиОднаТрассаДляМониторинга()
        {
            _sut.FakeWindowManager.Log
                  .OfType<MyMessageBoxViewModel>()
                  .Last()
                  .Lines[0].Line
                  .Should().Be(Resources.SID_There_are_no_ports_for_monitoring_);
        }


        [When(@"Пользователь ставит птичку включить все трассы главного переключателя")]
        public void WhenПользовательСтавитПтичкуВключитьВсеТрассыГлавногоПереключателя()
        {
            _vm.Model.Charons[0].GroupenCheck = true;
        }

        [When(@"Птичку включить именно вторую трассу на четвертом порту БОПа")]
        public void WhenПтичкуВключитьИменноВторуюТрассуНаЧетвертомПортуБоПа()
        {
             _vm.Model.Charons[1].Ports[3].IsIncluded = true;
        }

        [Then(@"Длительность точных базовых обоих трасс включена в длительность цикла мониторинга")]
        public void ThenДлительностьТочныхБазовыхОбоихТрассВключенаВДлительностьЦиклаМониторинга()
        {
             _vm.Model.CycleTime.Should().Be(@"00:00:34");
        }

        [When(@"Пользователь уменьшает частоту измерения по точной")]
        public void WhenПользовательУменьшаетЧастотуИзмеренияПоТочной()
        {
            _vm.Model.Frequencies.SelectedPreciseSaveFreq.Should().Be(Frequency.EveryHour);
            _vm.Model.Frequencies.SelectedPreciseMeasFreq = Frequency.Every12Hours;
        }

        [Then(@"Частота сохранения по точной изменяется соответственно")]
        public void ThenЧастотаСохраненияПоТочнойИзменяетсяСоответственно()
        {
            _vm.Model.Frequencies.SelectedPreciseSaveFreq.Should().Be(Frequency.Every12Hours);
        }

        [When(@"Пользователь включает авто режим и жмет применить")]
        public void WhenПользовательВключаетАвтоРежимИЖметПрименить()
        {
            _vm.Model.IsMonitoringOn = true;
            _vm.Apply();
        }

        [When(@"Пользователь жмет секретную комбинацию Ctrl-B для пересылки базовых на RTU")]
        public void WhenПользовательЖметСекретнуюКомбинациюCtrl_BДляПересылкиБазовыхНаRTU()
        {
            _vm.ReSendBaseRefsForAllSelectedTraces().Wait();
        }

    }
}
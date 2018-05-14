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
            _rtuLeaf = (RtuLeaf) _sut.TreeOfRtuModel.GetById(_rtu.Id);
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
            var trace1 = _sut.SetTrace(_rtu.NodeId, @"trace1");
            _sut.AttachTraceTo(trace1.TraceId, _rtuLeaf, p0, Answer.Yes);
            _traceLeaf1 = (TraceLeaf) _sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(trace1.TraceId);

            var vm = _sut.ClientScope.Resolve<BaseRefsAssignViewModel>();
            vm.Initialize(trace1);
            vm.PreciseBaseFilename = SystemUnderTest.Base1550Lm4YesThresholds;
            var baseRefs = vm.PrepareDto(trace1).BaseRefs;

            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            var baseRefChecker = _sut.ClientScope.Resolve<BaseRefsChecker>();
            baseRefChecker.IsBaseRefsAcceptable(baseRefs, trace1).Should().BeTrue();

            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            vm.Save().Wait();
        }

        [Given(@"Еще трасса с базовыми подключена к (.*) порту БОПа")]
        public void GivenЕщеТрассаСБазовымиПодключенаКПортуБоПа(int p0)
        {
            var trace1 = _sut.SetTrace(_rtu.NodeId, @"trace2");
            _sut.AttachTraceTo(trace1.TraceId, _otauLeaf, p0, Answer.Yes);
            _traceLeaf2 = (TraceLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(trace1.TraceId);

            var vm = _sut.ClientScope.Resolve<BaseRefsAssignViewModel>();
            vm.Initialize(trace1);
            vm.PreciseBaseFilename = SystemUnderTest.Base1550Lm4YesThresholds;
            var baseRefs = vm.PrepareDto(trace1).BaseRefs;

            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            var baseRefChecker = _sut.ClientScope.Resolve<BaseRefsChecker>();
            baseRefChecker.IsBaseRefsAcceptable(baseRefs, trace1).Should().BeTrue();

            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            vm.Save().Wait();
        }

        [When(@"Пользователь открывает форма Настройки мониторинга и жмет применить")]
        public void WhenПользовательОткрываетФормаНастройкиМониторингаИЖметПрименить()
        {
            _vm = _sut.ClientScope.Resolve<MonitoringSettingsViewModel>(new NamedParameter(@"rtuLeaf", _rtuLeaf));
            _vm.Model.Charons.Count.Should().Be(2);
            _vm.Model.Charons[0].Ports[2].TraceTitle.Should().Be(_traceLeaf1.Title);
            _vm.Model.Charons[0].Ports[2].IsIncluded.Should().BeFalse();
            _vm.Model.Charons[1].Ports[3].TraceTitle.Should().Be(_traceLeaf2.Title);
            _vm.Model.Charons[1].Ports[3].IsIncluded.Should().BeFalse();

      //      _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            _vm.Apply();
        }

        [When(@"Включает обе трассы в цикл мониторинга")]
        public void WhenВключаетОбеТрассыВЦиклМониторинга()
        {
            _vm.Model.Frequencies.SelectedFastSaveFreq = Frequency.Every12Hours;
            _vm.Model.Charons[0].Ports[2].IsIncluded = true;
            _vm.Model.Charons[1].Ports[3].IsIncluded = true;
        }

        [Then(@"Рассчитывается длительность цикла мониторинга")]
        public void ThenРассчитываетсяДлительностьЦиклаМониторинга()
        {
            _vm.Model.CycleTime.Should().Be(@"00:00:04");
        }

    }
}
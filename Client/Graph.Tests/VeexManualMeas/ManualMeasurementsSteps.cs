using System.Linq;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class MeasurementClientSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginOnEmptyBaseAsRoot();
        private Iit.Fibertest.Graph.Trace _trace;
        private Iit.Fibertest.Graph.Rtu _rtu;
        private RtuLeaf _rtuLeaf;
        private TraceLeaf _traceLeaf;
        private OtauLeaf _otauLeaf;

        private ClientMeasurementStartedDto _startedDto;
        private ClientMeasurementVeexResultDto _measResult;
        private ClientMeasurementVeexResultDto _measResultWithSorBytes;

        private NetAddress _preparationResult;

        [Given(@"Существует вииксовский RTU")]
        public void GivenСуществуетВииксовскийRtu()
        {
            _trace = _sut.CreateTraceRtuEmptyTerminal();
            _traceLeaf = (TraceLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(_trace.TraceId);
            _rtuLeaf = (RtuLeaf)_traceLeaf.Parent;
            _rtu = _sut.ReadModel.Rtus.First(r => r.Id == _rtuLeaf.Id);
            _sut.SetNameAndAskInitializationRtu(_rtuLeaf.Id, @"1.1.1.1", "", 80);
        }

        [Given(@"Пользователь запускает измерение Client на порту основного переключателя")]
        public void GivenПользовательЗапускаетИзмерениеClientНаПортуОсновногоПереключателя()
        {
            var vm = _sut.ClientScope.Resolve<ClientMeasurementViewModel>();

            var fullDto = vm.ForUnitTests(_rtuLeaf, 5, null, new VeexMeasOtdrParameters());
            _startedDto = _sut.WcfServiceCommonC2D.StartClientMeasurementAsync(fullDto).Result;
        }

        [When(@"Клиент запрашивает результат измерния по Id")]
        public void WhenКлиентЗапрашиваетРезультатИзмернияПоId()
        {
            var getDto = new GetClientMeasurementDto()
            {
                RtuId = _rtu.Id,
                VeexMeasurementId = _startedDto.ClientMeasurementId.ToString(), 
            };
            _measResult = _sut.WcfServiceCommonC2D.GetClientMeasurementAsync(getDto).Result;
            if (_measResult != null && _measResult.ReturnCode == ReturnCode.Ok)
            {
                _measResultWithSorBytes = _sut.WcfServiceCommonC2D.GetClientMeasurementSorBytesAsync(getDto).Result;
            }
        }

        [Then(@"Приходит рефлектограмма")]
        public void ThenПриходитРефлектограмма()
        {
            _measResultWithSorBytes.SorBytes.Length.ShouldBeEquivalentTo(32000);
        }

        [Given(@"Присоединяем доп переключатель")]
        public void GivenПрисоединяемДопПереключатель()
        {
            _otauLeaf = _sut.AttachOtau(_rtuLeaf, 5, "192.168.96.237", 4001);
        }

        [Given(@"Пользователь запускает измерение Client на порту доп переключателя")]
        public void GivenПользовательЗапускаетИзмерениеClientНаПортуДопПереключателя()
        {
            var vm = _sut.ClientScope.Resolve<ClientMeasurementViewModel>();
            // var otau = _sut.ReadModel.Otaus.FirstOrDefault(o => o.Serial == _otauLeaf.Serial);
            // var otauPortDto = vm.PrepareOtauPortDto(_rtu, otau, _otauLeaf, 7);
            // var fullDto = vm.PrepareDto(_rtu, otauPortDto, _rtuLeaf.OtauNetAddress, null, new VeexMeasOtdrParameters());

            var fullDto = vm.ForUnitTests(_otauLeaf, 7, null, new VeexMeasOtdrParameters());
            _startedDto = _sut.WcfServiceCommonC2D.StartClientMeasurementAsync(fullDto).Result;
        }

        [Given(@"Пользователь запрашивает измерение Reflect по порту основного переключателя")]
        public void GivenПользовательЗапрашиваетИзмерениеReflectПоПортуОсновногоПереключателя()
        {
            var ca = _sut.ClientScope.Resolve<CommonActions>();
            _preparationResult = ca.PrepareRtuForMeasurementReflect(_rtuLeaf, 5).Result;
        }


        [When(@"Пользователь запрашивает измерение Reflect по порту доп переключателя")]
        public void WhenПользовательЗапрашиваетИзмерениеReflectПоПортуДопПереключателя()
        {
            var ca = _sut.ClientScope.Resolve<CommonActions>();
            _preparationResult = ca.PrepareRtuForMeasurementReflect(_otauLeaf, 3).Result;
        }

        [Then(@"Отсылается команда подготовки RTU")]
        public void ThenОтсылаетсяКомандаПодготовкиRtu()
        {
            _preparationResult.ShouldBeEquivalentTo(new NetAddress("1.1.1.1", 1500));
        }
    }
}

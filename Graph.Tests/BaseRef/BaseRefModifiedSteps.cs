using System.Collections.Generic;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Optixsoft.SorExaminer.OtdrDataFormat;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
   
    [Binding]
    public sealed class BaseRefModifiedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Iit.Fibertest.Graph.Rtu _rtu;
        private Iit.Fibertest.Graph.Trace _trace;
        private List<BaseRefDto> _baseRefs;

        private int _closureLocationOnOriginalBaseRef;
        private int _emptyNodeToTheRightOfClosureLocation;
      
        [Given(@"Существует инициализированный RTU")]
        public void GivenСуществуетИнициализированныйRtu()
        {
            _rtu = _sut.SetInitializedRtu();
        }

        [Given(@"К нему нарисована (.*) с пустыми узлами")]
        public void GivenКНемуНарисована(string p0)
        {
            _trace = _sut.SetTrace(_rtu.NodeId, p0);
        }

        [When(@"Пользователь задает рефлектограмму BaseRef1")]
        public void WhenПользовательЗадаетРефлектограмму()
        {
            var vm = _sut.ClientScope.Resolve<BaseRefsAssignViewModel>();
            vm.Initialize(_trace);
            vm.PreciseBaseFilename = SystemUnderTest.Base1550Lm4YesThresholds;
            _baseRefs = vm.PrepareDto(_trace).BaseRefs;

            SorData.TryGetFromBytes(_baseRefs[0].SorBytes, out var otdrKnownBlocks);
            otdrKnownBlocks.LinkParameters.LandmarkBlocks.Length.Should().Be(4);
            var landmark = otdrKnownBlocks.LinkParameters.LandmarkBlocks[2];
            _closureLocationOnOriginalBaseRef = landmark.Location;  // 497035

            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            var baseRefChecker = _sut.ClientScope.Resolve<BaseRefsChecker>();
            baseRefChecker.IsBaseRefsAcceptable(_baseRefs, _trace).Should().BeTrue();

            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            vm.Save().Wait();
        }

        [Then(@"На рефлектограмму добавляются ориентиры соответствующие пустым узлам Но расстояние до ориентиров оборудования не изменяется")]
        public void ThenНаРефлектограммуДобавляютсяОриентирыСоответствующиеПустымУзламНоРасстояниеДоОборудованияНеИзменяется()
        {
            SorData.TryGetFromBytes(_baseRefs[0].SorBytes, out var otdrKnownBlocks);
            otdrKnownBlocks.LinkParameters.LandmarkBlocks.Length.Should().Be(9);
            var landmark = otdrKnownBlocks.LinkParameters.LandmarkBlocks[2];
            landmark.Code.Should().Be(LandmarkCode.Manhole);
            landmark.Location.Should().Be(114839);

            otdrKnownBlocks.LinkParameters.LandmarkBlocks[5].Location.Should().Be(_closureLocationOnOriginalBaseRef);
            _emptyNodeToTheRightOfClosureLocation = otdrKnownBlocks.LinkParameters.LandmarkBlocks[6].Location;
        }

        [Then(@"Расстояние до ориентиров оборудования не изменяется Расстояние до ориентиров пустых узлов правее оборудования не изменяется")]
        public void ThenРасстояниеДоОриентировОборудованияНеИзменяетсяРасстояниеДоОриентировПустыхУзловПравееОборудованияНеИзменяется()
        {
        }


        [When(@"Пользователь добавляет точки привязки и двигает их")]
        public void WhenПользовательДобавляетТочкиПривязки()
        {
            _sut.AddAdjustmentPoints(_trace);
        }

        [Then(@"При применении базовой ориентиры пустые узлы оказываются на другом расстоянии")]
        public void ThenПриПримененииБазовойОриентирыПустыхУзловОказываютсяНаДругомРасстоянии()
        {
            SorData.TryGetFromBytes(_baseRefs[0].SorBytes, out var otdrKnownBlocks);
            otdrKnownBlocks.LinkParameters.LandmarkBlocks.Length.Should().Be(9);
            var landmark = otdrKnownBlocks.LinkParameters.LandmarkBlocks[2];
            landmark.Code.Should().Be(LandmarkCode.Manhole);
            landmark.Location.Should().Be(139293);

            otdrKnownBlocks.LinkParameters.LandmarkBlocks[5].Location.Should().Be(_closureLocationOnOriginalBaseRef);
            otdrKnownBlocks.LinkParameters.LandmarkBlocks[6].Location.Should().Be(_emptyNodeToTheRightOfClosureLocation);
        }

        [When(@"Пользователь добавляет кабельный резерв в пустой узел после проверяемого")]
        public void WhenПользовательДобавляетКабельныйРезервВПустойУзелПослеПроверяемого()
        {
            _sut.AddCableReserve(_trace);
        }

        [Then(@"При применении базовой ориентиры для пустых узлов снова сдвигаются")]
        public void ThenПриПримененииБазовойОриентирыДляПустыхУзловСноваСдвигаются()
        {
            SorData.TryGetFromBytes(_baseRefs[0].SorBytes, out var otdrKnownBlocks);
            otdrKnownBlocks.LinkParameters.LandmarkBlocks.Length.Should().Be(9);
            var landmark = otdrKnownBlocks.LinkParameters.LandmarkBlocks[2];
            landmark.Code.Should().Be(LandmarkCode.Manhole);
            landmark.Location.Should().Be(139293);

            otdrKnownBlocks.LinkParameters.LandmarkBlocks[5].Location.Should().Be(_closureLocationOnOriginalBaseRef);
            otdrKnownBlocks.LinkParameters.LandmarkBlocks[6].Location.Should().Be(_emptyNodeToTheRightOfClosureLocation);
        }


    }
}
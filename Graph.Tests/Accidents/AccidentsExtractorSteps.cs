using System;
using System.IO;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.IitOtdrLibrary;
using Optixsoft.SorExaminer.OtdrDataFormat;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class AccidentsExtractorSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private OtdrDataKnownBlocks _sorData;

        private const double Precision = 0.001;
        Func<EquivalencyAssertionOptions<AccidentOnTrace>, EquivalencyAssertionOptions<AccidentOnTrace>> doubleAsserionOptions = options => options
            .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, Precision))
            .WhenTypeIs<double>();

        [Given(@"Включен мониторинг трассы")]
        public void GivenВключенМониторингТрассы()
        {
            _sut.SetTraceWithBaseRefs();
        }

        [When(@"Пришел MoniResult(.*)\.sor")]
        public void WhenПришелMoniResult_Sor(int p0)
        {
            var sorBytes = File.ReadAllBytes($@"..\..\Sut\MoniResults\moniresult{p0}.sor");
            _sorData = SorData.FromBytes(sorBytes);
        }

        [Then(@"Получен список эксидентов")]
        public void ThenПолученСписокЭксидентов()
        {
            var accidents = _sut.AccidentsExtractorFromSor.GetAccidents(_sorData, false);
            accidents.Count.Should().Be(1);
            var accident = accidents[0].Should().BeOfType<AccidentInOldEvent>().Subject;

            var expectation = new AccidentInOldEvent()
            {
                BrokenLandmarkIndex = 1,
                BrokenRftsEventNumber = 2,
                OpticalTypeOfAccident = OpticalAccidentType.Reflectance,
                AccidentDistanceKm = 0.0059,
                AccidentSeriousness = FiberState.Minor,
            };
         
            accident.ShouldBeEquivalentTo(expectation, doubleAsserionOptions);
        }

        [Then(@"Получен другой список эксидентов")]
        public void ThenПолученДругойСписокЭксидентов()
        {
            var accidents = _sut.AccidentsExtractorFromSor.GetAccidents(_sorData, false);
            accidents.Count.Should().Be(1);
            var accident = accidents[0].Should().BeOfType<AccidentInOldEvent>().Subject;

            var expectation = new AccidentInOldEvent()
            {
                BrokenLandmarkIndex = 1,
                BrokenRftsEventNumber = 2,
                OpticalTypeOfAccident = OpticalAccidentType.Reflectance,
                AccidentDistanceKm = 0.0059,
                AccidentSeriousness = FiberState.Minor,
            };

            accident.ShouldBeEquivalentTo(expectation, doubleAsserionOptions);
        }


    }
}

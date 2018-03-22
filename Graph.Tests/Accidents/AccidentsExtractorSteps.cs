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

        [When(@"Пришел (.*)\.sor")]
        public void WhenПришелMoniResult_Sor(string filename)
        {
            var sorBytes = File.ReadAllBytes($@"..\..\Sut\MoniResults\Trace4Lm\{filename}.sor");
            _sorData = SorData.FromBytes(sorBytes);
        }

        [Then(@"Получен список эксидентов для BreakBnode2")]
        public void ThenПолученСписокЭксидентов()
        {
            var accidents = _sut.AccidentsExtractorFromSor.GetAccidents(_sorData, false);
            accidents.Count.Should().Be(1);
            var accident = accidents[0].Should().BeOfType<AccidentInOldEvent>().Subject;

            var expectation = new AccidentInOldEvent()
            {
                BrokenLandmarkIndex = 2,
                BrokenRftsEventNumber = 3,
                OpticalTypeOfAccident = OpticalAccidentType.Break,
                AccidentDistanceKm = 10.1503,
                AccidentSeriousness = FiberState.FiberBreak,
            };

            accident.ShouldBeEquivalentTo(expectation, doubleAsserionOptions);
        }

        [Then(@"Получен список эксидентов для BreakBnode2-MinorRnode1")]
        public void ThenПолученДругойСписокЭксидентов()
        {
            var accidents = _sut.AccidentsExtractorFromSor.GetAccidents(_sorData, false);
            accidents.Count.Should().Be(2);
            var accident0 = accidents[0].Should().BeOfType<AccidentInOldEvent>().Subject;

            var expectation0 = new AccidentInOldEvent()
            {
                BrokenLandmarkIndex = 2,
                BrokenRftsEventNumber = 3,
                OpticalTypeOfAccident = OpticalAccidentType.Break,
                AccidentDistanceKm = 10.1503,
                AccidentSeriousness = FiberState.FiberBreak,
            };
            accident0.ShouldBeEquivalentTo(expectation0, doubleAsserionOptions);


            var accident1 = accidents[1].Should().BeOfType<AccidentInOldEvent>().Subject;
            var expectation1 = new AccidentInOldEvent()
            {
                BrokenLandmarkIndex = 1,
                BrokenRftsEventNumber = 2,
                OpticalTypeOfAccident = OpticalAccidentType.Reflectance,
                AccidentDistanceKm = 0.0059,
                AccidentSeriousness = FiberState.Minor,
            };
            accident1.ShouldBeEquivalentTo(expectation1, doubleAsserionOptions);
        }

        [Then(@"Получен список эксидентов для MajorLnode2-MinorRnode1")]
        public void ThenПолученСписокЭксидентовДляMajorLnode_MinorRnode()
        {
            var accidents = _sut.AccidentsExtractorFromSor.GetAccidents(_sorData, false);
            accidents.Count.Should().Be(2);
            var accident0 = accidents[0].Should().BeOfType<AccidentInOldEvent>().Subject;

            var expectation0 = new AccidentInOldEvent()
            {
                BrokenLandmarkIndex = 2,
                BrokenRftsEventNumber = 3,
                OpticalTypeOfAccident = OpticalAccidentType.Loss,
                AccidentDistanceKm = 10.1503,
                AccidentSeriousness = FiberState.Major,
            };
            accident0.ShouldBeEquivalentTo(expectation0, doubleAsserionOptions);


            var accident1 = accidents[1].Should().BeOfType<AccidentInOldEvent>().Subject;
            var expectation1 = new AccidentInOldEvent()
            {
                BrokenLandmarkIndex = 1,
                BrokenRftsEventNumber = 2,
                OpticalTypeOfAccident = OpticalAccidentType.Reflectance,
                AccidentDistanceKm = 0.0059,
                AccidentSeriousness = FiberState.Minor,
            };
            accident1.ShouldBeEquivalentTo(expectation1, doubleAsserionOptions);
        }

        [Then(@"Получен список эксидентов для MinorRnode1")]
        public void ThenПолученСписокЭксидентовДляMinorRnode()
        {
            var accidents = _sut.AccidentsExtractorFromSor.GetAccidents(_sorData, false);
            accidents.Count.Should().Be(1);
            var accident0 = accidents[0].Should().BeOfType<AccidentInOldEvent>().Subject;

            var expectation0 = new AccidentInOldEvent()
            {
                BrokenLandmarkIndex = 1,
                BrokenRftsEventNumber = 2,
                OpticalTypeOfAccident = OpticalAccidentType.Reflectance,
                AccidentDistanceKm = 0.0059,
                AccidentSeriousness = FiberState.Minor,
            };
            accident0.ShouldBeEquivalentTo(expectation0, doubleAsserionOptions);
        }


    }
}

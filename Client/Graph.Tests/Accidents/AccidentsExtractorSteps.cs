using System;
using System.IO;
using FluentAssertions;
using FluentAssertions.Equivalency;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Optixsoft.SorExaminer.OtdrDataFormat;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class AccidentsExtractorSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginAsRoot(Answer.Yes);
        private OtdrDataKnownBlocks _sorData;
        private Iit.Fibertest.Graph.Trace _trace;

        private const double Precision = 0.001;
        Func<EquivalencyAssertionOptions<AccidentOnTraceV2>, EquivalencyAssertionOptions<AccidentOnTraceV2>> _doubleAssertionOptions = 
            options => options
            .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, Precision))
            .WhenTypeIs<double>();


        [Given(@"Существует трасса под мониторингом")]
        public void GivenСуществуетТрассаПодМониторингом()
        {
            _trace = _sut.SetTraceWithAccidentInOldNode();
            _sut.GraphReadModel.Data.Nodes.Count.Should().Be(9);
            _sut.ReadModel.Nodes.Count.Should().Be(9);

            _trace.State.Should().Be(FiberState.NotJoined);
            _sut.AssertTraceFibersState(_trace);
            _sut.Attach(_trace, 3);

            _trace.State.Should().Be(FiberState.Unknown);
            _sut.AssertTraceFibersState(_trace);
        }


        [When(@"Пришел (.*)\.sor")]
        public void WhenПришелMoniResult_Sor(string filename)
        {
            var sorBytes = File.ReadAllBytes($@"..\..\Sut\MoniResults\{filename}.sor");
            _sorData = SorData.FromBytes(sorBytes);
        }

        [Then(@"Получен список эксидентов для BreakBnode2")]
        public void ThenПолученСписокЭксидентов()
        {
            var accidents = _sut.AccidentsFromSorExtractor.GetAccidents(_sorData, _trace.TraceId, false);
            accidents.Count.Should().Be(1);
            var accident = accidents[0];

            var expectation = new AccidentOnTraceV2()
            {
                AccidentLandmarkIndex = 5,
                BrokenRftsEventNumber = 3,
                OpticalTypeOfAccident = OpticalAccidentType.Break,
                IsAccidentInOldEvent = true,
                AccidentToRtuOpticalDistanceKm = 10.1503,
                AccidentSeriousness = FiberState.FiberBreak,
                AccidentCoors = new PointLatLng(55.08, 30.08),
                EventCode = @"R : E",
                DeltaLen = 0.0038,
                Left = new AccidentNeighbour()
                {
                    Coors = new PointLatLng(55.06, 30.06),
                    LandmarkIndex = 4,
                    ToRtuOpticalDistanceKm = 8.1308,
                },
                Right = new AccidentNeighbour()
                {
                    Coors = new PointLatLng(55.082, 30.082),
                    LandmarkIndex = 6,
                    ToRtuOpticalDistanceKm = 10.7333,
                },
            };

            accident.ShouldBeEquivalentTo(expectation, _doubleAssertionOptions);
        }

        [Then(@"Получен список эксидентов для BreakBnode2-MinorRnode1")]
        public void ThenПолученДругойСписокЭксидентов()
        {
            var accidents = _sut.AccidentsFromSorExtractor.GetAccidents(_sorData, _trace.TraceId, false);
            accidents.Count.Should().Be(2);
            var accident0 = accidents[0];

            var expectation0 = new AccidentOnTraceV2()
            {
                AccidentLandmarkIndex = 5,
                BrokenRftsEventNumber = 3,
                OpticalTypeOfAccident = OpticalAccidentType.Break,
                IsAccidentInOldEvent = true,
                AccidentToRtuOpticalDistanceKm = 10.1503,
                AccidentSeriousness = FiberState.FiberBreak,
                AccidentCoors = new PointLatLng(55.08, 30.08),
                EventCode = @"R : E",
                DeltaLen = 0.0038,
                Left = new AccidentNeighbour()
                {
                    Coors = new PointLatLng(55.06, 30.06),
                    LandmarkIndex = 4,
                    ToRtuOpticalDistanceKm = 8.1308,
                },
                Right = new AccidentNeighbour()
                {
                    Coors = new PointLatLng(55.082, 30.082),
                    LandmarkIndex = 6,
                    ToRtuOpticalDistanceKm = 10.7333,
                },
            };
            accident0.ShouldBeEquivalentTo(expectation0, _doubleAssertionOptions);


            var accident1 = accidents[1];

            var expectation1 = new AccidentOnTraceV2()
            {
                AccidentLandmarkIndex = 1,
                BrokenRftsEventNumber = 2,
                OpticalTypeOfAccident = OpticalAccidentType.Reflectance,
                IsAccidentInOldEvent = true,
                AccidentToRtuOpticalDistanceKm = 0.0059,
                AccidentSeriousness = FiberState.Minor,
                AccidentCoors = new PointLatLng(55.002, 30.002),
                EventCode = @"R : F",
                DeltaLen = 0.0038,
                Left = new AccidentNeighbour()
                {
                    Coors = new PointLatLng(55.0, 30.0),
                    LandmarkIndex = 0,
                    ToRtuOpticalDistanceKm = 0,
                    Title = @"Some title for RTU",
                },
                Right = new AccidentNeighbour()
                {
                    Coors = new PointLatLng(55.02, 30.02),
                    LandmarkIndex = 2,
                    ToRtuOpticalDistanceKm = 2.2307,
                },
            };
            accident1.ShouldBeEquivalentTo(expectation1, _doubleAssertionOptions);
        }

        [Then(@"Получен список эксидентов для MajorLnode2-MinorRnode1")]
        public void ThenПолученСписокЭксидентовДляMajorLnode_MinorRnode()
        {
            var accidents = _sut.AccidentsFromSorExtractor.GetAccidents(_sorData, _trace.TraceId, false);
            accidents.Count.Should().Be(2);
            var accident0 = accidents[0];

            var expectation0 = new AccidentOnTraceV2()
            {
                AccidentLandmarkIndex = 5,
                BrokenRftsEventNumber = 3,
                OpticalTypeOfAccident = OpticalAccidentType.Loss,
                IsAccidentInOldEvent = true,
                AccidentToRtuOpticalDistanceKm = 10.1503,
                AccidentSeriousness = FiberState.Major,
                AccidentCoors = new PointLatLng(55.08, 30.08),
                EventCode = @"R : F",
                DeltaLen = 0.0038,
                Left = new AccidentNeighbour()
                {
                    Coors = new PointLatLng(55.06, 30.06),
                    LandmarkIndex = 4,
                    ToRtuOpticalDistanceKm = 8.1308,
                },
                Right = new AccidentNeighbour()
                {
                    Coors = new PointLatLng(55.082, 30.082),
                    LandmarkIndex = 6,
                    ToRtuOpticalDistanceKm = 10.7333,
                },
            };
            accident0.ShouldBeEquivalentTo(expectation0, _doubleAssertionOptions);


            var accident1 = accidents[1];

            var expectation1 = new AccidentOnTraceV2()
            {
                AccidentLandmarkIndex = 1,
                BrokenRftsEventNumber = 2,
                OpticalTypeOfAccident = OpticalAccidentType.Reflectance,
                IsAccidentInOldEvent = true,
                AccidentToRtuOpticalDistanceKm = 0.0059,
                AccidentSeriousness = FiberState.Minor,
                AccidentCoors = new PointLatLng(55.002, 30.002),
                EventCode = @"R : F",
                DeltaLen = 0.0038,
                Left = new AccidentNeighbour()
                {
                    Coors = new PointLatLng(55.0, 30.0),
                    LandmarkIndex = 0,
                    ToRtuOpticalDistanceKm = 0,
                    Title = @"Some title for RTU",
                },
                Right = new AccidentNeighbour()
                {
                    Coors = new PointLatLng(55.02, 30.02),
                    LandmarkIndex = 2,
                    ToRtuOpticalDistanceKm = 2.2307,
                },
            };
            accident1.ShouldBeEquivalentTo(expectation1, _doubleAssertionOptions);
        }

        [Then(@"Получен список эксидентов для MinorRnode1")]
        public void ThenПолученСписокЭксидентовДляMinorRnode()
        {
            var accidents = _sut.AccidentsFromSorExtractor.GetAccidents(_sorData, _trace.TraceId, false);
            accidents.Count.Should().Be(1);
            var accident0 = accidents[0];

            var expectation0 = new AccidentOnTraceV2()
            {
                AccidentLandmarkIndex = 1,
                BrokenRftsEventNumber = 2,
                OpticalTypeOfAccident = OpticalAccidentType.Reflectance,
                IsAccidentInOldEvent = true,
                AccidentCoors = new PointLatLng(55.002, 30.002),
                AccidentToRtuOpticalDistanceKm = 0.0059,
                AccidentSeriousness = FiberState.Minor,
                EventCode = @"R : F",
                DeltaLen = 0.0038,
                Left = new AccidentNeighbour()
                {
                    Coors = new PointLatLng(55.0, 30.0),
                    LandmarkIndex = 0,
                    ToRtuOpticalDistanceKm = 0,
                    Title = @"Some title for RTU",
                },
                Right = new AccidentNeighbour()
                {
                    Coors = new PointLatLng(55.02, 30.02),
                    LandmarkIndex = 2,
                    ToRtuOpticalDistanceKm = 2.2307,
                },
            };
            accident0.ShouldBeEquivalentTo(expectation0, _doubleAssertionOptions);
        }

        [Then(@"Получен список эксидентов для DoubleMinorNode3")]
        public void ThenПолученСписокЭксидентовДляDoubleMinorNode3()
        {
            var accidents = _sut.AccidentsFromSorExtractor.GetAccidents(_sorData, _trace.TraceId, false);
            accidents.Count.Should().Be(2);
        }

    }
}

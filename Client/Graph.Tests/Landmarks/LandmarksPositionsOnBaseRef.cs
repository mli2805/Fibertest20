using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public class LandmarksPositionsOnBaseRef
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginOnEmptyBaseAsRoot();
        private Iit.Fibertest.Graph.Trace _trace;
        private Guid _saidFiberId;

        [Given(@"Задана трасса с базовой")]
        public void GivenЗаданаТрассаСБазовой()
        {
            _trace = _sut.SetTraceForLandmarks3();
            var traceLeaf = (TraceLeaf)_sut.TreeOfRtuModel.GetById(_trace.TraceId);
            _sut.AssignBaseRef(traceLeaf, SystemUnderTest.Base3Landmarks,
                SystemUnderTest.Base3Landmarks, null, Answer.Yes);
            _trace.PreciseId.Should().NotBe(Guid.Empty);
            _trace.FastId.Should().NotBe(Guid.Empty);
            _sut.Poller.EventSourcingTick().Wait();

            // on GRAPH
            TraceModelBuilder tmb = new TraceModelBuilder(new GraphGpsCalculator(_sut.ReadModel));
            var traceModel = _sut.ReadModel.GetTraceComponentsByIds(_trace);
            _saidFiberId = traceModel.FiberArray[0].FiberId;
            var tt = tmb.GetTraceModelWithoutAdjustmentPoints(traceModel);
            tt.DistancesMm[0].Should().Be(256000);

            // in SorData
            var baseRef = _sut.ReadModel.BaseRefs.First(b => b.Id == _trace.PreciseId);
            var sorBytes = _sut.WcfServiceCommonC2D.GetSorBytes(baseRef.SorFileId).Result;
            var sorData = SorData.FromBytes(sorBytes);
            var landmarksBaseParser = new LandmarksBaseParser(_sut.ReadModel);
            var landmarks = landmarksBaseParser.GetLandmarks(sorData, _trace);

            landmarks[1].Distance.ToString(CultureInfo.InvariantCulture)
                .Substring(0, 5).Should().Be("0.670");
        }

        [When(@"Пользователь задает физ длину участка (.*)")]
        public void WhenПользовательЗадаетФизДлинуУчастка(int p0)
        {
            var userInputLength = p0;
            _sut.FakeWindowManager.RegisterHandler(model => _sut.FiberUpdateHandler(model, userInputLength, Answer.Yes));
            _sut.GraphReadModel.GrmFiberRequests.UpdateFiber(new RequestUpdateFiber() { Id = _saidFiberId }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            var fiber = _sut.ReadModel.Fibers.First(f => f.FiberId == _saidFiberId);
            fiber.UserInputedLength.ShouldBeEquivalentTo(userInputLength);
        }

        [Then(@"Ориентиры сдвигаются")]
        public void ThenОриентирыСдвигаются()
        {
            var baseRef = _sut.ReadModel.BaseRefs.First(b => b.Id == _trace.PreciseId);
            var sorBytes = _sut.WcfServiceCommonC2D.GetSorBytes(baseRef.SorFileId).Result;
            var sorData = SorData.FromBytes(sorBytes);
            var landmarksBaseParser = new LandmarksBaseParser(_sut.ReadModel);
            var landmarks = landmarksBaseParser.GetLandmarks(sorData, _trace);

            ((int)Math.Round(landmarks[1].Distance * 1000)).ShouldBeEquivalentTo(102);
        }
    }
}

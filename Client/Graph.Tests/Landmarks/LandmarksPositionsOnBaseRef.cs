using System;
using System.IO;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
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
        private Guid _crossId;
        private int _crossOpticalDistance;
        private int _traceLengthOptical;

        [Given(@"Задана трасса семь узлов")]
        public void GivenЗаданаТрассаСемьУзлов()
        {
            _trace = _sut.SetTraceForLandmarks();
            _saidFiberId = _trace.FiberIds[3];
            _crossId = _trace.EquipmentIds[5];
        }

        [Given(@"Сохраняем исходные параметры из сорки")]
        public void GivenСохраняемИсходныеПараметрыИзСорки()
        {
            var buffer = File.ReadAllBytes(SystemUnderTest.BasTrace7);
            var sorData = SorData.FromBytes(buffer);
            var landmarksBaseParser = new LandmarksBaseParser(_sut.ReadModel);
            var landmarks = landmarksBaseParser.GetLandmarks(sorData, _trace);

            _traceLengthOptical = ((int)Math.Round(landmarks[6].OpticalDistance * 1000));
        }

        [Then(@"В сорке ориентир средней муфты примерно (.*)")]
        public void ThenВСоркеОриентирСреднейМуфтыПримерно(int p0)
        {
            var buffer = File.ReadAllBytes(SystemUnderTest.BasTrace7);
            var sorData = SorData.FromBytes(buffer);
            var landmarksBaseParser = new LandmarksBaseParser(_sut.ReadModel);
            var landmarks = landmarksBaseParser.GetLandmarks(sorData, _trace);

            _crossOpticalDistance = ((int)Math.Round(landmarks[4].OpticalDistance * 1000));
            (Math.Abs(_crossOpticalDistance - p0) < 5).Should().BeTrue();
        }


        [Given(@"Задаем автоматическаю базовую - только начало и конец привязаны")]
        public void GivenЗадаемАвтоматическаюБазовую_ТолькоНачалоИКонецПривязаны()
        {
            var traceLeaf = (TraceLeaf)_sut.TreeOfRtuModel.GetById(_trace.TraceId);
            _sut.AssignBaseRef(traceLeaf, SystemUnderTest.AutoBaseTrace7,
                SystemUnderTest.AutoBaseTrace7, null, Answer.Yes);
            _trace.PreciseId.Should().NotBe(Guid.Empty);
            _trace.FastId.Should().NotBe(Guid.Empty);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Given(@"Задаем ручную базовую - ориентир муфты посредине тоже привязан")]
        public void GivenЗадаемРучнуюБазовую_ОриентирМуфтыПосрединеТожеПривязан()
        {
            var traceLeaf = (TraceLeaf)_sut.TreeOfRtuModel.GetById(_trace.TraceId);
            _sut.AssignBaseRef(traceLeaf, SystemUnderTest.BasTrace7,
                SystemUnderTest.BasTrace7, null, Answer.Yes);
            _trace.PreciseId.Should().NotBe(Guid.Empty);
            _trace.FastId.Should().NotBe(Guid.Empty);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Пользователь задает физич длину участка перед муфтой (.*) метров")]
        public void WhenПользовательЗадаетФизичДлинуУчасткаПередМуфтойМетров(int p0)
        {
            var userInputLength = p0;
            _sut.FakeWindowManager.RegisterHandler(model => _sut.FiberUpdateHandler(model, userInputLength, Answer.Yes));
            _sut.GraphReadModel.GrmFiberRequests.UpdateFiber(new RequestUpdateFiber() { Id = _saidFiberId }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            var fiber = _sut.ReadModel.Fibers.First(f => f.FiberId == _saidFiberId);
            fiber.UserInputedLength.ShouldBeEquivalentTo(userInputLength);
        }

        [When(@"Пользователь вводит запас (.*) метров слева от муфты")]
        public void WhenПользовательВводитЗапасМетровСлеваОтМуфты(int p0)
        {
            var cableReserveToTheLeft = p0;
            var equipment = _sut.ReadModel.Equipments.First(e => e.EquipmentId == _crossId);
            equipment.Type.ShouldBeEquivalentTo(EquipmentType.Cross);
            equipment.CableReserveLeft = cableReserveToTheLeft;
            var mapper = new MapperConfiguration(
                cfg => cfg.AddProfile<MappingModelToCmdProfile>()).CreateMapper();
            var command = mapper.Map<UpdateEquipment>(equipment);
            _sut.WcfServiceDesktopC2D.SendCommandAsObj(command).Wait();
        }

        [Then(@"Ориентир средней муфты примерно (.*)")]
        public void ThenОриентирСреднейМуфтыПримерно(int p0)
        {
            var baseRef = _sut.ReadModel.BaseRefs.First(b => b.Id == _trace.PreciseId);
            var sorBytes = _sut.WcfServiceCommonC2D.GetSorBytes(baseRef.SorFileId).Result;
            var sorData = SorData.FromBytes(sorBytes);
            var landmarksBaseParser = new LandmarksBaseParser(_sut.ReadModel);
            var landmarks = landmarksBaseParser.GetLandmarks(sorData, _trace);

            var crossOpticalDistance = ((int)Math.Round(landmarks[4].OpticalDistance * 1000));
            (Math.Abs(crossOpticalDistance - p0) < 5).Should().BeTrue();
        }

        // private static void Log(List<Landmark> landmarks)
        // {
        //     var content = landmarks.Select(l =>
        //         { return $"{l.GpsDistance}"; }).ToList();
        //     content.AddRange(landmarks.Select(l =>
        //             { return $"{l.OpticalDistance}"; }).ToList()
        //     );
        //     File.WriteAllLines(@"c:\temp\landmarks.txt", content);
        // }

        [Then(@"Последний ориентир никогда не двигается")]
        public void ThenПоследнийОриентирНикогдаНеДвигается()
        {
            var baseRef = _sut.ReadModel.BaseRefs.First(b => b.Id == _trace.PreciseId);
            var sorBytes = _sut.WcfServiceCommonC2D.GetSorBytes(baseRef.SorFileId).Result;
            var sorData = SorData.FromBytes(sorBytes);
            var landmarksBaseParser = new LandmarksBaseParser(_sut.ReadModel);
            var landmarks = landmarksBaseParser.GetLandmarks(sorData, _trace);

            var newTraceLength = ((int)Math.Round(landmarks[6].OpticalDistance * 1000));
            (Math.Abs(newTraceLength - _traceLengthOptical) < 50).Should().BeTrue();
        }

    }
}

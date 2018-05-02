using System.Linq;
using System.Windows.Media;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuReInitializedSteps
    {
        private SystemUnderTest _sut = new SystemUnderTest();
        private Iit.Fibertest.Graph.Rtu _rtu;
        private RtuLeaf _rtuLeaf;
        private OtauLeaf _otauLeaf;
        private string _rtuAddress;
        private int _portWithBop;
        private string _newSerial;
        private int _oldOwnPortCount, _newOwnPortCount;
        private Iit.Fibertest.Graph.Trace _trace11;


        [Given(@"Задан RTU с серийником (.*) с адресом (.*) с длиной волны (.*) и с (.*) портами")]
        public void GivenЗаданRTUССерийникомСАдресом_СДлинойВолныSMИСПортами(string p0, string p1, string p2, int p3)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Yes));
            _sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 });
            _sut.Poller.EventSourcingTick().Wait();
            _rtu = _sut.ReadModel.Rtus.Last();
            _oldOwnPortCount = p3;
            _sut.FakeD2RWcfManager.SetFakeInitializationAnswer(serial:p0, waveLength: p2, ownPortCount: _oldOwnPortCount);
            _rtuAddress = p1;
            _rtuLeaf = _sut.SetNameAndAskInitializationRtu(_rtu.Id, _rtuAddress);
        }

        [Given(@"БОП подключен к RTU к порту  (.*)")]
        public void GivenБопПодключенКrtuкПорту(int p0)
        {
            _portWithBop = p0;
            _otauLeaf = _sut.AttachOtau(_rtuLeaf, _portWithBop);
        }

        [Given(@"Трасса (.*) подключена к порту RTU (.*)")]
        public void GivenТрассаTracePПодключенаКПортуRtu(string p0, int p1)
        {
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation { Type = EquipmentType.Terminal }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            var secondNodeId = _sut.ReadModel.Nodes.Last().NodeId;
            _sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = _rtu.NodeId, NodeId2 = secondNodeId }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            var trace = _sut.DefineTrace(secondNodeId, _rtu.NodeId, p0);
            _sut.Poller.EventSourcingTick().Wait();
            _sut.AttachTraceTo(trace.TraceId, _rtuLeaf, p1, Answer.Yes);
            _sut.Poller.EventSourcingTick().Wait();
            if (p1 == 11)
                _trace11 = trace;
        }

        [Given(@"Трасса подключена к порту БОПа (.*)")]
        public void GivenТрассаПодключенаКПортуБоПа(int p0)
        {
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation { Type = EquipmentType.Terminal }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            var secondNodeId = _sut.ReadModel.Nodes.Last().NodeId;
            _sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = _rtu.NodeId, NodeId2 = secondNodeId }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            var trace = _sut.DefineTrace(secondNodeId, _rtu.NodeId, @"Trace on BOP");
            _sut.Poller.EventSourcingTick().Wait();
            _sut.AttachTraceTo(trace.TraceId, _otauLeaf, p0, Answer.Yes);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Заменяем RTU на свежий с серийником (.*) и с (.*) портами")]
        public void WhenЗаменяемRtuНаСвежийССерийникомИсПортами(string p0, int p1)
        {
            _newSerial = p0;
            _newOwnPortCount = p1;
            if (_portWithBop > _newOwnPortCount)
                _sut.FakeD2RWcfManager.SetFakeInitializationAnswer(ReturnCode.RtuTooBigPortNumber);
            else
                _sut.FakeD2RWcfManager.SetFakeInitializationAnswer(ReturnCode.Ok, serial:_newSerial, ownPortCount: _newOwnPortCount);
        }

        [When(@"Пользователь нажимает переинициализировать RTU")]
        public void WhenПользовательНажимаетПереинициализироватьRtu()
        {
            _sut.FakeWindowManager.RegisterHandler(m => m is MyMessageBoxViewModel);
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuInitializeHandler(model, _rtuAddress, "", Answer.Yes));
            _rtuLeaf.MyContextMenu.First(i => i?.Header == Resources.SID_Network_settings).Command.Execute(_rtuLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Выдается сообщение что RTU инициализирован успешно")]
        public void ThenВыдаетсяСообщениеЧтоRtuИнициализированУспешно()
        {
            _sut.FakeWindowManager.Log
                .OfType<MyMessageBoxViewModel>()
                .Last()
                .Lines[0].Line
                .Should().Be(Resources.SID_RTU_initialized_successfully_);
        }

        [Then(@"Trace11 отсоединена и окрашена синим")]
        public void ThenTrace11ОтсоединенаИОкрашенаСиним()
        {
            _trace11.State.Should().Be(FiberState.NotJoined);
            var traceLeaf = (TraceLeaf)_sut.TreeOfRtuModel.GetById(_trace11.TraceId);
            traceLeaf.Color.Should().Be(Brushes.Blue);
            _trace11.OtauPort.Should().BeNull();
            traceLeaf.PortNumber.Should().Be(0);
            foreach (var fiber in _sut.ReadModel.GetTraceFibers(_trace11))
            {
                _sut.ReadModel.Fibers.First(f => f.FiberId == fiber.FiberId).States[_trace11.TraceId].Should()
                    .Be(FiberState.NotJoined);
                _sut.GraphReadModel.Data.Fibers.First(f => f.Id == fiber.FiberId).States[_trace11.TraceId].Should()
                    .Be(FiberState.NotJoined);
            }
        }

        [Then(@"Серийный номер и количество портов RTU изменяются на новые")]
        public void ThenСерийныйНомерИКоличествоПортовRtuИзменяютсяНаНовые()
        {
            _rtu.Serial.Should().Be(_newSerial);
            _rtu.OwnPortCount.Should().Be(_newOwnPortCount);
            _rtuLeaf.Serial.Should().Be(_newSerial);
            _rtuLeaf.OwnPortCount.Should().Be(_newOwnPortCount);
        }

        [Then(@"В дереве у RTU изменяется количество веток-портов")]
        public void ThenВДеревеУRTUИзменяетсяКоличествоВеток_Портов()
        {
            _rtuLeaf.ChildrenImpresario.Children.Count.Should().Be(9);
        }

    }
}
using System.Linq;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
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

        [Given(@"Задан RTU с адресом (.*) с длиной волны (.*) и с (.*) портами")]
        public void GivenЗаданRTUСАдресом_СДлинойВолныSMИСПортами(string p0, string p1, int p2)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Yes));
            _sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 });
            _sut.Poller.EventSourcingTick().Wait();
            _rtu = _sut.ReadModel.Rtus.Last();
            _sut.FakeD2RWcfManager.SetFakeInitializationAnswer(waveLength: p1, ownPortCount: p2);
            _rtuAddress = p0;
            _rtuLeaf = _sut.SetNameAndAskInitializationRtu(_rtu.Id, _rtuAddress);
        }

        [Given(@"БОП подключен к RTU к порту  (.*)")]
        public void GivenБопПодключенКrtuкПорту(int p0)
        {
            _portWithBop = p0;
            _otauLeaf = _sut.AttachOtau(_rtuLeaf, _portWithBop);
        }


        [Given(@"Трасса подключена к порту RTU (.*)")]
        public void GivenТрассаПодключенаКПортуRtu(int p0)
        {
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation { Type = EquipmentType.Terminal }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            var secondNodeId = _sut.ReadModel.Nodes.Last().NodeId;
            _sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = _rtu.NodeId, NodeId2 = secondNodeId }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            var trace = _sut.DefineTrace(secondNodeId, _rtu.NodeId, @"Trace on RTU");
            _sut.Poller.EventSourcingTick().Wait();
            _sut.AttachTraceTo(trace.TraceId, _rtuLeaf, p0, Answer.Yes);
            _sut.Poller.EventSourcingTick().Wait();
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
    }
}
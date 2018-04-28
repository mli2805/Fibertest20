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

        [Given(@"Существует RTU с адресом (.*) с длиной волны (.*) и с (.*) портами")]
        public void GivenСуществуетRtuсАдресомСДлинойВолныИсПортами(string p0, string p1, int p2)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Yes));
            _sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 });
            _sut.Poller.EventSourcingTick().Wait();
            _rtu = _sut.ReadModel.Rtus.Last();
            _sut.FakeD2RWcfManager.SetFakeInitializationAnswer(waveLength:p1, ownPortCount:p2);
            _rtuAddress = p0;
            _rtuLeaf = _sut.InitializeRtu(_rtu.Id, _rtuAddress);
        }

        [Given(@"БОП подключен к порту RTU (.*)")]
        public void GivenБопПодключенКПортуRtu(int p0)
        {
            _otauLeaf = _sut.AttachOtau(_rtuLeaf, p0);
        }

        [Given(@"Трасса подключена к порту RTU (.*)")]
        public void GivenТрассаПодключенаКПортуRtu(int p0)
        {
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation{ Type = EquipmentType.Terminal}).Wait();
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

        [When(@"RTU заменен на старинный не поддерживающий БОПы RTU")]
        public void WhenRtuЗамененНаСтаринныйНеПоддерживающийБопыRtu()
        {
            _sut.FakeD2RWcfManager.SetFakeInitializationAnswer(ReturnCode.RtuDoesntSupportBop);
        }

        [When(@"Пользователь жмет переинициализировать RTU")]
        public void WhenПользовательЖметПереинициализироватьRtu()
        {
            _rtuLeaf = _sut.InitializeRtu(_rtu.Id, _rtuAddress);
        }

        [Then(@"Выдается сообщение с требование вручную отсоединить БОП и повторить")]
        public void ThenВыдаетсяСообщениеСТребованиеВручнуюОтсоединитьБопиПовторить()
        {
        }

        [Then(@"В дереве ничего не меняется")]
        public void ThenВДеревеНичегоНеМеняется()
        {
        }

    }
}
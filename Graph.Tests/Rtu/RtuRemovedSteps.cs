using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuRemovedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Iit.Fibertest.Graph.Rtu _rtu;
        private Guid _rtuNodeId, _traceId; 

        private Iit.Fibertest.Graph.Node _endTraceNode;


        [Given(@"Существует РТУ")]
        public void GivenСуществуетРту()
        {
            _sut.ShellVm.ComplyWithRequest(new AddRtuAtGpsLocation()).Wait();
            _sut.Poller.Tick();
            _rtu = _sut.ReadModel.Rtus.Last();
            _rtuNodeId = _rtu.NodeId;
        }

        [Given(@"Существует несколько отрезков от РТУ")]
        public void GivenСуществуетНесколькоОтрезковОтРту()
        {
            _sut.ShellVm.ComplyWithRequest(new AddNode()).Wait();
            _sut.Poller.Tick();
            var n1 = _sut.ReadModel.Nodes.Last();
            _sut.ShellVm.ComplyWithRequest(new AddNode()).Wait();
            _sut.Poller.Tick();
            var n2 = _sut.ReadModel.Nodes.Last();
            _sut.ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() {Type= EquipmentType.Terminal}).Wait();
            _sut.Poller.Tick();
            _endTraceNode = _sut.ReadModel.Nodes.Last();
            _sut.ShellVm.ComplyWithRequest(new AddFiber() {Node1 = _rtu.NodeId , Node2 = n1.Id }).Wait();
            _sut.ShellVm.ComplyWithRequest(new AddFiber() {Node1 = n1.Id, Node2 = _endTraceNode.Id }).Wait();
            _sut.ShellVm.ComplyWithRequest(new AddFiber() {Node1 = _rtu.NodeId , Node2 = n2.Id }).Wait();
            _sut.Poller.Tick();
        }

        [Given(@"Существует трасса начинающаяся но не присоединенная к этому РТУ")]
        public void GivenСуществуетТрассаНачинающаясяНоНеПрисоединеннаяКЭтомуРту()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.QuestionAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.AddTraceViewHandler(model, @"some trace title", "", Answer.Yes));

            _sut.ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = _endTraceNode.Id, NodeWithRtuId = _rtu.NodeId }).Wait();
            _sut.Poller.Tick();
            _traceId = _sut.ReadModel.Traces.Last().Id;
        }

        [When(@"Пользователь кликает на РТУ удалить")]
        public void WhenПользовательКликаетНаРтуУдалить()
        {
            _sut.ShellVm.ComplyWithRequest(new RequestRemoveRtu() {NodeId = _rtu.NodeId}).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"РТУ удаляется")]
        public void ThenРтуУдаляется()
        {
            _sut.ReadModel.Rtus.FirstOrDefault(r => r.NodeId == _rtuNodeId).Should().BeNull();
        }

        [Then(@"Узел под РТУ и присоединенные к нему отрезки удаляются")]
        public void ThenУзелПодРтуиПрисоединенныеКНемуОтрезкиУдаляются()
        {
            _sut.ReadModel.Nodes.FirstOrDefault(n => n.Id == _rtuNodeId).Should().Be(null);
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.Node1 == _rtuNodeId || f.Node2 == _rtuNodeId).Should().BeNull();
        }
    }
}
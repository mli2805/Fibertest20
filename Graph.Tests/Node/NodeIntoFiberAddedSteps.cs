using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class NodeIntoFiberAddedSteps
    {
        private readonly SystemUnderTest _sut;
        private Guid _nodeForRtuId;
        private Guid _firstNodeId;
        private Guid _nodeId;
        private Guid _fiberId;

        public NodeIntoFiberAddedSteps(SystemUnderTest sut)
        {
            _sut = sut;
        }

        [Given(@"Есть трасса")]
        public void GivenЕстьТрасса()
        {
            var equipments = new List<Guid>();
            _sut.Map.AddRtuAtGpsLocation();
            _sut.Poller.Tick();
            _nodeForRtuId = _sut.ReadModel.Nodes.Single().Id;
            equipments.Add(_sut.ReadModel.Rtus.Single().Id);
            _sut.Map.AddNode();
            _sut.Map.AddEquipmentAtGpsLocation(EquipmentType.Terminal);
            _sut.Poller.Tick();
            _firstNodeId = _sut.ReadModel.Nodes[1].Id;
            var secondNodeId = _sut.ReadModel.Nodes.Last().Id;
            equipments.Add(Guid.Empty);
            equipments.Add(_sut.ReadModel.Equipments.Last().Id);
            _sut.Map.AddFiber(_nodeForRtuId, _firstNodeId);
            _sut.Map.AddFiber(_firstNodeId, secondNodeId);
            _sut.Poller.Tick();
            _fiberId = _sut.ReadModel.Fibers.First().Id;
            var addTraceViewModel = new AddTraceViewModel(_sut.ReadModel, _sut.Aggregate, new List<Guid>(){ _nodeForRtuId, _firstNodeId, secondNodeId }, equipments);
            addTraceViewModel.Save();
            _sut.Poller.Tick();
           
        }

        [When(@"Пользователь кликает добавить узел в отрезок этой трассы")]
        public void WhenПользовательКликаетДобавитьУзелВОтрезок()
        {
            _sut.Map.AddNodeIntoFiber(_fiberId);
            _sut.Poller.Tick();
            _nodeId = _sut.ReadModel.Nodes.Last().Id;
        }

        [Then(@"Старый отрезок удаляется и добавляются два новых и новый узел связывает их")]
        public void ThenВместоОтрезкаОбразуетсяДваНовыхИНовыйУзелСвязывающийИх()
        {
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.Id == _fiberId).Should().Be(null);
            _sut.ReadModel.Fibers.FirstOrDefault(f => new NodePairKey(f.Node1, f.Node2).
                        Equals(new NodePairKey(_firstNodeId,_nodeId))).Should().NotBe(null);
            _sut.ReadModel.Fibers.FirstOrDefault(f => new NodePairKey(f.Node1, f.Node2).
                        Equals(new NodePairKey(_nodeForRtuId, _nodeId))).Should().NotBe(null);
        }

        [Then(@"Новый узел входит в трассу")]
        public void ThenНовыйУзелВходитВТрассуАСвязностьТрассыСохраняется()
        {
            var trace = _sut.ReadModel.Traces.First();
            trace.Nodes.Should().Contain(_nodeId);
        }

        [Then(@"Отказ с сообщением")]
        public void ThenОтказССообщением()
        {
        }
    }
}

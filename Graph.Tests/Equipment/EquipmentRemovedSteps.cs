using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class EquipmentRemovedSteps
    {
        private readonly SystemUnderTest2 _sut = new SystemUnderTest2();
        private Guid _nodeId;
        private Guid _equipmentId;

        [Given(@"Существует узел с оборудованием")]
        public void GivenСуществуетУзелСОборудованием()
        {
            _sut.ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() {Type = EquipmentType.Sleeve}).Wait();
            _sut.Poller.Tick();
            _nodeId = _sut.ReadModel.Nodes.Single().Id;
            _equipmentId = _sut.ReadModel.Equipments.Single().Id;
        }

        [Given(@"Существует трасса использующая данное оборудование")]
        public void GivenСуществуетТрассаИспользующаяДанноеОборудование()
        {
            _sut.ShellVm.ComplyWithRequest(new AddRtuAtGpsLocation()).Wait();
            _sut.Poller.Tick();
            Guid rtuNodeId = _sut.ReadModel.Nodes.Last().Id;

            _sut.ShellVm.ComplyWithRequest(new AddFiber() {Node1 = rtuNodeId, Node2 = _nodeId}).Wait();
            _sut.Poller.Tick();
            
            _sut.FakeWindowManager.RegisterHandler(model => _sut.QuestionAnswer("Accept the path?", Answer.Yes, model));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(EquipmentChoiceAnswer.Use, model));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.AddTraceViewHandler(model, "some title", "", Answer.Yes));

            _sut.ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = _nodeId, NodeWithRtuId = rtuNodeId }).Wait();
            _sut.Poller.Tick();

        }

        [Given(@"Для этой трассы задана базовая")]
        public void GivenДляЭтойТрассыЗаданаБазовая()
        {
            _sut.ReadModel.Traces.Single().PreciseId = Guid.NewGuid();
        }

        [When(@"Пользователь нажимает удалить оборудование")]
        public void WhenПользовательНажимаетУдалитьОборудование()
        {
           // new NodeUpdateViewModel(_nodeId, _sut.ShellVm.GraphVm, new FakeWindowManager()).RemoveEquipment(_equipmentId);
            _sut.Poller.Tick();
        }

        [Then(@"Оборудование удаляется из трассы")]
        public void ThenОборудованиеУдаляетсяИзТрассы()
        {
            _sut.ReadModel.Traces.Where(t => t.Equipments.Contains(_equipmentId)).Should().BeEmpty();

        }

        [Then(@"Оборудование удаляется")]
        public void ThenОборудованиеУдаляется()
        {
            _sut.ReadModel.Equipments.FirstOrDefault(e=>e.Id == _equipmentId).Should().BeNull();
        }

        [Then(@"Оборудование НЕ удаляется")]
        public void ThenОборудованиеНеУдаляется()
        {
            _sut.ReadModel.Traces.Where(t => t.Equipments.Contains(_equipmentId)).Should().NotBeEmpty();
            _sut.ReadModel.Equipments.FirstOrDefault(e => e.Id == _equipmentId).Should().NotBeNull();
        }
    }
}

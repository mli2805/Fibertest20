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
    public sealed class EquipmentRemovedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private readonly MapViewModel _vm;
        private Guid _nodeId;
        private Guid _equipmentId;

        private UpdateNodeViewModel _updateNodeViewModel;

        public EquipmentRemovedSteps()
        {
            _vm = new MapViewModel(_sut.Aggregate, _sut.ReadModel);
        }

        [Given(@"Существует узел с оборудованием")]
        public void GivenСуществуетУзелСОборудованием()
        {
            _vm.AddEquipmentAtGpsLocation(EquipmentType.Sleeve);
            _sut.Poller.Tick();
            _nodeId = _sut.ReadModel.Nodes.Single().Id;
            _equipmentId = _sut.ReadModel.Equipments.Single().Id;
        }

        [Given(@"Существует трасса использующая данное оборудование")]
        public void GivenСуществуетТрассаИспользующаяДанноеОборудование()
        {
            _vm.AddRtuAtGpsLocation();
            _sut.Poller.Tick();
            Guid rtuNodeId = _sut.ReadModel.Nodes.Last().Id;

            _vm.AddFiber(rtuNodeId, _nodeId);
            _sut.Poller.Tick();

            var path = new PathFinder(_sut.ReadModel).FindPath(rtuNodeId, _nodeId);
            var traceNodes = path.ToList();
            var equipments = new List<Guid>() {_sut.ReadModel.Rtus.Single().Id, _equipmentId};
            new AddTraceViewModel(_sut.ReadModel, _sut.Aggregate, traceNodes, equipments).Save();
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
            new UpdateNodeViewModel(_nodeId, _sut.ReadModel, _sut.Aggregate).RemoveEquipment(_equipmentId);
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
        public void ThenОборудованиеНЕУдаляется()
        {
            _sut.ReadModel.Traces.Where(t => t.Equipments.Contains(_equipmentId)).Should().NotBeEmpty();
            _sut.ReadModel.Equipments.FirstOrDefault(e => e.Id == _equipmentId).Should().NotBeNull();
        }
    }
}

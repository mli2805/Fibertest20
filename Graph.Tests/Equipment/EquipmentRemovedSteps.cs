using System;
using System.Linq;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class EquipmentRemovedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _nodeAId, _equipmentA1Id;
        private Guid _nodeBId, _equipmentB1Id;
        private Iit.Fibertest.Graph.Trace _trace;
        private NodeUpdateViewModel _vm;


        [Given(@"Существует трасса c оборудованием А1 в середине и B1 в конце")]
        public void GivenСуществуетТрассаCОборудованиемАвСерединеИbвКонце()
        {
            _trace = _sut.SetTraceFromRtuThrouhgAtoB(out _nodeAId, out _equipmentA1Id, out _nodeBId, out _equipmentB1Id);
        }

      
        [Given(@"Для этой трассы задана базовая")]
        public void GivenДляЭтойТрассыЗаданаБазовая()
        {
            var traceLeaf = (TraceLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(_trace.TraceId);
            var rtuId = traceLeaf.Parent.Id;
            _sut.InitializeRtu(rtuId);

            _sut.AssignBaseRef(traceLeaf, SystemUnderTest.Base1625Lm3, SystemUnderTest.Base1625Lm3, null, Answer.Yes);
            traceLeaf.BaseRefsSet.PreciseId.Should().NotBe(Guid.Empty);
        }

        [Given(@"Открыта форма для редактирования узла где оборудование А1")]
        public void GivenОткрытаФормаДляРедактированияУзлаГдеОборудованиеА1()
        {
            _vm = _sut.ClientScope.Resolve<NodeUpdateViewModel>();
            _vm.Initialize(_nodeAId);
        }


        [Given(@"Открыта форма для редактирования узла где оборудование B1")]
        public void GivenОткрытаФормаДляРедактированияУзлаГдеОборудованиеB1()
        {
            _vm = _sut.ClientScope.Resolve<NodeUpdateViewModel>();
            _vm.Initialize(_nodeBId);
        }

        [Then(@"Пункт Удалить доступен для данного оборудования")]
        public void ThenПунктУдалитьДоступенДляДанногоОборудования()
        {
            _vm.EquipmentsInNode.First(e => e.Id == _equipmentA1Id).IsRemoveEnabled.Should().BeTrue();
        }

        [Then(@"Пункт Удалить недоступен для оборудования A1")]
        public void ThenПунктУдалитьНедоступенДляОборудованияA1()
        {
            _vm.EquipmentsInNode.First(e => e.Id == _equipmentA1Id).IsRemoveEnabled.Should().BeFalse();
        }

        [Then(@"Пункт Удалить недоступен для оборудования B1")]
        public void ThenПунктУдалитьНедоступенДляОборудованияB1()
        {
            _vm.EquipmentsInNode.First(e => e.Id == _equipmentB1Id).IsRemoveEnabled.Should().BeFalse();
        }

        [When(@"Пользователь нажимает удалить оборудование")]
        public void WhenПользовательНажимаетУдалитьОборудование()
        {
            var vm = _sut.ClientScope.Resolve<NodeUpdateViewModel>();
            vm.Initialize(_nodeAId);
            vm.EquipmentsInNode.First(it=>it.Id == _equipmentA1Id).Command = new RemoveEquipment() { EquipmentId = _equipmentA1Id};
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Оборудование удаляется из трассы")]
        public void ThenОборудованиеУдаляетсяИзТрассы()
        {
            _sut.ReadModel.Traces.Where(t => t.EquipmentIds.Contains(_equipmentA1Id)).Should().BeEmpty();
            _trace.EquipmentIds.Contains(_equipmentA1Id).ShouldBeEquivalentTo(false);
            _sut.ReadModel.Equipments.Count(e => e.NodeId == _nodeAId).ShouldBeEquivalentTo(1);
            _trace.EquipmentIds.Count.ShouldBeEquivalentTo(_trace.NodeIds.Count);
            _sut.ReadModel.Equipments.First(e => e.NodeId == _nodeAId).EquipmentId.ShouldBeEquivalentTo(_trace.EquipmentIds[1]);
            _trace.EquipmentIds.Contains(Guid.Empty).Should().BeFalse();
        }

        [Then(@"Оборудование удаляется")]
        public void ThenОборудованиеУдаляется()
        {
            _sut.ReadModel.Equipments.FirstOrDefault(e=>e.EquipmentId == _equipmentA1Id).Should().BeNull();
        }

    }
}

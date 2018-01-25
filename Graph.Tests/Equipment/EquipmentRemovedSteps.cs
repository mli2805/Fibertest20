using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class EquipmentRemovedSteps
    {
        private readonly SutForEquipmentUpdateRemove _sut = new SutForEquipmentUpdateRemove();
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
            var traceLeaf = (TraceLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_trace.Id);
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BaseRefAssignHandler(model, _trace.Id, SystemUnderTest.Base1625, SystemUnderTest.Base1625, null, Answer.Yes));
            _sut.TraceLeafActions.AssignBaseRefs(traceLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Given(@"Открыта форма для редактирования узла где оборудование А1")]
        public void GivenОткрытаФормаДляРедактированияУзлаГдеОборудованиеА1()
        {
            _vm = new NodeUpdateViewModel(_nodeAId, _sut.ShellVm.ReadModel, _sut.FakeWindowManager, _sut.ShellVm.C2DWcfManager);
        }
        [Given(@"Открыта форма для редактирования узла где оборудование B1")]
        public void GivenОткрытаФормаДляРедактированияУзлаГдеОборудованиеB1()
        {
            _vm = new NodeUpdateViewModel(_nodeBId, _sut.ShellVm.ReadModel, _sut.FakeWindowManager, _sut.ShellVm.C2DWcfManager);
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
            var vm = new NodeUpdateViewModel(_nodeAId, _sut.ShellVm.ReadModel, _sut.FakeWindowManager, _sut.ShellVm.C2DWcfManager);
            vm.EquipmentsInNode.First(it=>it.Id == _equipmentA1Id).Command = new RemoveEquipment() { Id = _equipmentA1Id};
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Оборудование удаляется из трассы")]
        public void ThenОборудованиеУдаляетсяИзТрассы()
        {
            _sut.ReadModel.Traces.Where(t => t.Equipments.Contains(_equipmentA1Id)).Should().BeEmpty();
            _trace.Equipments.Contains(_equipmentA1Id).ShouldBeEquivalentTo(false);
            _sut.ReadModel.Equipments.Count(e => e.NodeId == _nodeAId).ShouldBeEquivalentTo(1);
            _trace.Equipments.Count.ShouldBeEquivalentTo(_trace.Nodes.Count);
            _sut.ReadModel.Equipments.First(e => e.NodeId == _nodeAId).Id.ShouldBeEquivalentTo(_trace.Equipments[1]);
            _trace.Equipments.Contains(Guid.Empty).Should().BeFalse();
        }

        [Then(@"Оборудование удаляется")]
        public void ThenОборудованиеУдаляется()
        {
            _sut.ReadModel.Equipments.FirstOrDefault(e=>e.Id == _equipmentA1Id).Should().BeNull();
        }

    }
}

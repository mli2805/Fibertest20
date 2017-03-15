﻿using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class EquipmentRemovedSteps
    {
        private readonly SutForEquipmentOperations _sut = new SutForEquipmentOperations();
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
            var vm = new BaseRefsAssignViewModel(_trace, _sut.ReadModel, _sut.ShellVm.Bus);
            vm.PreciseBaseFilename = SystemUnderTest.Path;
            vm.Save();
            _sut.Poller.Tick();
        }

        [Given(@"Открыта форма для редактирования узла где оборудование А1")]
        public void GivenОткрытаФормаДляРедактированияУзлаГдеОборудованиеА1()
        {
            _vm = new NodeUpdateViewModel(_nodeAId, _sut.ShellVm.ReadModel, _sut.FakeWindowManager, _sut.ShellVm.Bus);
        }
        [Given(@"Открыта форма для редактирования узла где оборудование B1")]
        public void GivenОткрытаФормаДляРедактированияУзлаГдеОборудованиеB1()
        {
            _vm = new NodeUpdateViewModel(_nodeBId, _sut.ShellVm.ReadModel, _sut.FakeWindowManager, _sut.ShellVm.Bus);
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
            var vm = new NodeUpdateViewModel(_nodeAId, _sut.ShellVm.ReadModel, _sut.FakeWindowManager, _sut.ShellVm.Bus);
            vm.RemoveEquipment(new RemoveEquipment() {Id = _equipmentA1Id});
            vm.Cancel();
            _sut.Poller.Tick();
        }

        [Then(@"Оборудование удаляется из трассы")]
        public void ThenОборудованиеУдаляетсяИзТрассы()
        {
            _sut.ReadModel.Traces.Where(t => t.Equipments.Contains(_equipmentA1Id)).Should().BeEmpty();

        }

        [Then(@"Оборудование удаляется")]
        public void ThenОборудованиеУдаляется()
        {
            _sut.ReadModel.Equipments.FirstOrDefault(e=>e.Id == _equipmentA1Id).Should().BeNull();
        }

    }
}

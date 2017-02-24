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
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _nodeAId, _equipmentA1Id;
        private Guid _nodeBId;
        private Guid _rtuNodeId, _anotherNodeId;
        private Guid _traceId;


        [Given(@"Существует узел A с оборудованием A1")]
        public void GivenСуществуетУзелСОборудованием()
        {
            _sut.ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() {Type = EquipmentType.Sleeve}).Wait();
            _sut.Poller.Tick();
            _nodeAId = _sut.ReadModel.Nodes.Last().Id;
            _equipmentA1Id = _sut.ReadModel.Equipments.Last().Id;
        }

        [Given(@"Существует узел B с оборудованием B1")]
        public void GivenСуществуетУзелBсОборудованиемB1()
        {
            _sut.ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() { Type = EquipmentType.Sleeve }).Wait();
            _sut.Poller.Tick();
            _nodeBId = _sut.ReadModel.Nodes.Last().Id;
        }

        [Given(@"Существует RTU и еще один узел")]
        public void GivenСуществуетRtuиЕщеПаруУзлов()
        {
            _sut.ShellVm.ComplyWithRequest(new AddRtuAtGpsLocation()).Wait();
            _sut.Poller.Tick();
            _rtuNodeId = _sut.ReadModel.Nodes.Last().Id;

            _sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = _rtuNodeId, Node2 = _nodeBId }).Wait();
            _sut.Poller.Tick();

            _sut.ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            _sut.Poller.Tick();
            _anotherNodeId = _sut.ReadModel.Nodes.Last().Id;

            _sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = _anotherNodeId, Node2 = _nodeBId }).Wait();
            _sut.Poller.Tick();
        }

        [Given(@"Существует трасса c данным оборудованием в середине")]
        public void GivenСуществуетТрассаCДаннымОборудованиемВСередине()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.QuestionAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.AddTraceViewHandler(model, @"some title", "", Answer.Yes));

            _sut.ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = _anotherNodeId, NodeWithRtuId = _rtuNodeId }).Wait();
            _sut.Poller.Tick();
            _traceId = _sut.ReadModel.Traces.Last().Id;
        }

        [Given(@"Для этой трассы задана базовая")]
        public void GivenДляЭтойТрассыЗаданаБазовая()
        {
            _sut.FakeWindowManager.BaseIsSet();
            _sut.ShellVm.ComplyWithRequest(new RequestAssignBaseRef() { TraceId = _traceId }).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"Пункт Удалить недоступен для данного оборудования")]
        public void ThenПунктУдалитьНедоступенДляДанногоОборудования()
        {
            var vm = new NodeUpdateViewModel(_nodeAId, _sut.ShellVm.ReadModel, _sut.FakeWindowManager, _sut.ShellVm.Bus);
            vm.EquipmentsInNode.First(e => e.Id == _equipmentA1Id).IsRemoveEnabled.Should().BeFalse();
        }

        [When(@"Пользователь нажимает удалить оборудование")]
        public void WhenПользовательНажимаетУдалитьОборудование()
        {
            // не  работает , т.к. некому реагировать на RemoveEquipment
            // надо начинать с ShellVM
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

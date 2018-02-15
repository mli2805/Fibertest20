using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class FiberWithNodesSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _leftNodeId;
        private Guid _rightNodeId;
        private int _equipmentCountCutOff;
        private int _nodesCountCutOff;
        private int _fibersCountCutOff;
        private int _sleeveCountcutOff;
        

        [Given(@"Левый и правый узлы уже созданы")]
        public void GivenЛевыйИПравыйУзлыУжеСозданы()
        {
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation()).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _leftNodeId =_sut.ReadModel.Nodes.Last().Id;
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation()).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _rightNodeId =_sut.ReadModel.Nodes.Last().Id;
            _nodesCountCutOff =_sut.ReadModel.Nodes.Count;
            _equipmentCountCutOff =_sut.ReadModel.Equipments.Count;
            _fibersCountCutOff =_sut.ReadModel.Fibers.Count;
            _sleeveCountcutOff =_sut.ReadModel.Equipments.Count(e => e.Type == EquipmentType.Closure);
        }

        [Given(@"Между левым и правым узлом уже добавлен отрезок")]
        public void GivenМеждуЛевымИПравымУзломУжеДобавленОтрезок()
        {
            _sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() {Node1 = _leftNodeId, Node2 = _rightNodeId}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _fibersCountCutOff =_sut.ReadModel.Fibers.Count;
        }

        [When(@"Пользователь кликает добавить отрезок с узлами")]
        public void WhenПользовательКликаетДобавитьОтрезокСУзлами()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            _sut.GraphReadModel.GrmFiberWithNodesRequest.AddFiberWithNodes(new RequestAddFiberWithNodes() {Node1 = _leftNodeId, Node2 = _rightNodeId}).Wait();
        }

        [When(@"Пользователь кликает добавить отрезок с нулем узлов")]
        public void WhenПользовательКликаетДобавитьОтрезокСнулемУзлов()
        {
            var bluh = EquipmentType.Cross;
            _sut.FakeWindowManager.RegisterHandler(model => _sut.FiberWithNodesAdditionHandler(model, 0, bluh, Answer.Yes));
            _sut.GraphReadModel.GrmFiberWithNodesRequest.AddFiberWithNodes(new RequestAddFiberWithNodes() {Node1 = _leftNodeId, Node2 = _rightNodeId}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Пользователь кликает добавить отрезок с (.*) точками привязки")]
        public void WhenПользовательКликаетДобавитьОтрезокСТочкамиПривязки(int p0)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.FiberWithNodesAdditionHandler(model, p0, EquipmentType.AdjustmentPoint, Answer.Yes));
            _sut.GraphReadModel.GrmFiberWithNodesRequest.AddFiberWithNodes(new RequestAddFiberWithNodes() { Node1 = _leftNodeId, Node2 = _rightNodeId }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Пользователь кликает добавить отрезок с (.*) пустыми узлами")]
        public void WhenПользовательКликаетДобавитьОтрезокСПустымиУзлами(int p0)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.FiberWithNodesAdditionHandler(model, p0, EquipmentType.EmptyNode, Answer.Yes));
            _sut.GraphReadModel.GrmFiberWithNodesRequest.AddFiberWithNodes(new RequestAddFiberWithNodes() {Node1 = _leftNodeId, Node2 = _rightNodeId}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Пользователь кликает добавить отрезок с (.*) узлами с муфтами")]
        public void WhenПользовательКликаетДобавитьОтрезокСУзламиСОборудованием(int p0)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.FiberWithNodesAdditionHandler(model, p0, EquipmentType.Closure, Answer.Yes));
            _sut.GraphReadModel.GrmFiberWithNodesRequest.AddFiberWithNodes(new RequestAddFiberWithNodes() {Node1 = _leftNodeId, Node2 = _rightNodeId}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }


        [Then(@"Появляется сАобщение о существовании такого отрезка")]
        public void ThenПоявляетсяСАобщениеОСуществованииТакогоОтрезка()
        {
            _sut.FakeWindowManager.Log
                .OfType<MyMessageBoxViewModel>()
                .Last()
                .Lines[0].Line
                .Should().Be(Resources.SID_Section_already_exists);
        }


        [Then(@"Новый отрезок не сохраняется")]
        public void ThenНовыйОтрезокНеСохраняется()
        {
           _sut.ReadModel.Fibers.Count.Should().Be(_fibersCountCutOff);
        }

        [Then(@"Новый отрезок сохраняется")]
        public void ThenНовыйОтрезокСохраняется()
        {
           _sut.ReadModel.Fibers.Count.Should().Be(_fibersCountCutOff+1);
        }

        [Then(@"Создается (.*) узла и (.*) отрезка")]
        public void ThenСоздаетсяУзлаИОтрезка(int p0, int p1)
        {
           _sut.ReadModel.Nodes.Count.Should().Be(_nodesCountCutOff + p0);
           _sut.ReadModel.Equipments.Count.Should().Be(_equipmentCountCutOff + p0);
           _sut.ReadModel.Equipments.FirstOrDefault(e => e.Id == Guid.Empty).Should().BeNull();
           _sut.ReadModel.Fibers.Count.Should().Be(_fibersCountCutOff + p1);
        }

        [Then(@"Создается (.*) узла столько же оборудования и (.*) отрезка")]
        public void ThenСоздаетсяУзлаСтолькоЖеОборудованияИОтрезка(int p0, int p1)
        {
           _sut.ReadModel.Nodes.Count.Should().Be(_nodesCountCutOff + p0);
           _sut.ReadModel.Equipments.Count(e => e.Type == EquipmentType.Closure).Should().Be(_sleeveCountcutOff + p0);
           _sut.ReadModel.Fibers.Count.Should().Be(_fibersCountCutOff + p1);
        }
    }
}

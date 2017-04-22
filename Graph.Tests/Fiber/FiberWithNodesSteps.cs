using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;
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
        private int _nodesCountCutOff;
        private int _fibersCountCutOff;
        private int _sleeveCountcutOff;
        

        [Given(@"Левый и правый узлы уже созданы")]
        public void GivenЛевыйИПравыйУзлыУжеСозданы()
        {
            _sut.ShellVm.ComplyWithRequest(new AddNode()).Wait();
            _sut.Poller.Tick();
            _leftNodeId = _sut.ShellVm.ReadModel.Nodes.Last().Id;
            _sut.ShellVm.ComplyWithRequest(new AddNode()).Wait();
            _sut.Poller.Tick();
            _rightNodeId = _sut.ShellVm.ReadModel.Nodes.Last().Id;
            _nodesCountCutOff = _sut.ShellVm.ReadModel.Nodes.Count;
            _fibersCountCutOff = _sut.ShellVm.ReadModel.Fibers.Count;
            _sleeveCountcutOff = _sut.ShellVm.ReadModel.Equipments.Count(e => e.Type == EquipmentType.Closure);
        }

        [Given(@"Между левым и правым узлом уже добавлен отрезок")]
        public void GivenМеждуЛевымИПравымУзломУжеДобавленОтрезок()
        {
            _sut.ShellVm.ComplyWithRequest(new AddFiber() {Node1 = _leftNodeId, Node2 = _rightNodeId}).Wait();
            _sut.Poller.Tick();
            _fibersCountCutOff = _sut.ShellVm.ReadModel.Fibers.Count;
        }

        [When(@"Пользователь кликает добавить отрезок с узлами")]
        public void WhenПользовательКликаетДобавитьОтрезокСУзлами()
        {
            _sut.ShellVm.ComplyWithRequest(new RequestAddFiberWithNodes() {Node1 = _leftNodeId, Node2 = _rightNodeId}).Wait();
        }

        [When(@"Пользователь кликает добавить отрезок с нулем узлов")]
        public void WhenПользовательКликаетДобавитьОтрезокСнулемУзлов()
        {
            var bluh = EquipmentType.Cross;
            _sut.FakeWindowManager.RegisterHandler(model => _sut.FiberWithNodesAdditionHandler(model, 0, bluh, Answer.Yes));
            _sut.ShellVm.ComplyWithRequest(new RequestAddFiberWithNodes() {Node1 = _leftNodeId, Node2 = _rightNodeId}).Wait();
            _sut.Poller.Tick();
        }

        [When(@"Пользователь кликает добавить отрезок с (.*) пустыми узлами")]
        public void WhenПользовательКликаетДобавитьОтрезокСПустымиУзлами(int p0)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.FiberWithNodesAdditionHandler(model, p0, EquipmentType.Well, Answer.Yes));
            _sut.ShellVm.ComplyWithRequest(new RequestAddFiberWithNodes() {Node1 = _leftNodeId, Node2 = _rightNodeId}).Wait();
            _sut.Poller.Tick();
        }

        [When(@"Пользователь кликает добавить отрезок с (.*) узлами с муфтами")]
        public void WhenПользовательКликаетДобавитьОтрезокСУзламиСОборудованием(int p0)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.FiberWithNodesAdditionHandler(model, p0, EquipmentType.Closure, Answer.Yes));
            _sut.ShellVm.ComplyWithRequest(new RequestAddFiberWithNodes() {Node1 = _leftNodeId, Node2 = _rightNodeId}).Wait();
            _sut.Poller.Tick();
        }


        [Then(@"Появляется сАобщение о существовании такого отрезка")]
        public void ThenПоявляетсяСАобщениеОСуществованииТакогоОтрезка()
        {
            _sut.FakeWindowManager.Log
                .OfType<NotificationViewModel>()
                .Last()
                .Message
                .Should().Be(Resources.SID_Section_already_exists);
        }


        [Then(@"Новый отрезок не сохраняется")]
        public void ThenНовыйОтрезокНеСохраняется()
        {
            _sut.ShellVm.ReadModel.Fibers.Count.Should().Be(_fibersCountCutOff);
        }

        [Then(@"Новый отрезок сохраняется")]
        public void ThenНовыйОтрезокСохраняется()
        {
            _sut.ShellVm.ReadModel.Fibers.Count.Should().Be(_fibersCountCutOff+1);
        }

        [Then(@"Создается (.*) узла и (.*) отрезка")]
        public void ThenСоздаетсяУзлаИОтрезка(int p0, int p1)
        {
            _sut.ShellVm.ReadModel.Nodes.Count.Should().Be(_nodesCountCutOff + p0);
            _sut.ShellVm.ReadModel.Fibers.Count.Should().Be(_fibersCountCutOff + p1);
        }

        [Then(@"Создается (.*) узла столько же оборудования и (.*) отрезка")]
        public void ThenСоздаетсяУзлаСтолькоЖеОборудованияИОтрезка(int p0, int p1)
        {
            _sut.ShellVm.ReadModel.Nodes.Count.Should().Be(_nodesCountCutOff + p0);
            _sut.ShellVm.ReadModel.Equipments.Count(e => e.Type == EquipmentType.Closure).Should().Be(_sleeveCountcutOff + p0);
            _sut.ShellVm.ReadModel.Fibers.Count.Should().Be(_fibersCountCutOff + p1);
        }
    }
}

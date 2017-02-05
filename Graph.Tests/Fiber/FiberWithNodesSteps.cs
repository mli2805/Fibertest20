using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;
using NotificationViewModel = Iit.Fibertest.WpfClient.ViewModels.NotificationViewModel;

namespace Graph.Tests
{
    [Binding]
    public sealed class FiberWithNodesSteps
    {
        private readonly SystemUnderTest2 _sut = new SystemUnderTest2();
        private Guid _leftNodeId;
        private Guid _rightNodeId;
        private int _nodesCountCutOff;
        private int _fibersCountCutOff;
        private int _sleeveCountcutOff;
        

        [Given(@"Левый и правый узлы уже созданы")]
        public void GivenЛевыйИПравыйУзлыУжеСозданы()
        {
            _sut.ShellVm.ProcessAsk(new AddNode()).Wait();
            _sut.ShellVm.ProcessAsk(new AddNode()).Wait();
            _sut.Poller.Tick();
            _leftNodeId = _sut.ShellVm.ReadModel.Nodes.First().Id;
            _rightNodeId = _sut.ShellVm.ReadModel.Nodes.Last().Id;
            _nodesCountCutOff = _sut.ShellVm.ReadModel.Nodes.Count;
            _fibersCountCutOff = _sut.ShellVm.ReadModel.Fibers.Count;
            _sleeveCountcutOff = _sut.ShellVm.ReadModel.Equipments.Count(e => e.Type == EquipmentType.Sleeve);
        }

        [Given(@"Между левым и правым узлом уже добавлен отрезок")]
        public void GivenМеждуЛевымИПравымУзломУжеДобавленОтрезок()
        {
            _sut.ShellVm.ProcessAsk(new AddFiber() {Node1 = _leftNodeId, Node2 = _rightNodeId}).Wait();
            _fibersCountCutOff = _sut.ShellVm.ReadModel.Fibers.Count;
        }

        [When(@"Пользователь кликает добавить отрезок с узлами")]
        public void WhenПользовательКликаетДобавитьОтрезокСУзлами()
        {
            _sut.ShellVm.ProcessAsk(new AskAddFiberWithNodes() {Node1 = _leftNodeId, Node2 = _rightNodeId}).Wait();
        }

        [When(@"Пользователь кликает добавить отрезок с нулем узлов")]
        public void WhenПользовательКликаетДобавитьОтрезокСнулемУзлов()
        {
            //const EquipmentType doesntMatter = EquipmentType.Other;
//            _sut.MapVm.AddFiberWithNodes(_leftNodeId, _rightNodeId, 0, doesntMatter);
            _sut.ShellVm.ProcessAsk(new AskAddFiberWithNodes() {Node1 = _leftNodeId, Node2 = _rightNodeId}).Wait();
        }

        [When(@"Пользователь кликает добавить отрезок с (.*) пустыми узлами")]
        public void WhenПользовательКликаетДобавитьОтрезокСПустымиУзлами(int p0)
        {
//            _sut.MapVm.AddFiberWithNodes(_leftNodeId, _rightNodeId, p0, EquipmentType.None);
            _sut.ShellVm.ProcessAsk(new AskAddFiberWithNodes() {Node1 = _leftNodeId, Node2 = _rightNodeId}).Wait();
        }

        [When(@"Пользователь кликает добавить отрезок с (.*) узлами с муфтами")]
        public void WhenПользовательКликаетДобавитьОтрезокСУзламиСОборудованием(int p0)
        {
//            _sut.MapVm.AddFiberWithNodes(_leftNodeId, _rightNodeId, p0, EquipmentType.Sleeve);
            _sut.ShellVm.ProcessAsk(new AskAddFiberWithNodes() {Node1 = _leftNodeId, Node2 = _rightNodeId}).Wait();
        }


        [Then(@"Выдается сообщение (.*)")]
        public void ThenВыдаетсяСообщение(string message)
        {
            _sut.FakeWindowManager.Log
                .OfType<NotificationViewModel>()
                .Last()
                .Message
                .Should().Be(message);
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
            _sut.ShellVm.ReadModel.Equipments.Count(e => e.Type == EquipmentType.Sleeve).Should().Be(_sleeveCountcutOff + p0);
            _sut.ShellVm.ReadModel.Fibers.Count.Should().Be(_fibersCountCutOff + p1);
        }
    }
}

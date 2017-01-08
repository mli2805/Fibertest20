using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class FiberWithNodesSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private readonly MapViewModel _vm;
        private Guid _leftNodeId;
        private Guid _rightNodeId;
        private int _nodesCountCutOff;
        private int _fibersCountCutOff;
        private int _sleeveCountcutOff;

        public FiberWithNodesSteps()
        {
            _vm = new MapViewModel(_sut.Aggregate);
        }

        [Given(@"Левый и правый узлы уже созданы")]
        public void GivenЛевыйИПравыйУзлыУжеСозданы()
        {
            _vm.AddNode();
            _vm.AddNode();
            _sut.Poller.Tick();
            _leftNodeId = _sut.ReadModel.Nodes.First().Id;
            _rightNodeId = _sut.ReadModel.Nodes.Last().Id;
            _sut.Poller.Tick();
            _nodesCountCutOff = _sut.ReadModel.Nodes.Count;
            _fibersCountCutOff = _sut.ReadModel.Fibers.Count;
            _sleeveCountcutOff = _sut.ReadModel.Equipments.Count(e => e.Type == EquipmentType.Sleeve);
        }

        [Given(@"Между левым и правым узлом уже добавлен отрезок")]
        public void GivenМеждуЛевымИПравымУзломУжеДобавленОтрезок()
        {
            _vm.AddFiber(_leftNodeId, _rightNodeId);
            _sut.Poller.Tick();
            _fibersCountCutOff = _sut.ReadModel.Fibers.Count;
        }

        [When(@"Пользователь кликает добавить отрезок с узлами")]
        public void WhenПользовательКликаетДобавитьОтрезокСУзлами()
        {
            const int neverMind = -1;
            const EquipmentType doesntMatter = EquipmentType.Other;
            _vm.AddFiberWithNodes(_leftNodeId, _rightNodeId, neverMind, doesntMatter);
            _sut.Poller.Tick();
        }

        [When(@"Пользователь кликает добавить отрезок с нулем узлов")]
        public void WhenПользовательКликаетДобавитьОтрезокСнулемУзлов()
        {
            const EquipmentType doesntMatter = EquipmentType.Other;
            _vm.AddFiberWithNodes(_leftNodeId, _rightNodeId, 0, doesntMatter);
            _sut.Poller.Tick();
        }

        [When(@"Пользователь кликает добавить отрезок с (.*) пустыми узлами")]
        public void WhenПользовательКликаетДобавитьОтрезокСПустымиУзлами(int p0)
        {
            _vm.AddFiberWithNodes(_leftNodeId, _rightNodeId, p0, EquipmentType.None);
            _sut.Poller.Tick();
        }

        [When(@"Пользователь кликает добавить отрезок с (.*) узлами с муфтами")]
        public void WhenПользовательКликаетДобавитьОтрезокСУзламиСОборудованием(int p0)
        {
            _vm.AddFiberWithNodes(_leftNodeId, _rightNodeId, p0, EquipmentType.Sleeve);
            _sut.Poller.Tick();
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
            _sut.ReadModel.Fibers.Count.Should().Be(_fibersCountCutOff + p1);
        }

        [Then(@"Создается (.*) узла столько же оборудования и (.*) отрезка")]
        public void ThenСоздаетсяУзлаСтолькоЖеОборудованияИОтрезка(int p0, int p1)
        {
            _sut.ReadModel.Nodes.Count.Should().Be(_nodesCountCutOff + p0);
            _sut.ReadModel.Equipments.Count(e => e.Type == EquipmentType.Sleeve).Should().Be(_sleeveCountcutOff + p0);
            _sut.ReadModel.Fibers.Count.Should().Be(_fibersCountCutOff + p1);
        }
    }
}

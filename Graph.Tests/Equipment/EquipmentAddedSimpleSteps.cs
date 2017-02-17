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
    public class EquipmentAddedSimpleSteps
    {
        private readonly SystemUnderTest2 _sut = new SystemUnderTest2();
        private Guid _nodeId;
        private Guid _oldEquipmentId;

        private const EquipmentType EquipmentType = Iit.Fibertest.Graph.EquipmentType.Terminal;
        private const string Title = "some title";
        private const string Comment = "some comment";
        private const int LeftReserve = 5;
        private const int RightReserve = 77;

        [Given(@"Существует пустой узел")]
        public void GivenСуществуетПустойУзел()
        {
            _sut.ShellVm.ComplyWithRequest(new AddNode()).Wait();
            _sut.Poller.Tick();
            _nodeId = _sut.ReadModel.Nodes.Last().Id;
        }

        [Given(@"Существует некоторый узел с оборудованием")]
        public void GivenСуществуетНекоторыйУзелСОборудованием()
        {
            _sut.ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation()).Wait();
            _sut.Poller.Tick();
            _nodeId = _sut.ReadModel.Nodes.Last().Id;
            _oldEquipmentId = _sut.ReadModel.Equipments.Last().Id;
        }

        [Then(@"Пользователь вводит тип и другие параметры оборудования и жмет Сохранить")]
        public void ThenПользовательВводитТипИДругиеПараметрыОборудования()
        {
            _sut.FakeWindowManager.RegisterHandler(model => 
                _sut.EquipmentUpdateHandler(model, _nodeId, EquipmentType, Title, Comment, LeftReserve, RightReserve, Answer.Yes));
        }

        [Then(@"Пользователь вводит тип и другие параметры оборудования но жмет Отмена")]
        public void ThenПользовательВводитТипИДругиеПараметрыОборудованияНоЖметОтмена()
        {
            _sut.FakeWindowManager.RegisterHandler(model =>
                _sut.EquipmentUpdateHandler(model, _nodeId, EquipmentType, Title, Comment, LeftReserve, RightReserve, Answer.Cancel));
        }

        [Then(@"На форме Добавить оборудование")]
        public void WhenПользовательКликаетДобавитьОборудование()
        {
            _sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentIntoNode() { NodeId = _nodeId }).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"Создается оборудование в узле")]
        public void ThenСоздаетсяОборудованиеВУзле()
        {
            var equipment = _sut.ReadModel.Equipments.First(e => e.NodeId == _nodeId);
            equipment.Title.Should().Be(Title);
            equipment.Type.Should().Be(EquipmentType);
            equipment.CableReserveLeft.Should().Be(LeftReserve);
            equipment.CableReserveRight.Should().Be(RightReserve);
            equipment.Comment.Should().Be(Comment);
        }
        [Then(@"Оборудование в узле не создается")]
        public void ThenОборудованиеВУзлеНеСоздается()
        {
            _sut.ReadModel.Equipments.Count.Should().Be(0);
        }

        [Then(@"Создается еще одно оборудование в том же узле")]
        public void ThenСоздаетсяЕщеОдноОборудованиеВТомЖеУзле()
        {
            _sut.ReadModel.Equipments.Count(e => e.NodeId == _nodeId).Should().Be(2);

            var equipment = _sut.ReadModel.Equipments.First(e => e.NodeId == _nodeId && e.Id != _oldEquipmentId);
            equipment.Title.Should().Be(Title);
            equipment.Type.Should().Be(EquipmentType);
            equipment.CableReserveLeft.Should().Be(LeftReserve);
            equipment.CableReserveRight.Should().Be(RightReserve);
            equipment.Comment.Should().Be(Comment);
        }

    }
}

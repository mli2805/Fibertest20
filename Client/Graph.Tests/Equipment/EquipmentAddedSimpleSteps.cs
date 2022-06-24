using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public class EquipmentAddedSimpleSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginOnEmptyBaseAsRoot();
        private Guid _nodeId;
        private Iit.Fibertest.Graph.Equipment _equipment;

        [Given(@"Существует пустой узел")]
        public void GivenСуществуетПустойУзел()
        {
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _nodeId = _sut.ReadModel.Nodes.Last().NodeId;
        }

        [Given(@"Существует некоторый узел с оборудованием")]
        public void GivenСуществуетНекоторыйУзелСОборудованием()
        {
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation()).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _nodeId = _sut.ReadModel.Nodes.Last().NodeId;
        }

        [Then(@"Пользователь вводит тип и другие параметры оборудования и жмет Сохранить")]
        public void ThenПользовательВводитТипИДругиеПараметрыОборудования()
        {
            _sut.FakeWindowManager.RegisterHandler(model =>
                _sut.EquipmentInfoViewModelHandler(model, Answer.Yes));
        }

        [Then(@"Пользователь вводит тип и другие параметры оборудования но жмет Отмена")]
        public void ThenПользовательВводитТипИДругиеПараметрыОборудованияНоЖметОтмена()
        {
            _sut.FakeWindowManager.RegisterHandler(model =>
                _sut.EquipmentInfoViewModelHandler(model, Answer.Cancel));
        }

        [Then(@"На форме Добавить оборудование")]
        public void WhenПользовательКликаетДобавитьОборудование()
        {
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentIntoNode(new RequestAddEquipmentIntoNode() { NodeId = _nodeId }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _equipment = _sut.ReadModel.Equipments.LastOrDefault();
        }

        [Then(@"Создается оборудование в узле")]
        public void ThenСоздаетсяОборудованиеВУзле()
        {
            _equipment.Title.Should().Be(SystemUnderTest.NewTitleForTest);
            _equipment.Type.Should().Be(SystemUnderTest.NewTypeForTest);
            _equipment.CableReserveLeft.Should().Be(SystemUnderTest.NewLeftCableReserve);
            _equipment.CableReserveRight.Should().Be(SystemUnderTest.NewRightCableReserve);
            _equipment.Comment.Should().Be(SystemUnderTest.NewCommentForTest);
            _sut.ReadModel.Equipments.FirstOrDefault(e => e.EquipmentId == Guid.Empty).Should().BeNull();

        }
        [Then(@"Оборудование в узле не создается")]
        public void ThenОборудованиеВУзлеНеСоздается()
        {
            _sut.ReadModel.Equipments.FirstOrDefault(e => e.NodeId == _nodeId && e.Type != EquipmentType.EmptyNode).Should().BeNull();
        }

        [Then(@"Создается еще одно оборудование в том же узле")]
        public void ThenСоздаетсяЕщеОдноОборудованиеВТомЖеУзле()
        {
            _sut.ReadModel.Equipments.Count(e => e.NodeId == _nodeId).Should().Be(3);

            _equipment.Title.Should().Be(SystemUnderTest.NewTitleForTest);
            _equipment.Type.Should().Be(SystemUnderTest.NewTypeForTest);
            _equipment.CableReserveLeft.Should().Be(SystemUnderTest.NewLeftCableReserve);
            _equipment.CableReserveRight.Should().Be(SystemUnderTest.NewRightCableReserve);
            _equipment.Comment.Should().Be(SystemUnderTest.NewCommentForTest);
            _sut.ReadModel.Equipments.FirstOrDefault(e => e.EquipmentId == Guid.Empty).Should().BeNull();
        }

    }
}

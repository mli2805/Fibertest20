using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public class EquipmentAddedSimpleSteps
    {
        private readonly SutForEquipment _sut = new SutForEquipment();
        private Guid _nodeId;
        private Guid _oldEquipmentId;

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
            _sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation()).Wait();
            _sut.Poller.Tick();
            _nodeId = _sut.ReadModel.Nodes.Last().Id;
            _oldEquipmentId = _sut.ReadModel.Equipments.Last().Id;
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
            _sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentIntoNode() { NodeId = _nodeId }).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"Создается оборудование в узле")]
        public void ThenСоздаетсяОборудованиеВУзле()
        {
            var equipment = _sut.ReadModel.Equipments.First(e => e.NodeId == _nodeId);
            equipment.Title.Should().Be(SutForEquipment.NewTitleForTest);
            equipment.Type.Should().Be(SutForEquipment.NewTypeForTest);
            equipment.CableReserveLeft.Should().Be(SutForEquipment.NewLeftCableReserve);
            equipment.CableReserveRight.Should().Be(SutForEquipment.NewRightCableReserve);
            equipment.Comment.Should().Be(SutForEquipment.NewCommentForTest);

        }
        [Then(@"Оборудование в узле не создается")]
        public void ThenОборудованиеВУзлеНеСоздается()
        {
            _sut.ReadModel.Equipments.FirstOrDefault(e => e.NodeId == _nodeId).Should().BeNull();
        }

        [Then(@"Создается еще одно оборудование в том же узле")]
        public void ThenСоздаетсяЕщеОдноОборудованиеВТомЖеУзле()
        {
            _sut.ReadModel.Equipments.Count(e => e.NodeId == _nodeId).Should().Be(2);

            var equipment = _sut.ReadModel.Equipments.First(e => e.NodeId == _nodeId && e.Id != _oldEquipmentId);
            equipment.Title.Should().Be(SutForEquipment.NewTitleForTest);
            equipment.Type.Should().Be(SutForEquipment.NewTypeForTest);
            equipment.CableReserveLeft.Should().Be(SutForEquipment.NewLeftCableReserve);
            equipment.CableReserveRight.Should().Be(SutForEquipment.NewRightCableReserve);
            equipment.Comment.Should().Be(SutForEquipment.NewCommentForTest);
        }

    }
}

﻿using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public class EquipmentAddedSimpleSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _nodeId;
        private Iit.Fibertest.Graph.Equipment _equipment;

        [Given(@"Существует пустой узел")]
        public void GivenСуществуетПустойУзел()
        {
            _sut.ShellVm.ComplyWithRequest(new AddNode()).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _nodeId = _sut.ReadModel.Nodes.Last().Id;
        }

        [Given(@"Существует некоторый узел с оборудованием")]
        public void GivenСуществуетНекоторыйУзелСОборудованием()
        {
            _sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation()).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _nodeId = _sut.ReadModel.Nodes.Last().Id;
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
            _sut.Poller.EventSourcingTick().Wait();
            _equipment = _sut.ReadModel.Equipments.LastOrDefault();
        }

        [Then(@"Создается оборудование в узле")]
        public void ThenСоздаетсяОборудованиеВУзле()
        {
            var equipment = _sut.ReadModel.Equipments.First(e => e.NodeId == _nodeId);
            equipment.Title.Should().Be(SystemUnderTest.NewTitleForTest);
            equipment.Type.Should().Be(SystemUnderTest.NewTypeForTest);
            equipment.CableReserveLeft.Should().Be(SystemUnderTest.NewLeftCableReserve);
            equipment.CableReserveRight.Should().Be(SystemUnderTest.NewRightCableReserve);
            equipment.Comment.Should().Be(SystemUnderTest.NewCommentForTest);
            _sut.ReadModel.Equipments.FirstOrDefault(e => e.Id == Guid.Empty).Should().BeNull();

        }
        [Then(@"Оборудование в узле не создается")]
        public void ThenОборудованиеВУзлеНеСоздается()
        {
            _sut.ReadModel.Equipments.FirstOrDefault(e => e.NodeId == _nodeId).Should().BeNull();
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
            _sut.ReadModel.Equipments.FirstOrDefault(e => e.Id == Guid.Empty).Should().BeNull();
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class EquipmentAddedOnNodeUpdateSteps
    {
        private readonly SutForEquipmentAdded _sut = new SutForEquipmentAdded();
        private NodeUpdateViewModel _nodeUpdateViewModel;
        private int _cutOff, _itemsCount;

        [Given(@"Через узел проходят три трассы")]
        public void GivenЧерезУзелПроходятТриТрассы()
        {
            _sut.SetThreeTraceThroughNode();
        }

        [Given(@"Пользователь открывает форму редактирования узла")]
        public void GivenПользовательОткрываетФормуРедактированияУзла()
        {
            _nodeUpdateViewModel = new NodeUpdateViewModel(_sut.NodeId, _sut.ReadModel, _sut.FakeWindowManager, _sut.ShellVm.Bus);
        }

        [When(@"Пользователь жмет добавить оборудование вводит парамы и сохраняет")]
        public void WhenПользовательЖметДобавитьОборудованиеВводитПарамыИСохраняет()
        {
            _sut.FakeWindowManager.RegisterHandler(model =>
                _sut.TraceChoiceHandler(model, new List<Guid>() { _sut.TraceWithoutEqId }, Answer.Yes));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentInfoViewModelHandler(model, Answer.Yes));

            _nodeUpdateViewModel.AddEquipment();
            _sut.Poller.Tick();
        }

        [When(@"Пользователь жмет добавить оборудование вводит парамы и резко отказывается от сохранения")]
        public void WhenПользовательЖметДобавитьОборудованиеВводитПарамыИРезкоОтказываетсяОтСохранения()
        {
            _sut.FakeWindowManager.RegisterHandler(model =>
                _sut.TraceChoiceHandler(model, new List<Guid>() { _sut.TraceWithoutEqId }, Answer.Yes));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentInfoViewModelHandler(model, Answer.Cancel));

            _cutOff = _sut.Poller.CurrentEventNumber;
            _itemsCount = _nodeUpdateViewModel.EquipmentsInNode.Count;
            _nodeUpdateViewModel.AddEquipment();
            _sut.Poller.Tick();
        }

        [Then(@"Новое оборудование сохраняется и появляется на форме редактирования узла")]
        public void ThenНовоеОборудованиеСохраняетсяИПоявляетсяНаФормеРедактированияУзла()
        {
            var equipment = _sut.ReadModel.Equipments.Last();

            equipment.NodeId.Should().Be(_sut.NodeId);
            equipment.Title.Should().Be(SutForEquipment.NewTitleForTest);
            equipment.Type.Should().Be(SutForEquipment.NewTypeForTest);
            equipment.CableReserveLeft.Should().Be(SutForEquipment.NewLeftCableReserve);
            equipment.CableReserveRight.Should().Be(SutForEquipment.NewRightCableReserve);
            equipment.Comment.Should().Be(SutForEquipment.NewCommentForTest);

            var item = _nodeUpdateViewModel.EquipmentsInNode.First(it => it.Id == equipment.Id);
            item.Title.Should().Be(SutForEquipment.NewTitleForTest);
            item.Type.Should().Be(SutForEquipment.NewTypeForTest.ToLocalizedString());
            item.Comment.Should().Be(SutForEquipment.NewCommentForTest);
            item.Traces.Should().Be(_sut.ReadModel.Traces.First(t => t.Id == _sut.TraceWithoutEqId).Title+@" ;  ");
        }

        [Then(@"Новое оборудование НЕ сохраняется и НЕ появляется на форме редактирования узла")]
        public void ThenНовоеОборудованиеНеСохраняетсяИнеПоявляетсяНаФормеРедактированияУзла()
        {
            _sut.Poller.CurrentEventNumber.Should().Be(_cutOff);
            _nodeUpdateViewModel.EquipmentsInNode.Count.Should().Be(_itemsCount);
        }
    }
}

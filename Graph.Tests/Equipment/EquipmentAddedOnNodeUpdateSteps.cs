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
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Iit.Fibertest.Graph.Equipment _oldEquipment;
        private NodeUpdateViewModel _nodeUpdateViewModel;
        private int _cutOff, _itemsCount;
        private Guid _traceWithoutEqId;

        [Given(@"Через узел проходят три трассы")]
        public void GivenЧерезУзелПроходятТриТрассы()
        {
            _sut.SetThreeTraceThroughNode(out _oldEquipment, out _, out _, out _traceWithoutEqId);
        }

        [Given(@"Пользователь открывает форму редактирования узла")]
        public void GivenПользовательОткрываетФормуРедактированияУзла()
        {
            _nodeUpdateViewModel = new NodeUpdateViewModel(_oldEquipment.NodeId, _sut.ReadModel, _sut.FakeWindowManager, _sut.ShellVm.C2DWcfManager);
        }

        [When(@"Пользователь жмет добавить оборудование вводит парамы и сохраняет")]
        public void WhenПользовательЖметДобавитьОборудованиеВводитПарамыИСохраняет()
        {
            _sut.FakeWindowManager.RegisterHandler(model =>
                _sut.TraceChoiceHandler(model, new List<Guid>() { _traceWithoutEqId }, Answer.Yes));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentInfoViewModelHandler(model, Answer.Yes));

            _nodeUpdateViewModel.AddEquipmentIntoNode(false).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Пользователь жмет добавить оборудование вводит парамы и резко отказывается от сохранения")]
        public void WhenПользовательЖметДобавитьОборудованиеВводитПарамыИРезкоОтказываетсяОтСохранения()
        {
            _sut.FakeWindowManager.RegisterHandler(model =>
                _sut.TraceChoiceHandler(model, new List<Guid>() { _traceWithoutEqId }, Answer.Yes));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentInfoViewModelHandler(model, Answer.Cancel));

            _cutOff = _sut.Poller.CurrentEventNumber;
            _itemsCount = _nodeUpdateViewModel.EquipmentsInNode.Count;
            _nodeUpdateViewModel.AddEquipmentIntoNode(false).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Новое оборудование сохраняется и появляется на форме редактирования узла")]
        public void ThenНовоеОборудованиеСохраняетсяИПоявляетсяНаФормеРедактированияУзла()
        {
            var equipment = _sut.ReadModel.Equipments.Last();

            equipment.NodeId.Should().Be(_oldEquipment.NodeId);
            equipment.Title.Should().Be(SystemUnderTest.NewTitleForTest);
            equipment.Type.Should().Be(SystemUnderTest.NewTypeForTest);
            equipment.CableReserveLeft.Should().Be(SystemUnderTest.NewLeftCableReserve);
            equipment.CableReserveRight.Should().Be(SystemUnderTest.NewRightCableReserve);
            equipment.Comment.Should().Be(SystemUnderTest.NewCommentForTest);

            var item = _nodeUpdateViewModel.EquipmentsInNode.First(it => it.Id == equipment.Id);
            item.Title.Should().Be(SystemUnderTest.NewTitleForTest);
            item.Type.Should().Be(SystemUnderTest.NewTypeForTest.ToLocalizedString());
            item.Comment.Should().Be(SystemUnderTest.NewCommentForTest);
            item.Traces.Should().Be(_sut.ReadModel.Traces.First(t => t.Id == _traceWithoutEqId).Title+@" ;  ");

            _sut.ReadModel.Equipments.FirstOrDefault(e => e.Id == Guid.Empty).Should().BeNull();
        }

        [Then(@"Новое оборудование НЕ сохраняется и НЕ появляется на форме редактирования узла")]
        public void ThenНовоеОборудованиеНеСохраняетсяИнеПоявляетсяНаФормеРедактированияУзла()
        {
            _sut.Poller.CurrentEventNumber.Should().Be(_cutOff);
            _nodeUpdateViewModel.EquipmentsInNode.Count.Should().Be(_itemsCount);
        }
    }
}

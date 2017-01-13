using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class EquipmentAddedSteps
    {
        private readonly SystemUnderTest _sut;
        private EquipmentViewModel _equipmentViewModel;
        private Iit.Fibertest.Graph.Trace _saidTrace;
        private Guid _nodeWithoutEquipmentId;
        private Guid _nodeWithEquipmentId;
        private Guid _equipmentId;
        private List<Guid> _tracesForInsertion;

        private const string TitleForTest = "Name for equipment";
        private const EquipmentType TypeForTest = EquipmentType.CableReserve;
        private const int LeftCableReserve = 2;
        private const int RightCableReserve = 14;
        private const string CommentForTest = "Comment for equipment";

        public EquipmentAddedSteps(SystemUnderTest sut)
        {
            _sut = sut;
        }

        [Given(@"Трасса проходит через узел на котором пользователь кликает Добавить оборудование")]
        public void GivenТрассаПроходитЧерезНекоторыйУзел()
        {
            _saidTrace = _sut.ReadModel.Traces.Single();
            _nodeWithoutEquipmentId = _saidTrace.Nodes[1];
            _nodeWithEquipmentId = _saidTrace.Nodes[2];
        }

        [Given(@"Пользователь НЕ выбрал добавление оборудования в трассу")]
        public void GivenПользовательНЕВыбралДобавлениеОборудованияВТрассу()
        {
            _tracesForInsertion = new List<Guid>();
        }

        [Given(@"Пользователь выбрал добавление оборудования в трассу")]
        public void GivenПользовательВыбралДобавлениеОборудованияВТрассу()
        {
            _tracesForInsertion = new List<Guid> {_sut.ReadModel.Traces.Single().Id};
        }

        [Given(@"Открывыется окно для добавления оборудования во второй узел")]
        public void GivenAnAddEquipmentWindowOpenedForSaidNode()
        {
            _equipmentViewModel = new EquipmentViewModel(_nodeWithoutEquipmentId, Guid.Empty, _tracesForInsertion, _sut.ReadModel, _sut.Aggregate);
            _equipmentViewModel.Title = TitleForTest;
            _equipmentViewModel.Type = TypeForTest;
            _equipmentViewModel.CableReserveLeft = LeftCableReserve;
            _equipmentViewModel.CableReserveRight = RightCableReserve;
            _equipmentViewModel.Comment = CommentForTest;
        }

        [Given(@"Открывыется окно для добавления оборудования в третий узел")]
        public void GivenОткрывыетсяОкноДляДобавленияОборудованияВТретийУзел()
        {
            _equipmentViewModel = new EquipmentViewModel(_nodeWithEquipmentId, Guid.Empty, _tracesForInsertion, _sut.ReadModel, _sut.Aggregate);
            _equipmentViewModel.Title = TitleForTest;
            _equipmentViewModel.Type = TypeForTest;
            _equipmentViewModel.CableReserveLeft = LeftCableReserve;
            _equipmentViewModel.CableReserveRight = RightCableReserve;
            _equipmentViewModel.Comment = CommentForTest;
        }

        [Given(@"У трассы во втором узле нет оборудования")]
        public void GivenУТрассыВоВторомУзлеНетОборудования()
        {
            _saidTrace.Equipments[1].Should().Be(Guid.Empty);
        }

        [Given(@"В данном узле есть оборудование принадлежащее данной трассе")]
        public void GivenВДанномУзлеЕстьОборудованиеПринадлежащееДаннойТрассе()
        {
        }

        [Given(@"Для данной трассы задана базовая")]
        public void GivenДляДаннойТрассыЗаданаБазовая()
        {
            var _vm = new AssignBaseRefsViewModel(_saidTrace.Id, _sut.ReadModel, _sut.Aggregate);
            _vm.PreciseBaseFilename = @"..\..\base.sor";
            _vm.Save();
            _sut.Poller.Tick();
        }

        [When(@"Нажата клавиша Сохранить в окне добавления оборудования")]
        public void WhenSaveButtonOnAddEquipmentWindowPressed()
        {
            _equipmentViewModel.Save();
            _equipmentId = _equipmentViewModel.EquipmentId;
            _sut.Poller.Tick();
        }

        [When(@"Нажата клавиша Отменить в окне добавления оборудования")]
        public void WhenCancelButtonOnAddEquipmentWindowPressed()
        {
            _equipmentViewModel.Cancel();
        }

        [Then(@"Новое оборудование сохраняется")]
        public void ThenTheNewPieceOfEquipmentGetsSaved()
        {
            var equipment = _sut.ReadModel.Equipments.Single(e=>e.Id == _equipmentId);
            equipment.Title.Should().Be(TitleForTest);
            equipment.Type.Should().Be(TypeForTest);
            equipment.CableReserveLeft.Should().Be(LeftCableReserve);
            equipment.CableReserveRight.Should().Be(RightCableReserve);
            equipment.Comment.Should().Be(CommentForTest);
        }

        [Then(@"Новое оборудование НЕ сохраняется")]
        public void ThenНовоеОборудованиеНЕСохраняется()
        {
            _sut.ReadModel.Equipments.FirstOrDefault(e => e.Id == _equipmentId).Should().BeNull();
        }

        [Then(@"Новое оборудование НЕ добавляется в трассу")]
        public void ThenНовоеОборудованиеНЕДобавляетсяВТрассу()
        {
            var trace = _sut.ReadModel.Traces.Single();
            trace.Equipments[1].Should().NotBe(_equipmentId);
        }

        [Then(@"Новое оборудование добавляется в трассу")]
        public void ThenНовоеОборудованиеДобавляетсяВТрассу()
        {
            var trace = _sut.ReadModel.Traces.Single();
            trace.Equipments[1].Should().Be(_equipmentId);
        }

        [Then(@"Окно добавления оборудования закрывается")]
        public void ThenTheAddEquipmentWindowGetsClosed()
        {
            _equipmentViewModel.IsClosed.Should().BeTrue();
        }

        [Then(@"Окно добавления оборудования НЕ закрывается")]
        public void ThenОкноДобавленияОборудованияНЕЗакрывается()
        {
            _equipmentViewModel.IsClosed.Should().BeFalse();
        }
    }
}
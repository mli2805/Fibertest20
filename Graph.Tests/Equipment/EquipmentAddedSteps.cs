using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class EquipmentAddedSteps
    {
        private readonly SutForEquipmentAdded _sut = new SutForEquipmentAdded();
        private Guid _equipmentId;

        [Given(@"Дан узел с оборудованием")]
        public void GivenДанУзелСОборудованием()
        {
            _sut.SetNode();
        }

        [Given(@"Еще есть РТУ другие узлы и волокна")]
        public void GivenЕщеЕстьРтуДругиеУзлыИВолокна()
        {
            _sut.SetRtuAndOthers();
        }

        [Given(@"Одна трасса заканчивается в данном узле")]
        public void GivenОднаТрассаЗаканчиваетсяВДанномУзле()
        {
            _sut.SetShortTrace();
        }

        [Given(@"Одна трасса проходит через данный узел и использует оборудование")]
        public void GivenОднаТрассаПроходитЧерезДанныйУзелИИспользуетОборудование()
        {
            _sut.SetLongTraceWithEquipment();
        }

        [Given(@"Одна трасса проходит через данный узел но не использует оборудование")]
        public void GivenОднаТрассаПроходитЧерезДанныйУзелНоНеИспользуетОборудование()
        {
            _sut.SetLongTraceWithoutEquipment();
        }

        [Given(@"Для одной из трасс задана базовая")]
        public void GivenДляОднойИзТрассЗаданаБазовая()
        {
            var trace = _sut.ReadModel.Traces.First();
            var traceLeaf = (TraceLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(trace.Id);
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BaseRefAssignHandler(model, trace.Id, SystemUnderTest.Base1625, SystemUnderTest.Base1625, null, Answer.Yes));

            _sut.TraceLeafActions.AssignBaseRefs(traceLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Пользователь не отмечает ни одну трассу для включения оборудования")]
        public void ThenПользовательНеОтмечаетНиОднуТрассуДляВключенияОборудования()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.TraceChoiceHandler(model, new List<Guid>(), Answer.Yes));
        }

        [Then(@"Пользователь отмечает все трассы для включения оборудования")]
        public void ThenПользовательОтмечаетВсеТрассыДляВключенияОборудования()
        {
            _sut.FakeWindowManager.RegisterHandler(model =>
                    _sut.TraceChoiceHandler(model, new List<Guid>() { _sut.ShortTraceId, _sut.TraceWithEqId, _sut.TraceWithoutEqId },
                        Answer.Yes));
        }

        [Then(@"На форме выбора трасс эта трасса недоступна для выбора остальные доступны")]
        public void ThenНаФормеВыбораТрассЭтаТрассаНедоступнаДляВыбора()
        {
            var traceList = _sut.ShellVm.ReadModel.Traces.Where(t => t.Equipments.Contains(_sut.OldEquipmentId)).ToList();
            var traceChoiceVm = new TraceChoiceViewModel(traceList);
            traceChoiceVm.Choices.First(l => l.Id == _sut.ShortTraceId).IsEnabled.Should().BeFalse();
            foreach (var traceChoice in traceChoiceVm.Choices.Where(l => l.Id != _sut.ShortTraceId))
            {
                traceChoice.IsEnabled.Should().BeTrue();
            }
        }

        [Then(@"Пользователь вводит парамы оборудования и жмет Сохранить")]
        public void ThenПользовательВводитПарамОборудованияИЖметСохранить()
        {
            _sut.FakeWindowManager.RegisterHandler(model=> _sut.EquipmentInfoViewModelHandler(model, Answer.Yes));
        }

        [Then(@"Пользователь вводит парамы оборудования и жмет Отмена")]
        public void ThenПользовательВводитПарамыОборудованияИЖметОтмена()
        {
            _sut.FakeWindowManager.RegisterHandler(model=> _sut.EquipmentInfoViewModelHandler(model, Answer.Cancel));
        }

        [Then(@"В окне Добавить оборудование")]
        public void ThenВОкнеДобавитьОборудование()
        {
            _sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentIntoNode() { NodeId = _sut.NodeId }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"В узле создается новое оборудование")]
        public void ThenВУзлеСоздаетсяНовоеОборудование()
        {
            _sut.ReadModel.Equipments.Count(e => e.NodeId == _sut.NodeId).Should().Be(3);

            var equipment = _sut.ReadModel.Equipments.First(e => e.NodeId == _sut.NodeId && e.Id != _sut.OldEquipmentId && e.Type != EquipmentType.EmptyNode);

            equipment.Title.Should().Be(SutForEquipment.NewTitleForTest);
            equipment.Type.Should().Be(SutForEquipment.NewTypeForTest);
            equipment.CableReserveLeft.Should().Be(SutForEquipment.NewLeftCableReserve);
            equipment.CableReserveRight.Should().Be(SutForEquipment.NewRightCableReserve);
            equipment.Comment.Should().Be(SutForEquipment.NewCommentForTest);
            _sut.ReadModel.Equipments.FirstOrDefault(e => e.Id == Guid.Empty).Should().BeNull();

            _equipmentId = equipment.Id;
        }

        [Then(@"Но в трассы оно не входит")]
        public void ThenНоВТрассыОноНеВходит()
        {
            _sut.ReadModel.Traces.Any(t => t.Equipments.Contains(_equipmentId)).Should().BeFalse();
        }

        [Then(@"Новое оборудование входит во все трассы а старое ни в одну")]
        public void ThenНовоеОборудованиеВходитВоВсеТрассыАСтароеНиВОдну()
        {
            var shortTrace = _sut.ReadModel.Traces.First(t => t.Id == _sut.ShortTraceId);
            shortTrace.Equipments.Contains(_equipmentId).Should().BeTrue();
            shortTrace.Equipments.Count.ShouldBeEquivalentTo(shortTrace.Nodes.Count);

            var traceWithEqId = _sut.ReadModel.Traces.First(t => t.Id == _sut.TraceWithEqId);
            traceWithEqId.Equipments.Contains(_equipmentId).Should().BeTrue();
            traceWithEqId.Equipments.Count.ShouldBeEquivalentTo(traceWithEqId.Nodes.Count);

            var traceWithoutEqId = _sut.ReadModel.Traces.First(t => t.Id == _sut.TraceWithoutEqId);
            traceWithoutEqId.Equipments.Contains(_equipmentId).Should().BeTrue();
            traceWithoutEqId.Equipments.Count.ShouldBeEquivalentTo(traceWithoutEqId.Nodes.Count);

            _sut.ReadModel.Traces.Any(t => t.Equipments.Contains(_sut.OldEquipmentId)).Should().BeFalse();
            _sut.ReadModel.Equipments.FirstOrDefault(e => e.Id == Guid.Empty).Should().BeNull();
        }

        [Then(@"В узле НЕ создается новое оборудование")]
        public void ThenВУзлеНеСоздаетсяНовоеОборудование()
        {
            _sut.ReadModel.Equipments.Count(e => e.NodeId == _sut.NodeId).Should().Be(2);
        }
    }
}
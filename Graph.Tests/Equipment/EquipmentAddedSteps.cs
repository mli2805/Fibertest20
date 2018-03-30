using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph.Requests;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class EquipmentAddedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _equipmentId;
        private Guid _rtuNodeId;
        private Guid _anotherNodeId, _anotherNodeId2;

        private Iit.Fibertest.Graph.Equipment _oldEquipment;
        private Guid _shortTraceId, _traceWithEqId, _traceWithoutEqId;

        [Given(@"Дан узел с оборудованием")]
        public void GivenДанУзелСОборудованием()
        {
            _oldEquipment = _sut.SetNode();
        }

        [Given(@"Еще есть РТУ другие узлы и волокна")]
        public void GivenЕщеЕстьРтуДругиеУзлыИВолокна()
        {
            var rtu = _sut.SetRtuAndOthers(_oldEquipment.NodeId, out _anotherNodeId, out _anotherNodeId2);
            _rtuNodeId = rtu.NodeId;
            _sut.InitializeRtu(rtu.Id);
        }

        [Given(@"Одна трасса заканчивается в данном узле")]
        public void GivenОднаТрассаЗаканчиваетсяВДанномУзле()
        {
            _shortTraceId = _sut.SetShortTrace(_oldEquipment.NodeId, _rtuNodeId);
        }

        [Given(@"Одна трасса проходит через данный узел и использует оборудование")]
        public void GivenОднаТрассаПроходитЧерезДанныйУзелИИспользуетОборудование()
        {
            _traceWithEqId = _sut.SetLongTraceWithEquipment(_rtuNodeId, _anotherNodeId);
        }

        [Given(@"Одна трасса проходит через данный узел но не использует оборудование")]
        public void GivenОднаТрассаПроходитЧерезДанныйУзелНоНеИспользуетОборудование()
        {
            _traceWithoutEqId = _sut.SetLongTraceWithoutEquipment(_rtuNodeId, _anotherNodeId2);
        }

        [Given(@"Для одной из трасс задана базовая")]
        public void GivenДляОднойИзТрассЗаданаБазовая()
        {
            var trace = _sut.ReadModel.Traces.First();
            var traceLeaf = (TraceLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(trace.TraceId);

            _sut.AssignBaseRef(traceLeaf, SystemUnderTest.Base1625, SystemUnderTest.Base1625, null, Answer.Yes);
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
                    _sut.TraceChoiceHandler(model, new List<Guid>() { _shortTraceId, _traceWithEqId, _traceWithoutEqId },
                        Answer.Yes));
        }

        [Then(@"На форме выбора трасс эта трасса недоступна для выбора остальные доступны")]
        public void ThenНаФормеВыбораТрассЭтаТрассаНедоступнаДляВыбора()
        {
            var traceList = _sut.ReadModel.Traces.Where(t => t.EquipmentIds.Contains(_oldEquipment.EquipmentId)).ToList();
            var traceChoiceVm = new TracesToEquipmentInjectionViewModel(traceList);
            traceChoiceVm.Choices.First(l => l.Id == _shortTraceId).IsEnabled.Should().BeFalse();
            foreach (var traceChoice in traceChoiceVm.Choices.Where(l => l.Id != _shortTraceId))
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
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentIntoNode(new RequestAddEquipmentIntoNode() { NodeId = _oldEquipment.NodeId }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"В узле создается новое оборудование")]
        public void ThenВУзлеСоздаетсяНовоеОборудование()
        {
            _sut.ReadModel.Equipments.Count(e => e.NodeId == _oldEquipment.NodeId).Should().Be(3);

            var equipment = _sut.ReadModel.Equipments.First(e => e.NodeId == _oldEquipment.NodeId && e.EquipmentId != _oldEquipment.EquipmentId && e.Type != EquipmentType.EmptyNode);

            equipment.Title.Should().Be(SystemUnderTest.NewTitleForTest);
            equipment.Type.Should().Be(SystemUnderTest.NewTypeForTest);
            equipment.CableReserveLeft.Should().Be(SystemUnderTest.NewLeftCableReserve);
            equipment.CableReserveRight.Should().Be(SystemUnderTest.NewRightCableReserve);
            equipment.Comment.Should().Be(SystemUnderTest.NewCommentForTest);
            _sut.ReadModel.Equipments.FirstOrDefault(e => e.EquipmentId == Guid.Empty).Should().BeNull();

            _equipmentId = equipment.EquipmentId;
        }

        [Then(@"Но в трассы оно не входит")]
        public void ThenНоВТрассыОноНеВходит()
        {
            _sut.ReadModel.Traces.Any(t => t.EquipmentIds.Contains(_equipmentId)).Should().BeFalse();
        }

        [Then(@"Новое оборудование входит во все трассы а старое ни в одну")]
        public void ThenНовоеОборудованиеВходитВоВсеТрассыАСтароеНиВОдну()
        {
            var shortTrace = _sut.ReadModel.Traces.First(t => t.TraceId == _shortTraceId);
            shortTrace.EquipmentIds.Contains(_equipmentId).Should().BeTrue();
            shortTrace.EquipmentIds.Count.ShouldBeEquivalentTo(shortTrace.NodeIds.Count);

            var traceWithEqId = _sut.ReadModel.Traces.First(t => t.TraceId == _traceWithEqId);
            traceWithEqId.EquipmentIds.Contains(_equipmentId).Should().BeTrue();
            traceWithEqId.EquipmentIds.Count.ShouldBeEquivalentTo(traceWithEqId.NodeIds.Count);

            var traceWithoutEqId = _sut.ReadModel.Traces.First(t => t.TraceId == _traceWithoutEqId);
            traceWithoutEqId.EquipmentIds.Contains(_equipmentId).Should().BeTrue();
            traceWithoutEqId.EquipmentIds.Count.ShouldBeEquivalentTo(traceWithoutEqId.NodeIds.Count);

            _sut.ReadModel.Traces.Any(t => t.EquipmentIds.Contains(_oldEquipment.EquipmentId)).Should().BeFalse();
            _sut.ReadModel.Equipments.FirstOrDefault(e => e.EquipmentId == Guid.Empty).Should().BeNull();
        }

        [Then(@"В узле НЕ создается новое оборудование")]
        public void ThenВУзлеНеСоздаетсяНовоеОборудование()
        {
            _sut.ReadModel.Equipments.Count(e => e.NodeId == _oldEquipment.NodeId).Should().Be(2);
        }
    }
}
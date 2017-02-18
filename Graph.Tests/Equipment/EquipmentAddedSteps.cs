using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class EquipmentAddedSteps
    {
        private readonly SystemUnderTest2 _sut = new SystemUnderTest2();
        private Guid _nodeId, _rtuNodeId, _anotherNodeId, _anotherNodeId2;
        private Guid _shortTraceId, _traceWithEqId, _traceWithoutEqId;
        private Guid _equipmentId, _oldEquipmentId;
        private int _cutOff;

        private const EquipmentType EquipmentType = Iit.Fibertest.Graph.EquipmentType.Terminal;
        private const string Title = "some title";
        private const string Comment = "some comment";
        private const int LeftReserve = 5;
        private const int RightReserve = 77;

        [Given(@"Дан узел с оборудованием")]
        public void GivenДанУзелСОборудованием()
        {
            _sut.ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() {Type = EquipmentType.Sleeve}).Wait();
            _sut.Poller.Tick();
            _nodeId = _sut.ReadModel.Nodes.Last().Id;
            _oldEquipmentId = _sut.ReadModel.Equipments.Last().Id;
        }

        [Given(@"Еще есть РТУ другие узлы и волокна")]
        public void GivenЕщеЕстьРтуДругиеУзлыИВолокна()
        {
            _sut.ShellVm.ComplyWithRequest(new AddRtuAtGpsLocation()).Wait();
            _sut.Poller.Tick();
            _rtuNodeId = _sut.ReadModel.Nodes.Last().Id;

            _sut.ShellVm.ComplyWithRequest(new AddFiber() {Node1 = _rtuNodeId, Node2 = _nodeId}).Wait();
            _sut.Poller.Tick();

            _sut.ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() {Type = EquipmentType.Terminal}).Wait();
            _sut.Poller.Tick();
            _anotherNodeId = _sut.ReadModel.Nodes.Last().Id;

            _sut.ShellVm.ComplyWithRequest(new AddFiber() {Node1 = _anotherNodeId, Node2 = _nodeId}).Wait();
            _sut.Poller.Tick();

            _sut.ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() {Type = EquipmentType.Other}).Wait();
            _sut.Poller.Tick();
            _anotherNodeId2 = _sut.ReadModel.Nodes.Last().Id;

            _sut.ShellVm.ComplyWithRequest(new AddFiber() {Node1 = _anotherNodeId2, Node2 = _nodeId}).Wait();
            _sut.Poller.Tick();
        }

        [Given(@"Одна трасса заканчивается в данном узле")]
        public void GivenОднаТрассаЗаканчиваетсяВДанномУзле()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.QuestionAnswer("Accept the path?", Answer.Yes, model));
            _sut.FakeWindowManager.RegisterHandler(
                model => _sut.EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            _sut.FakeWindowManager.RegisterHandler(
                model => _sut.AddTraceViewHandler(model, "some title", "", Answer.Yes));

            _sut.ShellVm.ComplyWithRequest(new RequestAddTrace() {LastNodeId = _nodeId, NodeWithRtuId = _rtuNodeId})
                .Wait();
            _sut.Poller.Tick();
            _shortTraceId = _sut.ReadModel.Traces.Last().Id;
        }

        [Given(@"Одна трасса проходит через данный узел и использует оборудование")]
        public void GivenОднаТрассаПроходитЧерезДанныйУзелИИспользуетОборудование()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.QuestionAnswer("Accept the path?", Answer.Yes, model));
            _sut.FakeWindowManager.RegisterHandler(
                model => _sut.EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            _sut.FakeWindowManager.RegisterHandler(
                model => _sut.EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            _sut.FakeWindowManager.RegisterHandler(
                model => _sut.AddTraceViewHandler(model, "some title", "", Answer.Yes));

            _sut.ShellVm.ComplyWithRequest(new RequestAddTrace()
            {
                LastNodeId = _anotherNodeId,
                NodeWithRtuId = _rtuNodeId
            }).Wait();
            _sut.Poller.Tick();
            _traceWithEqId = _sut.ReadModel.Traces.Last().Id;
        }

        [Given(@"Одна трасса проходит через данный узел но не использует оборудование")]
        public void GivenОднаТрассаПроходитЧерезДанныйУзелНоНеИспользуетОборудование()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.QuestionAnswer("Accept the path?", Answer.Yes, model));
            _sut.FakeWindowManager.RegisterHandler(
                model => _sut.EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 1));
            _sut.FakeWindowManager.RegisterHandler(
                model => _sut.EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            _sut.FakeWindowManager.RegisterHandler(
                model => _sut.AddTraceViewHandler(model, "some title", "", Answer.Yes));

            _sut.ShellVm.ComplyWithRequest(new RequestAddTrace()
            {
                LastNodeId = _anotherNodeId2,
                NodeWithRtuId = _rtuNodeId
            }).Wait();
            _sut.Poller.Tick();
            _traceWithoutEqId = _sut.ReadModel.Traces.Last().Id;
        }

        [Given(@"Для одной из трасс задана базовая")]
        public void GivenДляОднойИзТрассЗаданаБазовая()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BaseRefAssignHandler(model, SystemUnderTest2.Path, SystemUnderTest2.Path, null, Answer.Yes));
            _sut.ShellVm.ComplyWithRequest(new RequestAssignBaseRef() { TraceId = _shortTraceId }).Wait();
            _sut.Poller.Tick();
        }



        [Then(@"Пользователь не отмечает ни одну трассу для включения оборудования")]
        public void ThenПользовательНеОтмечаетНиОднуТрассуДляВключенияОборудования()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.TraceChoiceHandler(model, new List<Guid>(), Answer.Yes));
        }

        [Then(@"Пользователь отмечает все трассы для включения оборудования")]
        public void ThenПользовательОтмечаетВсеТрассыДляВключенияОборудования()
        {
            _sut.FakeWindowManager.RegisterHandler(
                model =>
                    _sut.TraceChoiceHandler(model, new List<Guid>() {_shortTraceId, _traceWithEqId, _traceWithoutEqId},
                        Answer.Yes));
        }

        [Then(@"На форме выбора трасс эта трасса недоступна для выбора остальные доступны")]
        public void ThenНаФормеВыбораТрассЭтаТрассаНедоступнаДляВыбора()
        {
            var traceList = _sut.ShellVm.GraphVm.Traces.Where(t => t.Equipments.Contains(_oldEquipmentId)).ToList();
            var traceChoiceVm = new TraceChoiceViewModel(traceList);
            traceChoiceVm.Choices.First(l => l.Id == _shortTraceId).IsEnabled.Should().BeFalse();
            foreach (var traceChoice in traceChoiceVm.Choices.Where(l => l.Id != _shortTraceId))
            {
                traceChoice.IsEnabled.Should().BeTrue();
            }
        }

        [Then(@"Пользователь вводит парамы оборудования и жмет Сохранить")]
        public void ThenПользовательВводитПарамОборудованияИЖметСохранить()
        {
            _sut.FakeWindowManager.RegisterHandler(model =>
                _sut.EquipmentUpdateHandler(model, _nodeId, EquipmentType, Title, Comment, LeftReserve, RightReserve,
                    Answer.Yes));
        }

        [Then(@"Пользователь вводит парамы оборудования и жмет Отмена")]
        public void ThenПользовательВводитПарамыОборудованияИЖметОтмена()
        {
            _sut.FakeWindowManager.RegisterHandler(model =>
                _sut.EquipmentUpdateHandler(model, _nodeId, EquipmentType, Title, Comment, LeftReserve, RightReserve,
                    Answer.Cancel));
        }

        [Then(@"В окне Добавить оборудование")]
        public void ThenВОкнеДобавитьОборудование()
        {
            _sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentIntoNode() {NodeId = _nodeId}).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"В узле создается новое оборудование")]
        public void ThenВУзлеСоздаетсяНовоеОборудование()
        {
            _sut.ReadModel.Equipments.Count(e => e.NodeId == _nodeId).Should().Be(2);

            var equipment = _sut.ReadModel.Equipments.First(e => e.NodeId == _nodeId && e.Id != _oldEquipmentId);
            equipment.Title.Should().Be(Title);
            equipment.Type.Should().Be(EquipmentType);
            equipment.CableReserveLeft.Should().Be(LeftReserve);
            equipment.CableReserveRight.Should().Be(RightReserve);
            equipment.Comment.Should().Be(Comment);

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
            _sut.ReadModel.Traces.All(t => t.Equipments.Contains(_equipmentId)).Should().BeTrue();
            _sut.ReadModel.Traces.Any(t => t.Equipments.Contains(_oldEquipmentId)).Should().BeFalse();
        }

        [Then(@"В узле НЕ создается новое оборудование")]
        public void ThenВУзлеНеСоздаетсяНовоеОборудование()
        {
            _sut.ReadModel.Equipments.Count(e => e.NodeId == _nodeId).Should().Be(1);

        }
    }
}
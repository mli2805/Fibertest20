using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench;
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
            _sut.FakeWindowManager.BaseIsSet();

            var traceLeaf = (TraceLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_sut._shortTraceId);
            traceLeaf.AssignBaseRefsAction(null);
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
            _sut.FakeWindowManager.RegisterHandler(model =>
                    _sut.TraceChoiceHandler(model, new List<Guid>() { _sut._shortTraceId, _sut._traceWithEqId, _sut._traceWithoutEqId },
                        Answer.Yes));
        }

        [Then(@"На форме выбора трасс эта трасса недоступна для выбора остальные доступны")]
        public void ThenНаФормеВыбораТрассЭтаТрассаНедоступнаДляВыбора()
        {
            var traceList = _sut.ShellVm.ReadModel.Traces.Where(t => t.Equipments.Contains(_sut._oldEquipmentId)).ToList();
            var traceChoiceVm = new TraceChoiceViewModel(traceList);
            traceChoiceVm.Choices.First(l => l.Id == _sut._shortTraceId).IsEnabled.Should().BeFalse();
            foreach (var traceChoice in traceChoiceVm.Choices.Where(l => l.Id != _sut._shortTraceId))
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
            _sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentIntoNode() { NodeId = _sut._nodeId }).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"В узле создается новое оборудование")]
        public void ThenВУзлеСоздаетсяНовоеОборудование()
        {
            _sut.ReadModel.Equipments.Count(e => e.NodeId == _sut._nodeId).Should().Be(2);

            var equipment = _sut.ReadModel.Equipments.First(e => e.NodeId == _sut._nodeId && e.Id != _sut._oldEquipmentId);

            equipment.Title.Should().Be(SutForEquipment.NewTitleForTest);
            equipment.Type.Should().Be(SutForEquipment.NewTypeForTest);
            equipment.CableReserveLeft.Should().Be(SutForEquipment.NewLeftCableReserve);
            equipment.CableReserveRight.Should().Be(SutForEquipment.NewRightCableReserve);
            equipment.Comment.Should().Be(SutForEquipment.NewCommentForTest);

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
            _sut.ReadModel.Traces.First(t => t.Id == _sut._shortTraceId).Equipments.Contains(_equipmentId).Should().BeTrue();
            _sut.ReadModel.Traces.First(t => t.Id == _sut._traceWithEqId).Equipments.Contains(_equipmentId).Should().BeTrue();
            _sut.ReadModel.Traces.First(t => t.Id == _sut._traceWithoutEqId).Equipments.Contains(_equipmentId).Should().BeTrue();
            _sut.ReadModel.Traces.Any(t => t.Equipments.Contains(_sut._oldEquipmentId)).Should().BeFalse();
        }

        [Then(@"В узле НЕ создается новое оборудование")]
        public void ThenВУзлеНеСоздаетсяНовоеОборудование()
        {
            _sut.ReadModel.Equipments.Count(e => e.NodeId == _sut._nodeId).Should().Be(1);
        }
    }
}
using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class NodeIntoFiberAddedSteps
    {
        private readonly SystemUnderTest _scene = new SystemUnderTest();
        private Guid _a1Id;
        private Guid _b1Id;
        private Guid _nodeId;
        private Guid _equipmentId;
        private Iit.Fibertest.Graph.Fiber _fiber;
        private Iit.Fibertest.Graph.Trace _trace;

        [Given(@"Две трассы проходят через отрезок и две нет")]
        public void GivenЕстьДвеТрассыПроходящиеЧерезОтрезокИОднаНе()
        {
            _scene.CreatePositionForAddNodeIntoFiberTest(out _fiber, out _trace);
            _a1Id = _fiber.NodeId1;
            _b1Id = _fiber.NodeId2;

            _scene.InitializeRtu(_trace.RtuId);
        }

        [Given(@"Для трассы проходящей по данному отрезку задана базовая")]
        public void GivenДляДаннойТрассыЗаданаБазовая()
        {
            var traceLeaf = (TraceLeaf)_scene.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_trace.TraceId);

            _scene.AssignBaseRef(traceLeaf, SystemUnderTest.Base1625, SystemUnderTest.Base1625, null, Answer.Yes);
            traceLeaf.BaseRefsSet.PreciseId.Should().NotBe(Guid.Empty);
        }

        [When(@"Пользователь кликает добавить узел в первый отрезок этой трассы")]
        public void WhenПользовательКликаетДобавитьУзелВОтрезок()
        {
            _scene.FakeWindowManager.RegisterHandler(model => _scene.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            _scene.GraphReadModel.GrmNodeRequests.AddNodeIntoFiber(new RequestAddNodeIntoFiber() {FiberId = _fiber.FiberId}).Wait();
            _scene.Poller.EventSourcingTick().Wait();
            _nodeId = _scene.ReadModel.Nodes.Last().NodeId;
            _equipmentId = _scene.ReadModel.Equipments.Last().EquipmentId;
        }

        [Then(@"Старый отрезок удаляется и добавляются два новых и новый узел связывает их")]
        public void ThenВместоОтрезкаОбразуетсяДваНовыхИНовыйУзелСвязывающийИх()
        {
            _scene.ReadModel.Fibers.FirstOrDefault(f => f.FiberId == _fiber.FiberId).Should().Be(null);
            _scene.ReadModel.HasFiberBetween(_b1Id, _nodeId).Should().BeTrue();
            _scene.ReadModel.HasFiberBetween(_a1Id, _nodeId).Should().BeTrue();
        }

        [Then(@"Новый узел входит в трассу")]
        public void ThenНовыйУзелВходитВТрассуАСвязностьТрассыСохраняется()
        {
            _trace.NodeIds.Should().Contain(_nodeId);
            _trace.NodeIds.Count.ShouldBeEquivalentTo(_trace.EquipmentIds.Count);
            _trace.EquipmentIds.Contains(Guid.Empty).Should().BeFalse();
            _trace.EquipmentIds.Contains(_equipmentId).Should().BeTrue();
        }

        [Then(@"Cообщением об невозможности")]
        public void ThenCообщениемОбНевозможности()
        {
            _scene.FakeWindowManager.Log
                .OfType<MyMessageBoxViewModel>()
                .Last()
                .Lines[0].Line
                .Should().Be(Resources.SID_It_s_impossible_to_change_trace_with_base_reflectogram);
        }

    }
}

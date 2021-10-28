using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;

using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class NodeIntoFiberAddedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginOnEmptyBaseAsRoot();
        private Guid _a1Id;
        private Guid _b1Id;
        private Guid _nodeId;
        private Guid _equipmentId;
        private Iit.Fibertest.Graph.Fiber _fiber;
        private Iit.Fibertest.Graph.Trace _trace;

        [Given(@"Две трассы проходят через отрезок и две нет")]
        public void GivenЕстьДвеТрассыПроходящиеЧерезОтрезокИОднаНе()
        {
            _sut.CreatePositionForAddNodeIntoFiberTest(out _fiber, out _trace);
            _a1Id = _fiber.NodeId1;
            _b1Id = _fiber.NodeId2;

            _sut.SetNameAndAskInitializationRtu(_trace.RtuId);
        }

        [Given(@"Для трассы проходящей по данному отрезку задана базовая")]
        public void GivenДляДаннойТрассыЗаданаБазовая()
        {
            var traceLeaf = (TraceLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(_trace.TraceId);

            _sut.AssignBaseRef(traceLeaf, SystemUnderTest.Base1625, SystemUnderTest.Base1625, null, Answer.Yes);
            traceLeaf.BaseRefsSet.PreciseId.Should().NotBe(Guid.Empty);
        }

        [When(@"Пользователь кликает добавить узел в первый отрезок этой трассы")]
        public void WhenПользовательКликаетДобавитьУзелВОтрезок()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            _sut.GraphReadModel.GrmNodeRequests.AddNodeIntoFiber(new RequestAddNodeIntoFiber() {FiberId = _fiber.FiberId}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _nodeId = _sut.ReadModel.Nodes.Last().NodeId;
            _equipmentId = _sut.ReadModel.Equipments.Last().EquipmentId;
        }

        [Then(@"Старый отрезок удаляется и добавляются два новых и новый узел связывает их")]
        public void ThenВместоОтрезкаОбразуетсяДваНовыхИНовыйУзелСвязывающийИх()
        {
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.FiberId == _fiber.FiberId).Should().Be(null);
            _sut.ReadModel.Fibers.Any(f =>
                f.NodeId1 == _b1Id && f.NodeId2 == _nodeId ||
                f.NodeId1 == _nodeId && f.NodeId2 == _b1Id).Should().BeTrue();
            _sut.ReadModel.Fibers.Any(f =>
                f.NodeId1 == _a1Id && f.NodeId2 == _nodeId ||
                f.NodeId1 == _nodeId && f.NodeId2 == _a1Id).Should().BeTrue();
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
            _sut.FakeWindowManager.Log
                .OfType<MyMessageBoxViewModel>()
                .Last()
                .Lines[0].Line
                .Should().Be(Resources.SID_It_s_impossible_to_change_trace_with_base_reflectogram);
        }

    }
}

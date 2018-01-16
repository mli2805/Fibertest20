using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.StringResources;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class NodeIntoFiberAddedSteps
    {
        private readonly SutForAddNodeIntoFiber _sut = new SutForAddNodeIntoFiber();
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
            _a1Id = _fiber.Node1;
            _b1Id = _fiber.Node2;
        }

        [Given(@"Для трассы проходящей по данному отрезку задана базовая")]
        public void GivenДляДаннойТрассыЗаданаБазовая()
        {
            var traceLeaf = (TraceLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_trace.Id);
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BaseRefAssignHandler(model, _trace.Id, SystemUnderTest.Path, SystemUnderTest.Path, null, Answer.Yes));
            _sut.TraceLeafActions.AssignBaseRefs(traceLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Пользователь кликает добавить узел в первый отрезок этой трассы")]
        public void WhenПользовательКликаетДобавитьУзелВОтрезок()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            _sut.ShellVm.ComplyWithRequest(new RequestAddNodeIntoFiber() {FiberId = _fiber.Id}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _nodeId = _sut.ReadModel.Nodes.Last().Id;
            _equipmentId = _sut.ReadModel.Equipments.Last().Id;
        }

        [Then(@"Старый отрезок удаляется и добавляются два новых и новый узел связывает их")]
        public void ThenВместоОтрезкаОбразуетсяДваНовыхИНовыйУзелСвязывающийИх()
        {
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.Id == _fiber.Id).Should().Be(null);
            _sut.ReadModel.HasFiberBetween(_b1Id, _nodeId).Should().BeTrue();
            _sut.ReadModel.HasFiberBetween(_a1Id, _nodeId).Should().BeTrue();
        }

        [Then(@"Новый узел входит в трассу")]
        public void ThenНовыйУзелВходитВТрассуАСвязностьТрассыСохраняется()
        {
            _trace.Nodes.Should().Contain(_nodeId);
            _trace.Nodes.Count.ShouldBeEquivalentTo(_trace.Equipments.Count);
            _trace.Equipments.Contains(Guid.Empty).Should().BeFalse();
            _trace.Equipments.Contains(_equipmentId).Should().BeTrue();
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

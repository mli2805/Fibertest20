using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench;
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
            _sut.FakeWindowManager.BaseIsSet();
            _sut.ShellVm.ComplyWithRequest(new RequestAssignBaseRef() { TraceId = _trace.Id });
            _sut.Poller.Tick();
        }

        [When(@"Пользователь кликает добавить узел в первый отрезок этой трассы")]
        public void WhenПользовательКликаетДобавитьУзелВОтрезок()
        {
            _sut.ShellVm.ComplyWithRequest(new RequestAddNodeIntoFiber() {FiberId = _fiber.Id}).Wait();
            _sut.Poller.Tick();
            _nodeId = _sut.ReadModel.Nodes.Last().Id;
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
        }

        [Then(@"Cообщением об невозможности")]
        public void ThenCообщениемОбНевозможности()
        {
            _sut.FakeWindowManager.Log
                .OfType<NotificationViewModel>()
                .Last()
                .Message
                .Should().Be(Resources.SID_It_s_impossible_to_change_trace_with_base_reflectogram);
        }

    }
}

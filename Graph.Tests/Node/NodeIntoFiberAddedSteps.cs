using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class NodeIntoFiberAddedSteps
    {
        private readonly SystemUnderTest _sut;
        private Guid _a1Id;
        private Guid _b1Id;
        private Guid _nodeId;
        private Guid _fiberId;

        public NodeIntoFiberAddedSteps(SystemUnderTest sut)
        {
            _sut = sut;
        }

        [Given(@"Две трассы проходят через отрезок и две нет")]
        public void GivenЕстьДвеТрассыПроходящиеЧерезОтрезокИОднаНе()
        {
            _sut.CreatePositionForAddNodeIntoFiberTest();
            _fiberId = _sut.ReadModel.Fibers.First().Id;
            _a1Id = _sut.ReadModel.Traces.First().Nodes[1];
            _b1Id = _sut.ReadModel.Traces.First().Nodes[2];
        }

        [Given(@"Для трассы проходящей по данному отрезку задана базовая")]
        public void GivenДляДаннойТрассыЗаданаБазовая()
        {
            var vm = new AssignBaseRefsViewModel(_sut.ReadModel.Traces.First().Id, _sut.ReadModel, _sut.Aggregate);
            vm.PreciseBaseFilename = @"..\..\base.sor";
            vm.Save();
            _sut.Poller.Tick();
        }

        [When(@"Пользователь кликает добавить узел в первый отрезок этой трассы")]
        public void WhenПользовательКликаетДобавитьУзелВОтрезок()
        {
            _sut.MapVm.AddNodeIntoFiber(_fiberId);
            _sut.Poller.Tick();
            _nodeId = _sut.ReadModel.Nodes.Last().Id;
        }

        [Then(@"Старый отрезок удаляется и добавляются два новых и новый узел связывает их")]
        public void ThenВместоОтрезкаОбразуетсяДваНовыхИНовыйУзелСвязывающийИх()
        {
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.Id == _fiberId).Should().Be(null);
            _sut.ReadModel.HasFiberBetween(_b1Id, _nodeId).Should().BeTrue();
            _sut.ReadModel.HasFiberBetween(_a1Id, _nodeId).Should().BeTrue();
        }

        [Then(@"Новый узел входит в трассу")]
        public void ThenНовыйУзелВходитВТрассуАСвязностьТрассыСохраняется()
        {
            var trace = _sut.ReadModel.Traces.First();
            trace.Nodes.Should().Contain(_nodeId);
        }

        [Then(@"Отказ с сообщением (.*)")]
        public void ThenСообщение(string message)
        {
            _sut.FakeWindowManager.Log
                .OfType<ErrorNotificationViewModel>()
                .Last()
                .ErrorMessage
                .Should().Be(message);
        }
    }
}

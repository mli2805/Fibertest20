using System;
using System.Linq;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class NodeIntoFiberAddedSteps
    {
        private readonly SystemUnderTest _sut;
        private Guid _nodeForRtuId;
        private Guid _firstNodeId;
        private Guid _nodeId;
        private Guid _fiberId;

        public NodeIntoFiberAddedSteps(SystemUnderTest sut)
        {
            _sut = sut;
        }

        [Given(@"Есть трасса")]
        public void GivenЕстьТрасса()
        {

            _sut.CreateTrace();
            _nodeForRtuId = _sut.ReadModel.Traces.First().Nodes[0];
            _firstNodeId = _sut.ReadModel.Traces.First().Nodes[1];
            _fiberId = _sut.ReadModel.Fibers.First().Id;
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
            _sut.ReadModel.HasFiberBetween(_firstNodeId, _nodeId).Should().BeTrue();
            _sut.ReadModel.HasFiberBetween(_nodeForRtuId, _nodeId).Should().BeTrue();
        }

        [Then(@"Новый узел входит в трассу")]
        public void ThenНовыйУзелВходитВТрассуАСвязностьТрассыСохраняется()
        {
            var trace = _sut.ReadModel.Traces.First();
            trace.Nodes.Should().Contain(_nodeId);
        }

        [Then(@"Отказ с сообщением")]
        public void ThenОтказССообщением()
        {
        }
    }
}

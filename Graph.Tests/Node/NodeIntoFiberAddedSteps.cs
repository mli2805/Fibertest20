using System;
using System.Linq;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class NodeIntoFiberAddedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _leftNodeId;
        private Guid _rightNodeId;
        private Guid _fiberId;

        [Given(@"Есть левый и правый узлы и отрезок между ними")]
        public void GivenЕстьЛевыйИПравыйУзлыИОтрезокМеждуНими()
        {
            _leftNodeId = _sut.AddNode();
            _rightNodeId = _sut.AddNode();
            _fiberId = _sut.AddFiber(_leftNodeId, _rightNodeId);
        }

        [When(@"Пользователь кликает добавить узел в отрезок")]
        public void WhenПользовательКликаетДобавитьУзелВОтрезок()
        {
            _sut.AddNodeIntoFiber(_fiberId);
        }

        [Then(@"Вместо отрезка образуется два новых и новый узел связывающий их")]
        public void ThenВместоОтрезкаОбразуетсяДваНовыхИНовыйУзелСвязывающийИх()
        {
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.Id == _fiberId).Should().Be(null);
        }
    }
}

using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class EquipmentAtGpsLocationAddedSteps
    {
        private readonly SystemUnderTest2 _sut = new SystemUnderTest2();

        [When(@"Пользователь кликает добавить узел с оборудованием")]
        public void WhenПользовательКликаетДобавитьУзелСОборудованием()
        {
            _sut.ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() {Type = EquipmentType.Terminal}).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"Новый узел с оборудованием сохраняется")]
        public void ThenНовыйУзелСОборудованиемСохраняется()
        {
            _sut.ReadModel.Equipments.Single().NodeId.Should().Be(_sut.ReadModel.Nodes.Single().Id);
        }
    }
}

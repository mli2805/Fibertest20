using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class EquipmentAtGpsLocationAddedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();

        [When(@"Пользователь кликает добавить узел с оборудованием")]
        public void WhenПользовательКликаетДобавитьУзелСОборудованием()
        {
            _sut.GraphReadModel.GrmNodeRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() {Type = EquipmentType.Terminal}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Новый узел с оборудованием сохраняется")]
        public void ThenНовыйУзелСОборудованиемСохраняется()
        {
            _sut.ReadModel.Equipments.Last().NodeId.Should().Be(_sut.ReadModel.Nodes.Last().Id);
            _sut.ReadModel.Equipments.FirstOrDefault(e => e.Id == Guid.Empty).Should().BeNull();
        }
    }
}

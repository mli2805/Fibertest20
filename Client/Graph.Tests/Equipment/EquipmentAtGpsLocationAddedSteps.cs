using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class EquipmentAtGpsLocationAddedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginOnEmptyBaseAsRoot();

        [When(@"Пользователь кликает добавить узел с оборудованием")]
        public void WhenПользовательКликаетДобавитьУзелСОборудованием()
        {
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() {Type = EquipmentType.Terminal}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Новый узел с оборудованием сохраняется")]
        public void ThenНовыйУзелСОборудованиемСохраняется()
        {
            _sut.ReadModel.Equipments.Last().NodeId.Should().Be(_sut.ReadModel.Nodes.Last().NodeId);
            _sut.ReadModel.Equipments.FirstOrDefault(e => e.EquipmentId == Guid.Empty).Should().BeNull();
        }
    }
}

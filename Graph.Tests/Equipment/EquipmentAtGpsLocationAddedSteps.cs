using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests.Equipment
{
    [Binding]
    public sealed class EquipmentAtGpsLocationAddedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private readonly MapViewModel _vm;

        public EquipmentAtGpsLocationAddedSteps()
        {
            _vm = new MapViewModel(_sut.Aggregate);
        }

        [When(@"Пользователь кликает добавить узел с оборудованием")]
        public void WhenПользовательКликаетДобавитьУзелСОборудованием()
        {
            _vm.AddEquipmentAtGpsLocation();
            _sut.Poller.Tick();
        }

        [Then(@"Новый узел с оборудованием сохраняется")]
        public void ThenНовыйУзелСОборудованиемСохраняется()
        {
            _sut.ReadModel.Equipments.Single().NodeId.Should().Be(_sut.ReadModel.Nodes.Single().Id);
        }
    }
}

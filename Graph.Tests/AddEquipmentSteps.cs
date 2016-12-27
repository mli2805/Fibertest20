using System;
using FluentAssertions;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class AddEquipmentSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _saidNodeId;
        private AddEquipmentViewModel _window;
        private int _cutOff;

        [Given(@"A container-node created")]
        public void GivenAContainer_NodeCreated()
        {
            _saidNodeId = _sut.AddNode();
            _cutOff = _sut.CurrentEventNumber;
        }

        [Given(@"An Add Equipment window opened for said node")]
        public void GivenAnAddEquipmentWindowOpenedForSaidNode()
        {
            _window = new AddEquipmentViewModel(_saidNodeId, _sut.ReadModel, _sut.Aggregate);
        }

        [When(@"When Save button on Add Equipment window pressed")]
        public void WhenWhenSaveButtonOnAddEquipmentWindowPressed()
        {
            _window.Save();
        }

        [Then(@"The new piece of equipment gets saved")]
        public void ThenTheNewPieceOfEquipmentGetsSaved()
        {
            _sut.CurrentEventNumber.Should().BeGreaterThan(_cutOff);
        }

        [Then(@"The Add Equipment window gets closed")]
        public void ThenTheAddEquipmentWindowGetsClosed()
        {
            _window.IsClosed.Should().BeTrue();
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.WpfClient.ViewModels;
using PrivateReflectionUsingDynamic;
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

    public class SystemUnderTest
    {
        public Aggregate Aggregate { get; } = new Aggregate();
        public ReadModel ReadModel { get; } = new ReadModel();
        private int _currentEventNumber;
        public int CurrentEventNumber => _currentEventNumber + Aggregate.Events.Count;

        public Guid AddNode()
        {
            var newGuid = Guid.NewGuid();
            var cmd = new AddNode
            {
                Id = newGuid
            };

            Aggregate.When(cmd);

            Apply();

            return newGuid;
        }

        private void Apply()
        {
            foreach (var e in Aggregate.Events)
                ReadModel.AsDynamic().Apply(e);
            _currentEventNumber += Aggregate.Events.Count;
            Aggregate.Events.Clear();
        }

        public void UpdateNode(Guid nodeId, string title)
        {
            var cmd = new UpdateNode()
            {
                Id = nodeId,
                Title = title,
            };

            Aggregate.When(cmd);

            Apply();
        }
    }
}
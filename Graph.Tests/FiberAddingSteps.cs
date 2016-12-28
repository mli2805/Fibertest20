using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class FiberAddingSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _leftNodeId;
        private Guid _rightNodeId;
        private int _cutOff;

        [Given(@"Left and right nodes created")]
        public void GivenALeftAndRightNodesCreated()
        {
            _leftNodeId = _sut.AddNode();
            _cutOff = _sut.CurrentEventNumber;
            _rightNodeId = _sut.AddNode();
            _cutOff = _sut.CurrentEventNumber;
        }

        [When(@"User clicked Add fiber")]
        public void WhenUserClickedAddFiber()
        {
            _sut.AddFiber(_leftNodeId, _rightNodeId);
        }

        [Then(@"New event persisted")]
        public void ThenNewEventPersisted()
        {
            _sut.CurrentEventNumber.Should().BeGreaterThan(_cutOff);
        }


    }
}

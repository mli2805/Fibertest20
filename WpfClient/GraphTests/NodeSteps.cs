using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.WpfClient.ViewModels;
using Iit.Fibertest.WpfClient.Views;
using TechTalk.SpecFlow;

namespace Iit.Fibertest.GraphTests
{
    [Binding]
    public sealed class NodeSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _saidNode;
        private UpdateNodeViewModel _window;
        private int _cutOff;

        [Given(@"A node created")]
        public void CreateNode()
        {
            _saidNode = _sut.AddNode();
            _cutOff = _sut.CurrentEvent;
        }

        [Given(@"An update window opened for said node")]
        public void OpenWindow()
        {
            _window = new UpdateNodeViewModel(_saidNode);
        }
        [When(@"Save button pressed")]
        public void Save()
        {
            _window.Save();
        }
        [Then(@"Nothing gets saved")]
        public void AssertThereAreNoNewEvents()
        {
            _sut.CurrentEvent.Should().Be(_cutOff);
        }
        [Then(@"The window gets closed")]
        public void AssertTheWindowIsClosed()
        {
            _window.IsActive.Should().BeFalse();
        }
        [Then(@"The change gets saved")]
        public void AssertThereAreNewEvents()
        {
            //TODO: replace with an actual check with UI
            _sut.CurrentEvent.Should().BeGreaterThan(_cutOff);
        }

        [Then(@"Title field is red")]
        public void ThenTitleFieldIsRed()
        {
            _window.Error.Should().NotBeNullOrEmpty();
        }
        [Then(@"The window is not closed")]
        public void ThenTheWindowIsNotClosed()
        {
            _window.IsActive.Should().BeTrue();
        }

    }
}

public class SystemUnderTest
{
    public Aggregate Aggregate { get; } = new Aggregate();
    public int CurrentEvent => Aggregate.Events.Count;

    public Guid AddNode()
    {
        var newGuid = Guid.NewGuid();
        var cmd = new AddNode
        {
            Id = newGuid
        };
        Aggregate.When(cmd);
        return newGuid;
    }
}


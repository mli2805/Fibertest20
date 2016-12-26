using System;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.WpfClient.ViewModels;
using PrivateReflectionUsingDynamic;
using TechTalk.SpecFlow;

namespace Iit.Fibertest.GraphTests
{
    [Binding]
    public sealed class NodeSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _saidNodeId;
        private UpdateNodeViewModel _window;
        private int _cutOff;

        [Given(@"A node created")]
        public void CreateNode()
        {
            _saidNodeId = _sut.AddNode();
            _cutOff = _sut.CurrentEventNumber;
        }

        [Given(@"A node created with title (.*)")]
        public void CreateNode(string title)
        {
            _saidNodeId = _sut.AddNode();
            _sut.UpdateNode(_saidNodeId, title);
            _cutOff = _sut.CurrentEventNumber;
        }

        [Given(@"An update window opened for said node")]
        public void OpenWindow()
        {
            _window = new UpdateNodeViewModel(_saidNodeId, _sut.ReadModel, _sut.Aggregate);
        }
        [Given(@"Title was set to (.*)")]
        public void GivenTitleWasSetToBlah_Blah(string title)
        {
            _window.Title = title;
        }

        [When(@"Save button pressed")]
        public void Save()
        {
            _window.Save();
        }

        [When(@"Cancel button pressed")]
        public void WhenCancelButtonPressed()
        {
            _window.Cancel();
        }

        [Then(@"Nothing gets saved")]
        public void AssertThereAreNoNewEvents()
        {
            _sut.CurrentEventNumber.Should().Be(_cutOff);
        }
        [Then(@"The window gets closed")]
        public void AssertTheWindowIsClosed()
        {
            _window.IsClosed.Should().BeTrue();
        }
        [Then(@"The change gets saved")]
        public void AssertThereAreNewEvents()
        {
            //TODO: replace with an actual check with UI
            _sut.CurrentEventNumber.Should().BeGreaterThan(_cutOff);
        }

        [Then(@"Title field is red")]
        public void ThenTitleFieldIsRed()
        {
            _window.Error.Should().NotBeNullOrEmpty();
        }
        [Then(@"The window is not closed")]
        public void ThenTheWindowIsNotClosed()
        {
            _window.IsClosed.Should().BeFalse();
        }

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


using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;

using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class FiberAddedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginAsRoot(Answer.Yes);
        private Guid _leftNodeId;
        private Guid _rightNodeId;
        private int _cutOff;
        private Guid _fiberId;


        [Given(@"Левый и правый узлы созданы")]
        public void GivenALeftAndRightNodesCreated()
        {
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _leftNodeId = _sut.ReadModel.Nodes.Last().NodeId;
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _rightNodeId = _sut.ReadModel.Nodes.Last().NodeId;
            _cutOff = _sut.CurrentEventNumber;
        }

        [Given(@"Отрезок между левым и правым узлом уже добавлен")]
        public void AddFiber()
        {
            _sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() {FiberId = _fiberId, NodeId1 = _leftNodeId, NodeId2 = _rightNodeId}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _cutOff = _sut.CurrentEventNumber;
            _fiberId = _sut.ReadModel.Fibers.Last().FiberId;
        }

        [Given(@"На нем есть точка привязки")]
        public void GivenНаНемЕстьТочкаПривязки()
        {
            _sut.GraphReadModel.GrmNodeRequests.AddNodeIntoFiber(new RequestAddNodeIntoFiber() { FiberId = _fiberId, InjectionType = EquipmentType.AdjustmentPoint }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _cutOff = _sut.CurrentEventNumber;
        }


        [When(@"Пользователь кликает добавить отрезок")]
        public void WhenUserClickedAddFiber()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            _sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() {NodeId1 = _leftNodeId, NodeId2 = _rightNodeId}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Новый отрезок сохранен")]
        public void ThenNewEventPersisted()
        {
            _sut.ReadModel.Fibers.Where(f => f.NodeId1 == _leftNodeId && f.NodeId2 == _rightNodeId ||
                                             f.NodeId2 == _leftNodeId && f.NodeId1 == _rightNodeId).Should().NotBeNull();
        }

        [Then(@"Появится сообщение что есть такое волокно")]
        public void ThenПоявитсяСообщениеЧтоЕстьТакоеВолокно()
        {
            _sut.FakeWindowManager.Log
                .OfType<MyMessageBoxViewModel>()
                .Last()
                .Lines[0].Line
                .Should().Be(Resources.SID_Section_already_exists);
        }

        [Then(@"Новый отрезок не создается")]
        public void ThenНовыйОтрезокНеСоздается()
        {
            _sut.CurrentEventNumber.Should().Be(_cutOff);
        }
    }
}

using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class FiberUpdatedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _saidFiberId;
        private int _cutOff;
        private int _userInputLength;

        
        [Given(@"Существует отрезок")]
        public void GivenСуществуетОтрезок()
        {
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation()).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            var n1 = _sut.ReadModel.Nodes.Last().NodeId;

            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation()).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            var n2 = _sut.ReadModel.Nodes.Last().NodeId;

            _sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() {NodeId1 = n1, NodeId2 = n2}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _saidFiberId = _sut.ReadModel.Fibers.Last().FiberId;

            _cutOff = _sut.CurrentEventNumber;
        }

        [When(@"Пользователь ввел длину участка (.*) и нажал сохранить")]
        public void WhenПользовательВвелДлинуУчасткаИНажалСохранить(int p0)
        {
            _userInputLength = p0;
            _sut.FakeWindowManager.RegisterHandler(model => _sut.FiberUpdateHandler(model, _userInputLength, Answer.Yes));
            _sut.GraphReadModel.GrmFiberRequests.UpdateFiber(new RequestUpdateFiber() {Id = _saidFiberId}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Пользователь ввел длину участка (.*) и нажал отмена")]
        public void WhenПользовательВвелДлинуУчасткаИНажалОтмена(int p0)
        {
            _userInputLength = p0;
            _sut.FakeWindowManager.RegisterHandler(model => _sut.FiberUpdateHandler(model, _userInputLength, Answer.Cancel));
            _sut.GraphReadModel.GrmFiberRequests.UpdateFiber(new RequestUpdateFiber() { Id = _saidFiberId }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Сохраняется длина участка")]
        public void ThenСохраняетсяДлинаУчастка()
        {
            _sut.Poller.EventSourcingTick().Wait();
            _sut.ReadModel.Fibers.First(f => f.FiberId == _saidFiberId).UserInputedLength.Should().Be(_userInputLength);
        }

        [Then(@"Команда не отсылается")]
        public void ThenКомандаНеОтсылается()
        {
            _sut.CurrentEventNumber.Should().Be(_cutOff);
        }
    }
}

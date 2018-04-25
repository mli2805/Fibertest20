using System;
using System.Linq;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    public class SceneForHideTraces : SystemUnderTest
    {
        public Guid EmptyNode1Id, EmptyNode2Id;
        public Guid CommonClosureNodeId { get; set; }
        public Guid CommonTerminalNodeId { get; set; }

        public Guid NodeAId, NodeBId, NodeCId;
        public Iit.Fibertest.Graph.Rtu Rtu1, Rtu2;
        public Iit.Fibertest.Graph.Trace Trace1 { get; set; }
        public Iit.Fibertest.Graph.Trace Trace2 { get; set; }


        public void CreateRtu1WithTrace1()
        {
            FakeWindowManager.RegisterHandler(model => this.RtuUpdateHandler(model, @"RTU2", @"doesn't matter", Answer.Yes));
            GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 });
            Poller.EventSourcingTick().Wait();
            Rtu1 = ReadModel.Rtus.Last();

            GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation { Type = EquipmentType.EmptyNode }).Wait();
            Poller.EventSourcingTick().Wait();
            EmptyNode1Id = ReadModel.Nodes.Last().NodeId;

            GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation { Type = EquipmentType.Closure }).Wait();
            Poller.EventSourcingTick().Wait();
            CommonClosureNodeId = ReadModel.Nodes.Last().NodeId;

            GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation { Type = EquipmentType.Terminal }).Wait();
            Poller.EventSourcingTick().Wait();
            CommonTerminalNodeId = ReadModel.Nodes.Last().NodeId;

            GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = Rtu1.NodeId, NodeId2 = EmptyNode1Id }).Wait();
            GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = EmptyNode1Id, NodeId2 = CommonClosureNodeId }).Wait();
            GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = CommonClosureNodeId, NodeId2 = CommonTerminalNodeId }).Wait();
            Poller.EventSourcingTick().Wait();

            Trace1 = this.DefineTrace(CommonTerminalNodeId, Rtu1.NodeId, @"trace1", 3);
        }

        public void CreateRtu2WithTrace2PartlyOverlappingTrace1()
        {
            FakeWindowManager.RegisterHandler(model => this.RtuUpdateHandler(model, @"RTU1", @"doesn't matter", Answer.Yes));
            GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 });
            Poller.EventSourcingTick().Wait();
            Rtu2 = ReadModel.Rtus.Last();

            GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation { Type = EquipmentType.EmptyNode }).Wait();
            Poller.EventSourcingTick().Wait();
            EmptyNode2Id = ReadModel.Nodes.Last().NodeId;

            GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = Rtu2.NodeId, NodeId2 = EmptyNode2Id }).Wait();
            GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = EmptyNode2Id, NodeId2 = CommonClosureNodeId }).Wait();
            Poller.EventSourcingTick().Wait();

            Trace2 = this.DefineTrace(CommonTerminalNodeId, Rtu2.NodeId, @"trace2", 3);
        }

        public void CreateFiberToRtu2()
        {
            GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation { Type = EquipmentType.Other }).Wait();
            Poller.EventSourcingTick().Wait();
            NodeAId = ReadModel.Nodes.Last().NodeId;

            GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = Rtu2.NodeId, NodeId2 = NodeAId }).Wait();
        }

        public void CreateFiberBtoC()
        {
            GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation { Type = EquipmentType.Other }).Wait();
            Poller.EventSourcingTick().Wait();
            NodeBId = ReadModel.Nodes.Last().NodeId;

            GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation { Type = EquipmentType.Other }).Wait();
            Poller.EventSourcingTick().Wait();
            NodeCId = ReadModel.Nodes.Last().NodeId;

            GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = NodeBId, NodeId2 = NodeCId }).Wait();
        }
    }

    [Binding]
    public sealed class UserHidesTracesSteps
    {
        private readonly SceneForHideTraces _sut = new SceneForHideTraces();

        [Given(@"Входит пользователь (.*)")]
        public void GivenВходитПользователь(string user)
        {
            var vm = _sut.ClientScope.Resolve<LoginViewModel>();
            vm.UserName = user;
            vm.Password = user;
            vm.Login();
            _sut.ShellVm.GetStoredData().Wait();
            _sut.ReadModel.Users.Count.Should().Be(5);
        }

        [Then(@"Перезапускаем клиента")]
        public void ThenПерезапускаемКлиента()
        {
            _sut.RestartClient();
        }

        [Given(@"Рисует RTU1 с трассой1")]
        public void GivenРисуетRtu1СТрассой1()
        {
            _sut.CreateRtu1WithTrace1();
        }

        [Given(@"Рисует RTU2 с трассой2 у которой отрезок совпадает с отрезком трассы1")]
        public void GivenРисуетRtu2СТрассой2УКоторойОтрезокСовпадаетСОтрезкомТрассы1()
        {
            _sut.CreateRtu2WithTrace2PartlyOverlappingTrace1();
        }

        [Given(@"От RTU2 рисует отрезок до узла A")]
        public void GivenОтRtu2РисуетОтрезокДоУзлаA()
        {
            _sut.CreateFiberToRtu2();
        }

        [Given(@"Рисует отдельные узлы B и C и отрезок между ними")]
        public void GivenРисуетОтдельныеУзлыBиcиОтрезокМеждуНими()
        {
            _sut.CreateFiberBtoC();
        }

        [Then(@"В дереве видны оба RTU и обе трассы")]
        public void ThenВДеревеВидныОбаRtuиОбеТрассы()
        {
            _sut.TreeOfRtuModel.GetById(_sut.Rtu1.Id).Should().NotBeNull();
            _sut.TreeOfRtuModel.GetById(_sut.Rtu2.Id).Should().NotBeNull();
            _sut.TreeOfRtuModel.GetById(_sut.Trace1.TraceId).Should().NotBeNull();
            _sut.TreeOfRtuModel.GetById(_sut.Trace2.TraceId).Should().NotBeNull();
        }

        [Then(@"В карте видны оба RTU и обе трассы")]
        public void ThenВКартеВидныОбаRtuиОбеТрассы()
        {
            foreach (var trace in _sut.ReadModel.Traces)
            {
                foreach (var nodeId in trace.NodeIds)
                    _sut.GraphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == nodeId).Should().NotBeNull();

                foreach (var fiber in _sut.ReadModel.GetTraceFibers(trace))
                    _sut.GraphReadModel.Data.Fibers.FirstOrDefault(f => f.Id == fiber.FiberId).Should().NotBeNull();
            }
        }

        [Then(@"В карте видны оба RTU и трасса1 НО трасса2 от RTU2 не видна")]
        public void ThenВКартеВидныОбаRtuиТрасса1НоТрасса2ОтRtu2НеВидна()
        {
            {
                // RTU1 & Trace1
                foreach (var nodeId in _sut.Trace1.NodeIds)
                    _sut.GraphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == nodeId).Should().NotBeNull();

                foreach (var fiber in _sut.ReadModel.GetTraceFibers(_sut.Trace1))
                    _sut.GraphReadModel.Data.Fibers.FirstOrDefault(f => f.Id == fiber.FiberId).Should().NotBeNull();
            }

            {
                // RTU2
                _sut.GraphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == _sut.Rtu2.NodeId).Should().NotBeNull();
                // Trace2
                _sut.GraphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == _sut.EmptyNode2Id).Should().BeNull();
                var trace2Fibers = _sut.ReadModel.GetTraceFibers(_sut.Trace2).ToList();
                _sut.GraphReadModel.Data.Fibers.FirstOrDefault(f => f.Id == trace2Fibers[0].FiberId).Should().BeNull();
                _sut.GraphReadModel.Data.Fibers.FirstOrDefault(f => f.Id == trace2Fibers[1].FiberId).Should().BeNull();
            }
        }

        [Then(@"Не включенные в трассы элементы НЕ видны")]
        public void ThenНеВключенныеВТрассыЭлементыНеВидны()
        {
            _sut.GraphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == _sut.NodeAId).Should().BeNull();
            _sut.GraphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == _sut.NodeBId).Should().BeNull();
            _sut.GraphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == _sut.NodeCId).Should().BeNull();

            var fiberRtu2ToA = _sut.ReadModel.Fibers.First(f => f.NodeId1 == _sut.Rtu2.NodeId && f.NodeId2 == _sut.NodeAId);
            _sut.GraphReadModel.Data.Fibers.FirstOrDefault(f => f.Id == fiberRtu2ToA.FiberId).Should().BeNull();

            var fiberBtoC = _sut.ReadModel.Fibers.First(f => f.NodeId1 == _sut.NodeBId && f.NodeId2 == _sut.NodeCId);
            _sut.GraphReadModel.Data.Fibers.FirstOrDefault(f => f.Id == fiberBtoC.FiberId).Should().BeNull();
        }


        [When(@"Operator кликает на RTU2 пункт меню Скрыть трассы")]
        public void WhenOperatorКликаетНаRtu2ПунктМенюСкрытьТрассы()
        {
            _sut.GraphReadModel.GrmRtuRequests.SaveUsersHiddenRtus(_sut.Rtu2.NodeId).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }


    }
}

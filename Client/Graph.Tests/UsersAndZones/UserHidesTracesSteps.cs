using System.Linq;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WpfCommonViews;

using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class UserHidesTracesSteps
    {
        private readonly SceneForHideTraces _sut = new SceneForHideTraces();

        [Given(@"Входит пользователь (.*)")]
        public void GivenВходитПользователь(string user)
        {
            _sut.RestartClient();

            var vm = _sut.ClientScope.Resolve<LoginViewModel>();
            vm.UserName = user;
            vm.Password = user;
            vm.Login();
            _sut.FakeWindowManager.RegisterHandler(model => model is WaitViewModel);
            _sut.ShellVm.GetAlreadyStoredInCacheAndOnServerData().Wait();
            _sut.ReadModel.Users.Count.Should().Be(5);

            var vm1 = _sut.ClientScope.Resolve<ConfigurationViewModel>();
            vm1.IsGraphVisibleOnStart = true;
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

        [Then(@"В карте видны оба RTU и трасса2 A трасса1 от RTU1 не видна")]
        public void ThenВКартеВидныОбаRtuиТрасса2AТрасса1ОтRtu1НеВидна()
        {
            {
                // RTU2 & Trace2
                foreach (var nodeId in _sut.Trace2.NodeIds)
                    _sut.GraphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == nodeId).Should().NotBeNull();

                foreach (var fiber in _sut.ReadModel.GetTraceFibers(_sut.Trace2))
                    _sut.GraphReadModel.Data.Fibers.FirstOrDefault(f => f.Id == fiber.FiberId).Should().NotBeNull();
            }

            {
                // RTU1
                _sut.GraphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == _sut.Rtu1.NodeId).Should().NotBeNull();
                // Trace1
                _sut.GraphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == _sut.EmptyNode1Id).Should().BeNull();
                var trace1Fibers = _sut.ReadModel.GetTraceFibers(_sut.Trace1).ToList();
                _sut.GraphReadModel.Data.Fibers.FirstOrDefault(f => f.Id == trace1Fibers[0].FiberId).Should().BeNull();
                _sut.GraphReadModel.Data.Fibers.FirstOrDefault(f => f.Id == trace1Fibers[1].FiberId).Should().BeNull();
            }
        }

        [Then(@"Не включенные в трассы элементы НЕ видны")]
        public void ThenНеВключенныеВТрассыЭлементыНеВидны()
        {
            _sut.GraphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == _sut.NodeAId).Should().BeNull();
            _sut.GraphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == _sut.NodeBId).Should().BeNull();
            _sut.GraphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == _sut.NodeCId).Should().BeNull();

            var fiberRtu2ToA =
                _sut.ReadModel.Fibers.First(f => f.NodeId1 == _sut.Rtu2.NodeId && f.NodeId2 == _sut.NodeAId);
            _sut.GraphReadModel.Data.Fibers.FirstOrDefault(f => f.Id == fiberRtu2ToA.FiberId).Should().BeNull();

            var fiberBtoC = _sut.ReadModel.Fibers.First(f => f.NodeId1 == _sut.NodeBId && f.NodeId2 == _sut.NodeCId);
            _sut.GraphReadModel.Data.Fibers.FirstOrDefault(f => f.Id == fiberBtoC.FiberId).Should().BeNull();
        }

        [Then(@"Не включенные в трассы элементы ВИДНЫ")]
        public void ThenНеВключенныеВТрассыЭлементыВидны()
        {
            _sut.GraphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == _sut.NodeAId).Should().NotBeNull();
            _sut.GraphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == _sut.NodeBId).Should().NotBeNull();
            _sut.GraphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == _sut.NodeCId).Should().NotBeNull();

            var fiberRtu2ToA =
                _sut.ReadModel.Fibers.First(f => f.NodeId1 == _sut.Rtu2.NodeId && f.NodeId2 == _sut.NodeAId);
            _sut.GraphReadModel.Data.Fibers.FirstOrDefault(f => f.Id == fiberRtu2ToA.FiberId).Should().NotBeNull();

            var fiberBtoC = _sut.ReadModel.Fibers.First(f => f.NodeId1 == _sut.NodeBId && f.NodeId2 == _sut.NodeCId);
            _sut.GraphReadModel.Data.Fibers.FirstOrDefault(f => f.Id == fiberBtoC.FiberId).Should().NotBeNull();
        }

        [When(@"Кликает на карте на иконке RTU(.*) пункт меню Скрыть трассы")]
        public void WhenКликаетНаКартеНаИконкеRtuПунктМенюСкрытьТрассы(int p0)
        {
            var rtuNodeId = p0 == 1 ? _sut.Rtu1.NodeId : _sut.Rtu2.NodeId;
            var rtu = _sut.ReadModel.Rtus.First(r => r.NodeId == rtuNodeId);
            _sut.CurrentlyHiddenRtu.Collection.Add(rtu.Id);
            _sut.FakeWindowManager.RegisterHandler(model => model is WaitViewModel);

            var renderingManager = _sut.ClientScope.Resolve<RenderingManager>();
            int unused = renderingManager.RenderOnRtuChanged().Result;
        }
    }
}
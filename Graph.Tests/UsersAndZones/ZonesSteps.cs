using System;
using System.Linq;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;
using TechTalk.SpecFlow;

namespace Graph.Tests.UsersAndZones
{
    [Binding]
    public sealed class ZonesSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Iit.Fibertest.Graph.Rtu _defaultZoneRtu;
        private Iit.Fibertest.Graph.Rtu _rtuZone1;
        private Guid _opZone1UserId;
        private Iit.Fibertest.Graph.Trace _traceForZone1;
        private Iit.Fibertest.Graph.Trace _traceNotForZone1;


        [Given(@"Вход как рут")]
        public void GivenВходКакРут()
        {
            var vm = _sut.ClientContainer.Resolve<LoginViewModel>();
            vm.UserName = @"root";
            vm.Password = @"root";
            vm.Login();
            _sut.ShellVm.InitializeModels().Wait();
            _sut.ReadModel.Users.Count.Should().Be(5);
        }

        [Given(@"Настройка доп зоны и оператора этой доп зоны")]
        public void GivenНастройкаДопЗоныИОператораЭтойДопЗоны()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ZoneHandler(model, @"Zone 1", Answer.Yes));
            var vm = _sut.ClientContainer.Resolve<ZonesViewModel>();
            vm.AddZone();
            _sut.Poller.EventSourcingTick().Wait();

            var zoneId = _sut.ReadModel.Zones.Last().ZoneId;

            _sut.FakeWindowManager.RegisterHandler(model => _sut.UserHandler(model, @"OpZone1", @"123", zoneId, Answer.Yes));
            var vm2 = _sut.ClientContainer.Resolve<UserListViewModel>();
            vm2.AddNewUser();
            _sut.Poller.EventSourcingTick().Wait();
            _opZone1UserId = _sut.ReadModel.Users.Last().UserId;
        }

        [Given(@"Рисование")]
        public void GivenРисование()
        {
            _defaultZoneRtu = _sut.SetInitializedRtu();
//            var traceInDefaultZone = _sut.SetTrace(_defaultZoneRtu.NodeId, @"Trace in default zone");

            _rtuZone1 = _sut.SetInitializedRtuForZone1();
            _traceForZone1 = _sut.SetTraceForZone(_rtuZone1.NodeId, @"Trace will be IN zone 1");
            _traceNotForZone1 = _sut.SetTraceForZone(_rtuZone1.NodeId, @"Trace will NOT be in zone 1");
        }

        [Given(@"В доп зону включаем только одна из трасс второго RTU")]
        public void GivenВДопЗонуВключаемТолькоОднаИзТрассВторогоRtu()
        {

        }

        [Then(@"Рут выходит из приложения")]
        public void ThenРутВыходитИзПриложения()
        {
            _sut.ReadModel = new Model();
            _sut.TreeOfRtuModel = new TreeOfRtuModel();
//            _sut.GraphReadModel = new GraphReadModel();
        }


        [When(@"Вход как оператор доп зоны")]
        public void WhenВходКакОператорДопЗоны()
        {
            var vm = _sut.ClientContainer.Resolve<LoginViewModel>();
            vm.UserName = @"OpZone1";
            vm.Password = @"123";
            vm.Login();
            _sut.Poller.EventSourcingTick().Wait();

            _sut.ClientContainer.Resolve<CurrentUser>().UserId.Should().Be(_opZone1UserId);
        }

        [Then(@"Видна только часть графа входящая в зону")]
        public void ThenВиднаТолькоЧастьГрафаВходящаяВЗону()
        {
//            _sut.GraphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == _defaultZoneRtu.NodeId).Should().BeNull();

//            _sut.TreeOfRtuModel.Tree.Count.Should().Be(1);
        }

    }
}

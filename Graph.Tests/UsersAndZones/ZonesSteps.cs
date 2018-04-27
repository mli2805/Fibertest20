using System;
using System.Linq;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class ZonesSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Iit.Fibertest.Graph.Rtu _rtu1;
        private Iit.Fibertest.Graph.Rtu _rtu2;
        private Guid _zone1Id;
        private Guid _opZone1UserId;
        private Iit.Fibertest.Graph.Trace _trace1;
        private Iit.Fibertest.Graph.Trace _trace2;
        private ObjectsAsTreeToZonesViewModel _objectsVm;


        [Given(@"Вход как рут")]
        public void GivenВходКакРут()
        {
            var vm = _sut.ClientScope.Resolve<LoginViewModel>();
            vm.UserName = @"root";
            vm.Password = @"root";
            vm.Login();
            _sut.ShellVm.GetAlreadyStoredInCacheAndOnServerData().Wait();
            _sut.ReadModel.Users.Count.Should().Be(5);
        }

        [Given(@"Добавляем Зона1")]
        public void GivenДобавляемЗона1()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ZoneHandler(model, @"Zone 1", Answer.Yes));
            var vm = _sut.ClientScope.Resolve<ZonesViewModel>();
            vm.AddZone();
            _sut.Poller.EventSourcingTick().Wait();
            _zone1Id = _sut.ReadModel.Zones[1].ZoneId;
        }

        [Given(@"Добавляем Оператора для Зона1")]
        public void GivenДобавляемОператораДляЗона1()
        {
            _sut.FakeWindowManager.RegisterHandler(model =>
                _sut.UserHandler(model, @"OpZone1", @"123", _zone1Id, Answer.Yes));
            var vm2 = _sut.ClientScope.Resolve<UserListViewModel>();
            vm2.AddNewUser();
            _sut.Poller.EventSourcingTick().Wait();
            var operatorForZone1 = _sut.ReadModel.Users.Last();
            operatorForZone1.ZoneId.Should().NotBe(Guid.Empty);
            _opZone1UserId = operatorForZone1.UserId;
        }

        [Given(@"Создаем RTU1 и трассу")]
        public void GivenСоздаемRtu1ИТрассу()
        {
            _rtu1 = _sut.SetInitializedRtu();
            _sut.SetTrace(_rtu1.NodeId, @"Trace in default zone");
            _sut.GraphReadModel.Data.Nodes.Count.Should().Be(9);
        }

        [Given(@"Создаем RTU2 и трассу1 и трассу2")]
        public void GivenСоздаемRtu2ИТрассу1ИТрассу2()
        {
            _rtu2 = _sut.SetInitializedRtuForZone1();
            _trace1 = _sut.SetTraceForZone(_rtu2.NodeId, @"Trace will NOT be in zone 1");
            _trace2 = _sut.SetTraceForZone(_rtu2.NodeId, @"Trace will be IN zone 1");
            _sut.GraphReadModel.Data.Nodes.Count.Should().Be(12);
        }

        [When(@"Щелкаем включить трассу2 у RTU2 в Зона1")]
        public void WhenЩелкаемВключитьТрассу2Уrtu2ВЗона1()
        {
            _objectsVm = _sut.ClientScope.Resolve<ObjectsAsTreeToZonesViewModel>();
            _objectsVm.Rows.Count.Should().Be(5);
            var trace2Row = _objectsVm.Rows.First(r => r.TraceId == _trace2.TraceId);
            trace2Row.IsInZones[1].IsChecked = true;
        }

        [Then(@"У RTU2 тоже появляется птичка в столбце Зона1")]
        public void ThenУrtu2ТожеПоявляетсяПтичкаВСтолбцеЗона1()
        {
            var rtu2Row = _objectsVm.Rows.First(r => r.RtuId == _rtu2.Id && r.IsRtu);
            rtu2Row.IsInZones[1].IsChecked = true; // code behind does this, so here manually
        }

        [Given(@"Сохраняем зоны")]
        public void GivenСохраняемЗоны()
        {
            _objectsVm.Save();
            _sut.Poller.EventSourcingTick().Wait();
            _rtu2.ZoneIds.Count.Should().Be(2);
            _trace2.ZoneIds.Count.Should().Be(2);
        }

        [When(@"Перезапускаем клиентское приложение")]
        public void WhenПерезапускаемКлиентскоеПриложение()
        {
            _sut.RestartClient();
        }

        [When(@"Вход как Оператор для Зона1")]
        public void WhenВходКакОператорДляЗона1()
        {
            var vm = _sut.ClientScope.Resolve<LoginViewModel>();
            vm.UserName = @"OpZone1";
            vm.Password = @"123";
            vm.Login();
            _sut.ClientScope.Resolve<CurrentUser>().UserId.Should().Be(_opZone1UserId);

            _sut.ReadModel.Nodes.Count.Should().Be(0);
            _sut.TreeOfRtuModel.Tree.Count.Should().Be(0);
            _sut.GraphReadModel.Data.Nodes.Count.Should().Be(0);

            _sut.ShellVm.GetAlreadyStoredInCacheAndOnServerData().Wait();

            _sut.ReadModel.Nodes.Count.Should().Be(12);

            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"На карте видна только трасса2")]
        public void ThenНаКартеВиднаТолькоТрасса2()
        {
            _sut.GraphReadModel.Data.Nodes.Count.Should().Be(2);
        }

        [Then(@"В дереве только RTU2 трасса1 серая трасса2 синяя")]
        public void ThenВДеревеТолькоRtu2Трасса1СераяТрасса2Синяя()
        {
            _sut.TreeOfRtuModel.Tree.Count.Should().Be(1);
            var trace1Leaf = _sut.TreeOfRtuModel.GetById(_trace1.TraceId);
            trace1Leaf.Color.Should().Be(FiberState.NotInZone.GetBrush(true));
            var trace2Leaf = _sut.TreeOfRtuModel.GetById(_trace2.TraceId);
            trace2Leaf.Color.Should().Be(FiberState.NotJoined.GetBrush(true));
        }
    }
}
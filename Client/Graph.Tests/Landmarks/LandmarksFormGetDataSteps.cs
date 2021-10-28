using System.Linq;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class LandmaksFormGetDataSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginOnEmptyBaseAsRoot();
        private Iit.Fibertest.Graph.Trace _trace;
        private LandmarksViewModel _vm;

        [Given(@"Задана трасса с поинтами")]
        public void GivenЗаданаТрассаСПоинтами()
        {
            _trace = _sut.SetTraceForLandmarks();
        }

        [When(@"Пользователь открывает форму ориентиров")]
        public void WhenПользовательОткрываетФормуОриентиров()
        {
            _vm = _sut.ClientScope.Resolve<LandmarksViewModel>();
            _vm.InitializeFromTrace(_trace.TraceId, _trace.NodeIds[0]).Wait();
            _vm.SelectedGpsInputMode = _vm.GpsInputModes[0];
        }

        [Then(@"Проверяем вьюмодель")]
        public void ThenПроверяемВьюмодель()
        {
            _vm.Rows.Count.Should().Be(7);
            _vm.Rows[0].EquipmentType.Should().Be(@"RTU");
            _vm.Rows[0].NodeTitle.Should().Be(@"Some title for RTU");
            _vm.Rows[1].EquipmentType.Should().Be(Resources.SID_Closure);
            _vm.Rows[2].EquipmentType.Should().Be(Resources.SID_Other);
            _vm.Rows[3].EquipmentType.Should().Be(Resources.SID_CableReserve);
            _vm.Rows[4].EquipmentType.Should().Be(Resources.SID_Cross);
            _vm.Rows[4].GpsCoors.Should().StartWith(@"55.1220");
            _vm.Rows[5].EquipmentType.Should().Be(Resources.SID_Node);
            _vm.Rows[6].EquipmentType.Should().Be(Resources.SID_Terminal);
            _vm.Rows[6].Distance.Should().Be(@" 17.431");
        }

        [When(@"Задается базовая")]
        public void WhenЗадаетсяБазовая()
        {
            var traceLeaf = (TraceLeaf)_sut.TreeOfRtuModel.GetById(_trace.TraceId);
            _sut.AssignBaseRef(traceLeaf, SystemUnderTest.BasTrace7, SystemUnderTest.BasTrace7, null, Answer.Yes);
        }

        [Then(@"Проверяем ориентиры из базовой")]
        public void ThenПроверяемОриентирыИзБазовой()
        {
            _vm.Rows.Count.Should().Be(7);
            _vm.Rows[0].EquipmentType.Should().Be(@"RTU");

            _vm.Rows[1].EquipmentType.Should().Be(Resources.SID_Closure);

            _vm.Rows[2].EquipmentType.Should().Be(Resources.SID_Other);
            //_vm.Rows[2].Distance.Should().Be(@" 9.242");
            _vm.Rows[2].Distance.Should().Be(@" 8.758");

            _vm.Rows[3].EquipmentType.Should().Be(Resources.SID_CableReserve);
            //_vm.Rows[3].Distance.Should().Be(@" 9.696");
            _vm.Rows[3].Distance.Should().Be(@" 9.454");

            _vm.Rows[4].EquipmentType.Should().Be(Resources.SID_Cross);
            _vm.Rows[4].NodeTitle.Should().Be("");
            _vm.Rows[4].Distance.Should().Be(@" 10.150");

            _vm.Rows[5].EquipmentType.Should().Be(Resources.SID_Node);
            _vm.Rows[5].Distance.Should().Be(@" 11.153");

            _vm.Rows[6].EquipmentType.Should().Be(Resources.SID_Terminal);
            _vm.Rows[6].Distance.Should().Be(@" 12.157");
        }

        [When(@"При открытой форме ориентиров на карте двигаем узел с проключением и на форме узла меняем ему название")]
        public void WhenПриОткрытойФормеОриентировНаКартеДвигаемУзелСПроключениемИНаФормеУзлаМеняемЕмуНазвание()
        {
            var nodeId = _sut.ReadModel.Nodes.First(n => n.TypeOfLastAddedEquipment == EquipmentType.Cross).NodeId;

            _sut.GraphReadModel.GrmNodeRequests
                .MoveNode(new MoveNode() { NodeId = nodeId, Latitude = 55.2, Longitude = 30.2 }).Wait();
            _sut.Poller.EventSourcingTick().Wait();

            var nodeUpdateViewModel = _sut.ClientScope.Resolve<NodeUpdateViewModel>();
            nodeUpdateViewModel.Initialize(nodeId);
            nodeUpdateViewModel.Title = @"Node 4";
            nodeUpdateViewModel.Save();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"После обновления формы ориентиров имя узла и его координаты меняются")]
        public void ThenПослеОбновленияФормыОриентировИмяУзлаИЕгоКоординатыМеняются()
        {
            _vm.RefreshOrChangeTrace().Wait();

            _vm.Rows[4].NodeTitle.Should().Be(@"Node 4");
            _vm.Rows[4].GpsCoors.Should().StartWith(@"55.2000");
        }

        [Then(@"Расстояние до ориентира не меняется т к он привязан к событию")]
        public void ThenРасстояниеДоОриентираНеМеняетсяТкОнПривязанКСобытию()
        {
            _vm.Rows[4].Distance.Should().Be(@" 10.150");
        }

        [Then(@"Растояние до соседних ориентиров меняется т к они не привязаны")]
        public void ThenРастояниеДоСоседнихОриентировМеняетсяТкОниНеПривязаны()
        {
            _vm.Rows[3].Distance.Should().NotBe(@" 9.454");
            _vm.Rows[5].Distance.Should().NotBe(@" 11.153");
        }

        [When(@"Изменяет название первого узла")]
        public void WhenИзменяетНазваниеПервогоУзла()
        {
            _vm.SelectedRow = _vm.Rows[1];
            _vm.OneLandmarkViewModel.SelectedLandmark.NodeTitle = @"some changes";
        }

        [When(@"Щелкает на любой другой строчке а затем назад на первом узле")]
        public void WhenЩелкаетНаЛюбойДругойСтрочкеАЗатемНазадНаПервомУзле()
        {
            _vm.SelectedRow = _vm.Rows[0];
            _vm.SelectedRow = _vm.Rows[1];

        }

        [Then(@"Название первого узла должно быть первоначальным")]
        public void ThenНазваниеПервогоУзлаДолжноБытьПервоначальным()
        {
            _vm.OneLandmarkViewModel.SelectedLandmark.NodeTitle.Should().Be(null);
        }

        [When(@"Изменяет название и тип оборудования в первом узле")]
        public void WhenИзменяетНазваниеИТипОборудованияВПервомУзле()
        {
            _vm.SelectedRow = _vm.Rows[1];
            _vm.OneLandmarkViewModel.SelectedLandmark.EquipmentTitle = @"New title for equipment in node 1";
            _vm.OneLandmarkViewModel.SelectedLandmark.EquipmentType = EquipmentType.Other;

        }

        [When(@"Изменяет название и координаты первого узла и жмет Применить")]
        public void WhenИзменяетНазваниеИКоординатыПервогоУзлаИЖметПрименить()
        {
            _vm.OneLandmarkViewModel.SelectedLandmark.NodeTitle = @"New title for node 1";
            _vm.OneLandmarkViewModel.GpsInputSmallViewModel.OneCoorViewModelLatitude.Degrees = @"44";
            _vm.OneLandmarkViewModel.GpsInputSmallViewModel.OneCoorViewModelLatitude.Minutes = @"0";
            _vm.OneLandmarkViewModel.GpsInputSmallViewModel.OneCoorViewModelLatitude.Seconds = @"12";
            _vm.OneLandmarkViewModel.GpsInputSmallViewModel.OneCoorViewModelLongitude.Degrees = @"12";
            _vm.OneLandmarkViewModel.GpsInputSmallViewModel.OneCoorViewModelLongitude.Minutes = @"34";
            _vm.OneLandmarkViewModel.GpsInputSmallViewModel.OneCoorViewModelLongitude.Seconds = @"56";

            _vm.OneLandmarkViewModel.Apply();
            _sut.Poller.EventSourcingTick().Wait();
            _vm.RefreshAsChangesReaction(); //TODO refactor tests to use LandmarkViewsManager
        }

        [Then(@"Меняются поля в строке на форме ориентиров")]
        public void ThenМеняютсяПоляВСтрокеНаФормеОриентиров()
        {
            _vm.Rows[1].NodeTitle.Should().Be(@"New title for node 1");
            _vm.Rows[1].EquipmentTitle.Should().Be(@"New title for equipment in node 1");
            _vm.Rows[1].EquipmentType.Should().Be(EquipmentType.Other.ToLocalizedString());
            _vm.Rows[1].GpsCoors.Should().StartWith(@"44");
        }

        [Then(@"Меняется положение узла на карте")]
        public void ThenМеняетсяПоложениеУзлаНаКарте()
        {
            var nodeVm = _sut.GraphReadModel.Data.Nodes.First(n => n.Id == _vm.Rows[1].NodeId);
            nodeVm.Position.ToDetailedString(_vm.SelectedGpsInputMode.Mode).Should().StartWith(@"44");
        }

    }
}
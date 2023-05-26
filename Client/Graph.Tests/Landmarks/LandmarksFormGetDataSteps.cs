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
            _vm.RowsLandmarkViewModel.Rows.Count.Should().Be(7);
            _vm.RowsLandmarkViewModel.Rows[0].EquipmentType.Should().Be(@"RTU");
            _vm.RowsLandmarkViewModel.Rows[0].NodeTitle.Should().Be(@"Some title for RTU");
            _vm.RowsLandmarkViewModel.Rows[1].EquipmentType.Should().Be(Resources.SID_Closure);
            _vm.RowsLandmarkViewModel.Rows[2].EquipmentType.Should().Be(Resources.SID_Other);
            _vm.RowsLandmarkViewModel.Rows[3].EquipmentType.Should().Be(Resources.SID_CableReserve);
            _vm.RowsLandmarkViewModel.Rows[4].EquipmentType.Should().Be(Resources.SID_Cross);
            _vm.RowsLandmarkViewModel.Rows[4].GpsCoors.Should().StartWith(@"55.1220");
            _vm.RowsLandmarkViewModel.Rows[5].EquipmentType.Should().Be(Resources.SID_Node);
            _vm.RowsLandmarkViewModel.Rows[6].EquipmentType.Should().Be(Resources.SID_Terminal);
            _vm.RowsLandmarkViewModel.Rows[6].GpsDistance.Should().StartWith(@" 14.996");
            _vm.RowsLandmarkViewModel.Rows[6].OpticalDistance.Should().Be(@"");
        }

        [When(@"Задается базовая")]
        public void WhenЗадаетсяБазовая()
        {
            var traceLeaf = (TraceLeaf)_sut.TreeOfRtuModel.GetById(_trace.TraceId);
            _sut.AssignBaseRef(traceLeaf, SystemUnderTest.BasTrace7, SystemUnderTest.BasTrace7, null, Answer.Yes);
        }

        [Then(@"Оптические расстояния приивязанных ориентиры совпадают с расстояниями в сорке а у непривязанных нет")]
        public void ThenОптическиеРасстоянияПриивязанныхОриентирыСовпадаютСРасстояниямиВСоркеАуНепривязанныхНет()
        {
            _vm.RowsLandmarkViewModel.Rows.Count.Should().Be(7);
            _vm.RowsLandmarkViewModel.Rows[0].EquipmentType.Should().Be(@"RTU");
            _vm.RowsLandmarkViewModel.Rows[0].OpticalDistance.Should().Be(@" 0.000");

            // differs with sor (not related to key event)
            _vm.RowsLandmarkViewModel.Rows[1].EquipmentType.Should().Be(Resources.SID_Closure); 
            _vm.RowsLandmarkViewModel.Rows[1].OpticalDistance.Should().NotBe(@" 2.484");

            // differs with sor (not related to key event)
            _vm.RowsLandmarkViewModel.Rows[2].EquipmentType.Should().Be(Resources.SID_Other);
            _vm.RowsLandmarkViewModel.Rows[2].OpticalDistance.Should().NotBe(@" 5.238");

            // differs with sor (not related to key event)
            _vm.RowsLandmarkViewModel.Rows[3].EquipmentType.Should().Be(Resources.SID_CableReserve);
            _vm.RowsLandmarkViewModel.Rows[3].OpticalDistance.Should().NotBe(@" 7.129");

            _vm.RowsLandmarkViewModel.Rows[4].EquipmentType.Should().Be(Resources.SID_Cross); 
            _vm.RowsLandmarkViewModel.Rows[4].NodeTitle.Should().BeNull();
            _vm.RowsLandmarkViewModel.Rows[4].OpticalDistance.Should().Be(@" 10.150");

            // differs with sor (not related to key event)
            _vm.RowsLandmarkViewModel.Rows[5].EquipmentType.Should().Be(Resources.SID_Node);
            _vm.RowsLandmarkViewModel.Rows[5].OpticalDistance.Should().NotBe(@" 10.932");

            _vm.RowsLandmarkViewModel.Rows[6].EquipmentType.Should().Be(Resources.SID_Terminal);
            _vm.RowsLandmarkViewModel.Rows[6].OpticalDistance.Should().Be(@" 12.157");
        }

        [When(@"При открытой форме ориентиров на карте двигаем узел с проключением и на форме узла меняем ему название")]
        public void WhenПриОткрытойФормеОриентировНаКартеДвигаемУзелСПроключениемИНаФормеУзлаМеняемЕмуНазвание()
        {
            var nodeId = _sut.ReadModel.Nodes.First(n => n.TypeOfLastAddedEquipment == EquipmentType.Cross).NodeId;

            _sut.GraphReadModel.GrmNodeRequests
                .MoveNode(new MoveNode() { NodeId = nodeId, Latitude = 55.2, Longitude = 30.2 }).Wait();
            _sut.Poller.EventSourcingTick().Wait();

            // var nodeUpdateViewModel = _sut.ClientScope.Resolve<NodeUpdateViewModel>();
            // nodeUpdateViewModel.Initialize(nodeId);
            // nodeUpdateViewModel.Title = @"Node 4";
            // nodeUpdateViewModel.Save();
            // _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"На форме ориентиров меняем название узла")]
        public void WhenНаФормеОриентировМеняемНазваниеУзла()
        {
            _vm.RowsLandmarkViewModel.SelectedRow = _vm.RowsLandmarkViewModel.Rows.Skip(4).First();
            _vm.RowsLandmarkViewModel.OneLandmarkViewModel.LandmarkUnderWork.NodeTitle = @"Node 4";
            _vm.RowsLandmarkViewModel.UpdateTable();
        }


        [Then(@"После обновления формы ориентиров имя узла и его координаты меняются")]
        public void ThenПослеОбновленияФормыОриентировИмяУзлаИЕгоКоординатыМеняются()
        {
            _vm.RowsLandmarkViewModel.UpdateTable();
            _sut.FakeWindowManager.RegisterHandler(_sut.LandmarksCorrectionProgressHandler);
            _vm.SaveAllChanges().Wait();

            _vm.RowsLandmarkViewModel.Rows[4].NodeTitle.Should().Be(@"Node 4");
            _vm.RowsLandmarkViewModel.Rows[4].GpsCoors.Should().StartWith(@"55.2");
        }

        [Then(@"Расстояние до ориентира не меняется т к он привязан к событию")]
        public void ThenРасстояниеДоОриентираНеМеняетсяТкОнПривязанКСобытию()
        {
            _vm.RowsLandmarkViewModel.Rows[4].OpticalDistance.Should().Be(@" 10.150");
        }

        [Then(@"Растояние до соседних ориентиров меняется т к они не привязаны")]
        public void ThenРастояниеДоСоседнихОриентировМеняетсяТкОниНеПривязаны()
        {
            _vm.RowsLandmarkViewModel.Rows[3].OpticalDistance.Should().NotBe(@" 9.454");
            _vm.RowsLandmarkViewModel.Rows[5].OpticalDistance.Should().NotBe(@" 11.153");
        }

        [When(@"Изменяет название первого узла")]
        public void WhenИзменяетНазваниеПервогоУзла()
        {
            _vm.RowsLandmarkViewModel.SelectedRow = _vm.RowsLandmarkViewModel.Rows[1];
            _vm.RowsLandmarkViewModel.OneLandmarkViewModel.LandmarkUnderWork.NodeTitle = @"some changes";
        }

        [When(@"Щелкает на любой другой строчке а затем назад на первом узле")]
        public void WhenЩелкаетНаЛюбойДругойСтрочкеАЗатемНазадНаПервомУзле()
        {
            _vm.RowsLandmarkViewModel.SelectedRow = _vm.RowsLandmarkViewModel.Rows[0];
            _vm.RowsLandmarkViewModel.SelectedRow = _vm.RowsLandmarkViewModel.Rows[1];

        }

        [Then(@"Название первого узла должно быть первоначальным")]
        public void ThenНазваниеПервогоУзлаДолжноБытьПервоначальным()
        {
            _vm.RowsLandmarkViewModel.OneLandmarkViewModel.LandmarkUnderWork.NodeTitle.Should().Be(null);
        }

        [When(@"Изменяет название и тип оборудования в первом узле")]
        public void WhenИзменяетНазваниеИТипОборудованияВПервомУзле()
        {
            _vm.RowsLandmarkViewModel.SelectedRow = _vm.RowsLandmarkViewModel.Rows[1];
            _vm.RowsLandmarkViewModel.OneLandmarkViewModel.LandmarkUnderWork.EquipmentTitle = @"New title for equipment in node 1";
            _vm.RowsLandmarkViewModel.OneLandmarkViewModel.LandmarkUnderWork.EquipmentType = EquipmentType.Other;

        }

        [When(@"Изменяет название и координаты первого узла и жмет Применить")]
        public void WhenИзменяетНазваниеИКоординатыПервогоУзлаИЖметПрименить()
        {
            _vm.CurrentGis.GpsInputMode = GpsInputMode.DegreesMinutesAndSeconds;
            _vm.RowsLandmarkViewModel.OneLandmarkViewModel.LandmarkUnderWork.NodeTitle = @"New title for node 1";
            _vm.RowsLandmarkViewModel.OneLandmarkViewModel.GpsInputSmallViewModel.OneCoorViewModelLatitude.Degrees = @"44";
            _vm.RowsLandmarkViewModel.OneLandmarkViewModel.GpsInputSmallViewModel.OneCoorViewModelLatitude.Minutes = @"0";
            _vm.RowsLandmarkViewModel.OneLandmarkViewModel.GpsInputSmallViewModel.OneCoorViewModelLatitude.Seconds = @"12";
            _vm.RowsLandmarkViewModel.OneLandmarkViewModel.GpsInputSmallViewModel.OneCoorViewModelLongitude.Degrees = @"12";
            _vm.RowsLandmarkViewModel.OneLandmarkViewModel.GpsInputSmallViewModel.OneCoorViewModelLongitude.Minutes = @"34";
            _vm.RowsLandmarkViewModel.OneLandmarkViewModel.GpsInputSmallViewModel.OneCoorViewModelLongitude.Seconds = @"56";

            _vm.RowsLandmarkViewModel.UpdateTable();
            _sut.FakeWindowManager.RegisterHandler(_sut.LandmarksCorrectionProgressHandler);
            _vm.SaveAllChanges().Wait();
            // _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Меняются поля в строке на форме ориентиров")]
        public void ThenМеняютсяПоляВСтрокеНаФормеОриентиров()
        {
            _vm.RowsLandmarkViewModel.Rows[1].NodeTitle.Should().Be(@"New title for node 1");
            _vm.RowsLandmarkViewModel.Rows[1].EquipmentTitle.Should().Be(@"New title for equipment in node 1");
            _vm.RowsLandmarkViewModel.Rows[1].EquipmentType.Should().Be(EquipmentType.Other.ToLocalizedString());
            _vm.RowsLandmarkViewModel.Rows[1].GpsCoors.Should().StartWith(@"44");
        }
      
    }
}
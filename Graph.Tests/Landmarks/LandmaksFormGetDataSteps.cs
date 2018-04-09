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
        private readonly SystemUnderTest _sut = new SystemUnderTest();
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
            _vm = _sut.ClientContainer.Resolve<LandmarksViewModel>();
            _vm.Initialize(_trace.TraceId, false).Wait();
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
//            _vm.Rows[5].EquipmentType.Should().Be(Resources.SID_Node_without_equipment);
            _vm.Rows[5].EquipmentType.Should().Be(Resources.SID_Node);
            _vm.Rows[6].EquipmentType.Should().Be(Resources.SID_Terminal);
            _vm.Rows[6].Distance.Should().Be(@" 17.431");
        }

        [When(@"Задается базовая")]
        public void WhenЗадаетсяБазовая()
        {
            var traceLeaf = (TraceLeaf) _sut.TreeOfRtuModel.GetById(_trace.TraceId);
            _sut.AssignBaseRef(traceLeaf, SystemUnderTest.BaseTrace7, SystemUnderTest.BaseTrace7, null, Answer.Yes);
        }

        [Then(@"Проверяем ориентиры из базовой")]
        public void ThenПроверяемОриентирыИзБазовой()
        {
            _vm.Rows.Count.Should().Be(7);
            _vm.Rows[0].EquipmentType.Should().Be(@"RTU");

            _vm.Rows[1].EquipmentType.Should().Be(Resources.SID_Closure);

            _vm.Rows[2].EquipmentType.Should().Be(Resources.SID_Other);
            _vm.Rows[2].Distance.Should().Be(@" 8.758");

            _vm.Rows[3].EquipmentType.Should().Be(Resources.SID_CableReserve);
            _vm.Rows[3].Distance.Should().Be(@" 9.454");

            _vm.Rows[4].EquipmentType.Should().Be(Resources.SID_Cross);
            _vm.Rows[4].NodeTitle.Should().Be("");
            _vm.Rows[4].Distance.Should().Be(@" 10.150");

//            _vm.Rows[5].EquipmentType.Should().Be(Resources.SID_Node_without_equipment);
            _vm.Rows[5].EquipmentType.Should().Be(Resources.SID_Node);
            _vm.Rows[5].Distance.Should().Be(@" 11.153");

            _vm.Rows[6].EquipmentType.Should().Be(Resources.SID_Terminal);
            _vm.Rows[6].Distance.Should().Be(@" 12.157");

            _vm.TryClose();
        }

        [When(@"Двигаем узел с проключением и меняем ему название")]
        public void WhenДвигаемУзелСПроключениемИМеняемЕмуНазвание()
        {
            var nodeId = _sut.ReadModel.Nodes.First(n => n.TypeOfLastAddedEquipment == EquipmentType.Cross).NodeId;

            _sut.GraphReadModel.GrmNodeRequests
                .MoveNode(new MoveNode() {NodeId = nodeId, Latitude = 55.2, Longitude = 30.2}).Wait();
            _sut.Poller.EventSourcingTick().Wait();

            var nodeUpdateViewModel = _sut.ClientContainer.Resolve<NodeUpdateViewModel>();
            nodeUpdateViewModel.Initialize(nodeId);
            nodeUpdateViewModel.Title = @"Node 4";
            nodeUpdateViewModel.Save();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"На форме ориентиров имя узла меняется")]
        public void ThenНаФормеОриентировИмяУзлаМеняется()
        {
            _vm = _sut.ClientContainer.Resolve<LandmarksViewModel>();
            _vm.Initialize(_trace.TraceId, false).Wait();

            _vm.Rows[4].NodeTitle.Should().Be(@"Node 4");
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

    }
}
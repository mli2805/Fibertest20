using System;
using System.Linq;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class EquipmentRemovedWithLoopSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginAsRoot(Answer.Yes);
        private Guid _nodeAId, _equipmentA1Id;
        private Iit.Fibertest.Graph.Trace _trace;
        private NodeUpdateViewModel _vm;
        private LandmarksViewModel _lvm;


        [Given(@"Трасса использует муфту А1 дважды")]
        public void GivenТрассаИспользуетМуфтуА1Дважды()
        {
            _trace = _sut.CreateTraceDoublePassingClosure();
            _nodeAId = _trace.NodeIds[2];
            _equipmentA1Id = _trace.EquipmentIds[2];

            _trace.NodeIds.Count.Should().Be(8);
        }

        [Given(@"Открыта форма для редактирования узла с муфтой А1")]
        public void GivenОткрытаФормаДляРедактированияУзлаСМуфтойА1()
        {
            _vm = _sut.ClientScope.Resolve<NodeUpdateViewModel>();
            _vm.Initialize(_nodeAId);
        }

        [When(@"Пользователь удаляет оборудование")]
        public void WhenПользовательУдаляетОборудование()
        {
            _vm.EquipmentsInNode.First(it => it.Id == _equipmentA1Id).Command = new RemoveEquipment() { EquipmentId = _equipmentA1Id };
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Оборудование удаляется из обоих мест в трассе и в целом из графа")]
        public void ThenОборудованиеУдаляетсяИзОбоихМестВТрассеИвЦеломИзГрафа()
        {
            _sut.ReadModel.Traces.Where(t => t.EquipmentIds.Contains(_equipmentA1Id)).Should().BeEmpty();
            _trace.EquipmentIds.Contains(_equipmentA1Id).ShouldBeEquivalentTo(false);
            _sut.ReadModel.Equipments.Count(e => e.NodeId == _nodeAId).ShouldBeEquivalentTo(1);
            _trace.EquipmentIds.Count.ShouldBeEquivalentTo(_trace.NodeIds.Count);
            _sut.ReadModel.Equipments.First(e => e.NodeId == _nodeAId).EquipmentId.ShouldBeEquivalentTo(_trace.EquipmentIds[2]);
            _trace.EquipmentIds.Contains(Guid.Empty).Should().BeFalse();
        }

        [Given(@"Открыта форма ориентиров")]
        public void GivenОткрытаФормаОриентиров()
        {
            _lvm = _sut.ClientScope.Resolve<LandmarksViewModel>();
            _lvm.InitializeFromTrace(_trace.TraceId, _trace.NodeIds[0]).Wait();
            _lvm.Rows.Count.Should().Be(7); // + one point 
        }

        [Given(@"Две строки содержат муфту")]
        public void GivenДвеСтрокиСодержатМуфту()
        {
            _lvm.Rows[1].EquipmentId.Should().Be(_equipmentA1Id);
            _lvm.Rows[5].EquipmentId.Should().Be(_equipmentA1Id);
        }

        [When(@"Пользователь исключает муфту из трассы на первом проходе")]
        public void WhenПользовательИсключаетМуфтуИзТрассыНаПервомПроходе()
        {
            _lvm.SelectedRow = _lvm.Rows[1];
            _lvm.ExcludeEquipment();
            _sut.Poller.EventSourcingTick().Wait();
            _lvm.RefreshAsChangesReaction(); // TODO start LandmarkViewModel through LandmarksViewsManager
        }

        [Then(@"На первом проходе муфты нет на обратном есть")]
        public void ThenНаПервомПроходеМуфтыНетНаОбратномЕсть()
        {
            _lvm.Rows[1].EquipmentId.Should().NotBe(_equipmentA1Id);
            _lvm.Rows[5].EquipmentId.Should().Be(_equipmentA1Id);
        }

    }
}

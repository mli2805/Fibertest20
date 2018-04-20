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
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _nodeAId, _equipmentA1Id;
        private Iit.Fibertest.Graph.Trace _trace;
        private NodeUpdateViewModel _vm;


        [Given(@"Трасса использует муфту А1 дважды")]
        public void GivenТрассаИспользуетМуфтуА1Дважды()
        {
            _trace = _sut.CreateTraceDoublePassingClosure();
            _nodeAId = _trace.NodeIds[2];
            _equipmentA1Id = _trace.EquipmentIds[2];
        }

        [Given(@"Открыта форма для редактирования узла с муфтой А1")]
        public void GivenОткрытаФормаДляРедактированияУзлаСМуфтойА1()
        {
            _vm = _sut.ClientContainer.Resolve<NodeUpdateViewModel>();
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

    }
}

using System;
using System.Linq;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.StringResources;
using Optixsoft.SorExaminer.OtdrDataFormat;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class BaseRefAssignedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Iit.Fibertest.Graph.Trace _trace;
        private RtuLeaf _rtuLeaf;
        private TraceLeaf _traceLeaf;
        private Guid _oldPreciseId;
        private Guid _oldFastId;
        private Guid _terminalNodeId;
        private Guid _terminalId;
        private NodeUpdateViewModel _nodeUpdateViewModel;

        [Given(@"Была создана трасса")]
        public void GivenБылаСозданаТрасса()
        {
            _trace = _sut.CreateTraceRtuEmptyTerminal();
            _traceLeaf = (TraceLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(_trace.TraceId);
            _rtuLeaf = (RtuLeaf)_traceLeaf.Parent;
            _terminalNodeId = _sut.ReadModel.Nodes.First(n => n.NodeId == _trace.NodeIds.Last()).NodeId;
            _terminalId = _sut.ReadModel.Equipments.First(e => e.NodeId == _terminalNodeId).EquipmentId;
        }

        [Then(@"Пункт Задать базовые недоступен")]
        public void ThenПунктЗадатьБазовыеНедоступен()
        {
            _traceLeaf.MyContextMenu.First(i => i.Header == Resources.SID_Base_refs_assignment).Command.CanExecute(_traceLeaf).Should().BeFalse();
        }

        [When(@"RTU успешно инициализируется c длинной волны (.*)")]
        public void WhenRtuУспешноИнициализируетсяCДлиннойВолны(string p0)
        {
            _sut.SetNameAndAskInitializationRtu(_rtuLeaf.Id);
            _rtuLeaf.TreeOfAcceptableMeasParams.Units.ContainsKey(p0).Should().BeTrue();
        }


        [Then(@"Пункт Задать базовые становится доступен")]
        public void ThenПунктЗадатьБазовыеСтановитсяДоступен()
        {
            _traceLeaf.MyContextMenu.First(i => i.Header == Resources.SID_Base_refs_assignment).Command.CanExecute(_traceLeaf).Should().BeTrue();
        }


        [When(@"Пользователь указывает пути к точной и быстрой базовам и жмет сохранить")]
        public void WhenПользовательУказываетПутиКТочнойИБыстройБазовамИЖметСохранить()
        {
            _sut.AssignBaseRef(_traceLeaf, SystemUnderTest.Base1625, SystemUnderTest.Base1625, null, Answer.Yes);
        }


        [Then(@"У трассы заданы точная и быстрая базовые")]
        public void ThenУТрассыЗаданыТочнаяИБыстраяБазовые()
        {
            _trace.PreciseId.Should().NotBe(Guid.Empty);
            _trace.FastId.Should().NotBe(Guid.Empty);
        }

        [When(@"Пользователь изменяет быструю и жмет сохранить")]
        public void WhenПользовательИзменяетБыструюИЖметСохранить()
        {
            _oldPreciseId = _trace.PreciseId;
            _oldFastId = _trace.FastId;

            _sut.AssignBaseRef(_traceLeaf, null, SystemUnderTest.Base1625, null, Answer.Yes);
        }

        [Then(@"У трассы старая точная и новая быстрая базовые")]
        public void ThenУТрассыСтараяТочнаяИНоваяБыстраяБызовые()
        {
            _trace.PreciseId.Should().Be(_oldPreciseId);
            _trace.FastId.Should().NotBe(_oldFastId);
        }

        [When(@"Пользователь сбрасывает точную и задает дополнительную и жмет сохранить")]
        public void WhenПользовательСбрасываетТочнуюЗадаетДополнительнуюИЖметСохранить()
        {
            _sut.AssignBaseRef(_traceLeaf, "", null, SystemUnderTest.Base1625, Answer.Yes);
        }

        [Then(@"У трассы не задана точная и старая быстрая и есть дополнительная базовые")]
        public void ThenУТрассыНеЗаданаТочнаяСтараяБыстраяИЕстьДополнительнаяБазовые()
        {
            _trace.PreciseId.Should().Be(Guid.Empty);
            _trace.AdditionalId.Should().NotBe(Guid.Empty);
        }

        [Given(@"Задается имя (.*) для узла с оконечным кроссом")]
        public void GivenЗадаетсяИмяПосленийУзелДляУзлаСОконечнымКроссом(string p0)
        {
            _nodeUpdateViewModel = _sut.ClientScope.Resolve<NodeUpdateViewModel>();
            _nodeUpdateViewModel.Initialize(_terminalNodeId);
            _nodeUpdateViewModel.Title = p0;
            _nodeUpdateViewModel.Save();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Given(@"Оконечный кросс меняется на Другое и имя оборудования (.*)")]
        public void GivenОконечныйКроссМеняетсяНаДругоеИИмяОборудованияДр(string p0)
        {
            _nodeUpdateViewModel = _sut.ClientScope.Resolve<NodeUpdateViewModel>();
            _nodeUpdateViewModel.Initialize(_terminalNodeId);

            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentInfoViewModelHandler(model, Answer.Yes, EquipmentType.Other, 0, 0, p0));

            var item = _nodeUpdateViewModel.EquipmentsInNode.First(i => i.Id == _terminalId);
            item.Command = new UpdateEquipment() { EquipmentId = _terminalId };
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"У сохраненных на сервере базовых третий ориентир имеет имя (.*) и тип Другое")]
        public void ThenУСохраненныхНаСервереБазовыхТретийОриентирИмеетИмяПоследнийУзелДрИТипДругое(string p0)
        {
            var sorFileId = _sut.ReadModel.BaseRefs.First(b=>b.Id == _trace.PreciseId).SorFileId;
            var sorbBytes = _sut.WcfServiceForClient.GetSorBytes(sorFileId).Result;
            var sorData = SorData.FromBytes(sorbBytes);

            sorData.LinkParameters.LandmarkBlocks[2].Comment.Should().Be(p0);
            sorData.LinkParameters.LandmarkBlocks[2].Code.Should().Be(LandmarkCode.Other);
        }

    }
}

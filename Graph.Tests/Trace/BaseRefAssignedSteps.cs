using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class BaseRefAssignedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private readonly MapViewModel _mapViewModel;
        private AssignBaseRefsViewModel _assignBaseRefsViewModel;
        private Guid _traceId;
        private Guid _oldPreciseId;

        public BaseRefAssignedSteps()
        {
            _mapViewModel = new MapViewModel(_sut.Aggregate, _sut.ReadModel);
        }

        [Given(@"Существует трасса")]
        public void GivenСуществуетТрасса()
        {
            _mapViewModel.AddRtuAtGpsLocation();
            _mapViewModel.AddNode();
            _sut.Poller.Tick();
            var rtuNodeId = _sut.ReadModel.Nodes.First().Id;
            var lastNodeId = _sut.ReadModel.Nodes.Last().Id;
            _mapViewModel.AddFiber(rtuNodeId, lastNodeId);
            _sut.Poller.Tick();
            var traceNodes = new PathFinder(_sut.ReadModel).FindPath(rtuNodeId, lastNodeId).ToList();
            var addTraceViewModel = new AddTraceViewModel(_sut.ReadModel, _sut.Aggregate, traceNodes, new List<Guid>());
            addTraceViewModel.Save();
            _sut.Poller.Tick();
            _traceId = _sut.ReadModel.Traces.First().Id;
        }

        [Given(@"И для нее заданы точная и быстрая базовые")]
        public void GivenИДляНееЗаданыВсеТриБазовые()
        {
            var trace = _sut.ReadModel.Traces.Single(t => t.Id == _traceId);
            trace.PreciseId = Guid.NewGuid();
            _oldPreciseId = trace.PreciseId;
            trace.FastId = Guid.NewGuid();
        }


        [When(@"Открыта форма для задания базовых")]
        public void GivenОткрытаФормаДляЗаданияБазовых()
        {
            _assignBaseRefsViewModel = new AssignBaseRefsViewModel(_traceId,_sut.ReadModel, _sut.Aggregate);
        }

        [When(@"Пользователь меняет точную базовую")]
        public void WhenПользовательМеняетТочнуюБазовую()
        {
            _assignBaseRefsViewModel.PreciseBaseFilename = @"..\..\base.sor";
        }

        [When(@"Пользователь сбрасывает быструю базовую")]
        public void WhenПользовательСбрасываетБыструюБазовую()
        {
            _assignBaseRefsViewModel.FastBaseFilename = "";
        }

        [When(@"Пользователь задает дополнительную базовую")]
        public void WhenПользовательЗадаетДополнительнуюБазовую()
        {
            _assignBaseRefsViewModel.AdditionalBaseFilename = @"..\..\base.sor";
        }

        [When(@"Пользователь жмет сохранить")]
        public void WhenПользовательЖметСохранить()
        {
            _assignBaseRefsViewModel.Save();
            _sut.Poller.Tick();
        }

        [When(@"Пользователь жмет отмена")]
        public void WhenПользовательЖметОтмена()
        {
            _assignBaseRefsViewModel.Cancel();
            _sut.Poller.Tick();
        }

        [Then(@"У трассы новая точная базовая")]
        public void ThenУТрассыНоваяТочнаяБазовая()
        {
            _sut.ReadModel.Traces.Single(t => t.Id == _traceId).PreciseId.Should().NotBe(Guid.Empty);
        }

        [Then(@"У трассы не задана быстрая базовая")]
        public void ThenУТрассыНеЗаданаБыстраяБазовая()
        {
            _sut.ReadModel.Traces.Single(t => t.Id == _traceId).FastId.Should().Be(Guid.Empty);
        }

        [Then(@"У трассы задана дополнительная базовая")]
        public void ThenУТрассыЗаданаДополнительнаяБазовая()
        {
            _sut.ReadModel.Traces.Single(t => t.Id == _traceId).AdditionalId.Should().NotBe(Guid.Empty);
        }

        [Then(@"У трассы старая точная базовая")]
        public void ThenУТрассыСтараяТочнаяБазовая()
        {
            _sut.ReadModel.Traces.Single(t => t.Id == _traceId).PreciseId.Should().Be(_oldPreciseId);
        }

        [Then(@"У трассы задана быстрая базовая")]
        public void ThenУТрассыЗаданаБыстраяБазовая()
        {
            _sut.ReadModel.Traces.Single(t => t.Id == _traceId).FastId.Should().NotBe(Guid.Empty);
        }

        [Then(@"У трассы не задана дополнительная базовая")]
        public void ThenУТрассыНеЗаданаДополнительнаяБазовая()
        {
            _sut.ReadModel.Traces.Single(t => t.Id == _traceId).AdditionalId.Should().Be(Guid.Empty);
        }
    }
}

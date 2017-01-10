using System;
using System.Collections.Generic;
using System.Linq;
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

        [Given(@"Открыта форма для задания базовых")]
        public void GivenОткрытаФормаДляЗаданияБазовых()
        {
            _assignBaseRefsViewModel = new AssignBaseRefsViewModel(_traceId,_sut.ReadModel, _sut.Aggregate);
        }

        [When(@"Пользователь жмет сохранить")]
        public void WhenПользовательЖметСохранить()
        {
            _assignBaseRefsViewModel.Save();
        }

        [Then(@"Изменения сохраняются")]
        public void ThenИзмененияСохраняются()
        {
            ScenarioContext.Current.Pending();
        }
    }
}

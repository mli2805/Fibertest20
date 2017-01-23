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
    public sealed class TraceAddedThirdStageSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _rtuNodeId;
        private Guid _lastNodeId;

        private TraceAddViewModel _traceAddViewModel;

        private List<Guid> _nodes;
        private List<Guid> _equipments = new List<Guid>();

        [Given(@"Сформирован набор данных трассы и открыто окно создания трассы")]
        public void GivenСформированНаборДанныхТрассыИОткрытоОкноСозданияТрассы()
        {
            _sut.CreateFieldForPathFinderTest(out _rtuNodeId, out _lastNodeId);
            _nodes = new PathFinder(_sut.ReadModel).FindPath(_rtuNodeId, _lastNodeId).ToList();

            _equipments.Add(_sut.ReadModel.Rtus.Single(r => r.NodeId == _rtuNodeId).Id);
            foreach (var nodeId in _nodes.Skip(1))
            {
                _equipments.Add(_sut.ReadModel.Equipments.FirstOrDefault(e=>e.NodeId == nodeId) == null ? Guid.Empty : Guid.NewGuid());
            }

            _traceAddViewModel = new TraceAddViewModel(_sut.FakeWindowManager, _sut.ReadModel, _sut.Aggregate, _nodes, _equipments);
        }

        [When(@"Пользователь жмет Применить при пустом имени трассы")]
        public void GivenПользовательЖметПрименитьПриПустомИмениТрассы()
        {
            _traceAddViewModel.Title = "";
            _traceAddViewModel.Save();
        }

        [When(@"Пользователь вводит название трассы и жмет Сохранить")]
        public void WhenПользовательВводитНазваниеТрассыИЖметСохранить()
        {
            _traceAddViewModel.Title = "Doesn't matter";
            _traceAddViewModel.Save();
            _sut.Poller.Tick();
        }

        [When(@"Пользователь жмет Отмена")]
        public void WhenПользовательЖметОтмена()
        {
            _traceAddViewModel.Cancel();
        }

        [Then(@"Окно не закрывается")]
        public void ThenОкноНеЗакрывается()
        {
            _traceAddViewModel.IsClosed = false;
        }

        [Then(@"Окно закрывается")]
        public void ThenОкноЗакрывается()
        {
            _traceAddViewModel.IsClosed = true;
        }

        [Then(@"Новая трасса сохраняется и окно закрывается")]
        public void ThenНоваяТрассаСохраняетсяИОкноЗакрывается()
        {
            _sut.ReadModel.Traces.Count.Should().Be(1);
            _traceAddViewModel.IsClosed = true;
        }

        [Then(@"Трасса не сохраняется")]
        public void ThenТрассаНеСохраняется()
        {
            _sut.ReadModel.Traces.Count.Should().Be(0);
        }


    }
}

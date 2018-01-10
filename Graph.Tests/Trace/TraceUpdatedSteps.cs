using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceUpdatedSteps
    {
        private SystemUnderTest _sut = new SystemUnderTest();
        private Guid _traceId;
        private TraceInfoViewModel _viewModel;
        private const string NewTitle = @"new trace title";
        private const string NewComment = @"new trace comment";

        [Given(@"Существует трасса")]
        public void GivenСуществуетТрасса()
        {
            _traceId = _sut.CreateTraceRtuEmptyTerminal().Id;
        }
        [Given(@"Пользователь открывает форму Информация и вносит изменения")]
        public void GivenПользовательОткрываетФормуИнформацияИВноситИзменения()
        {
            var trace = _sut.ReadModel.Traces.FirstOrDefault(t => t.Id == _traceId);
            if (trace == null) return;
            _viewModel = new TraceInfoViewModel(_sut.ReadModel, _sut.ShellVm.C2DWcfManager, _sut.FakeWindowManager, _traceId, trace.Equipments, trace.Nodes);
            _viewModel.Title = NewTitle;
            _viewModel.Comment = NewComment;
            _viewModel.IsTraceModeLight = false;
        }

        [When(@"Пользователь жмет Сохранить")]
        public void WhenПользовательЖметСохранить()
        {
            _viewModel.Save();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Пользователь жмет Отмена")]
        public void WhenПользовательЖметОтмена()
        {
            _viewModel.Cancel();
        }

        [Then(@"Изменения сохраняются")]
        public void ThenИзмененияСохраняются()
        {
            var trace = _sut.ReadModel.Traces.FirstOrDefault(t => t.Id == _traceId);
            if (trace == null) return;

            _viewModel = new TraceInfoViewModel(_sut.ReadModel, _sut.ShellVm.C2DWcfManager, _sut.FakeWindowManager, _traceId, trace.Equipments, trace.Nodes);
            _viewModel.Title.Should().Be(NewTitle);
            _viewModel.Comment.Should().Be(NewComment);
            _viewModel.IsTraceModeLight.Should().BeFalse();
        }

        [Then(@"Изменения НЕ сохраняются")]
        public void ThenИзмененияНеСохраняются()
        {
            var trace = _sut.ReadModel.Traces.FirstOrDefault(t => t.Id == _traceId);
            if (trace == null) return;

            _viewModel = new TraceInfoViewModel(_sut.ReadModel, _sut.ShellVm.C2DWcfManager, _sut.FakeWindowManager, _traceId, trace.Equipments, trace.Nodes);
            _viewModel.Title.Should().NotBe(NewTitle);
            _viewModel.Comment.Should().NotBe(NewComment);
            _viewModel.IsTraceModeLight.Should().BeTrue();
        }
    }
}

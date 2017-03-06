using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceAttachedSteps
    {
        private readonly SutForTraceAttach _sut = new SutForTraceAttach();
        private Guid _traceId;
        private int _portNumber;
        private Guid _rtuId;

        [Given(@"Создаем трассу РТУ инициализирован")]
        public void GivenСоздаемТрассуРтуИнициализирован()
        {
           _sut.TraceCreatedAndRtuInitialized(out _traceId, out _rtuId);
        }

        [When(@"Пользователь выбирает присоединить к порту (.*) трассу и жмет Сохранить")]
        public void WhenПользовательВыбираетПрисоединитьКПортуТрассуИЖметСохранить(int p0)
        {
            _portNumber = p0;
            _sut.AttachTrace(_traceId, _portNumber, Answer.Yes);
            _sut.Poller.Tick();
        }

        [When(@"Пользователь выбирает присоединить к порту (.*) трассу а жмет Отмена")]
        public void WhenПользовательВыбираетПрисоединитьКПортуТрассуАЖметОтмена(int p0)
        {
            _portNumber = p0;
            _sut.AttachTrace(_traceId, _portNumber, Answer.Cancel);
        }

        [Then(@"Трасса присоединяется к порту РТУ")]
        public void ThenТрассаПрисоединяетсяКПортуРту()
        {
            _sut.ReadModel.Traces.First(t => t.Id == _traceId).Port.Should().Be(_portNumber);
            (_sut.ShellVm.MyLeftPanelViewModel.TreeReadModel.Tree.GetById(_rtuId).Children[_portNumber - 1] is TraceLeaf)
                .Should().BeTrue();
            _sut.ShellVm.MyLeftPanelViewModel.TreeReadModel.Tree.GetById(_rtuId).Children[_portNumber - 1].Id.Should().Be(_traceId);
        }

        [Then(@"Трасса НЕ присоединяется к порту РТУ")]
        public void ThenТрассаНеПрисоединяетсяКПортуРту()
        {
            _sut.ReadModel.Traces.First(t => t.Id == _traceId).Port.Should().BeLessThan(1);
            (_sut.ShellVm.MyLeftPanelViewModel.TreeReadModel.Tree.GetById(_rtuId).Children[_portNumber - 1] is PortLeaf)
                .Should().BeTrue();
        }

    }
}

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
        private RtuLeaf _rtuLeaf;
        private OtauLeaf _otauLeaf;

        [Given(@"Создаем трассу РТУ инициализирован")]
        public void GivenСоздаемТрассуРтуИнициализирован()
        {
           _rtuLeaf = _sut.TraceCreatedAndRtuInitialized(out _traceId, out _rtuId);
        }

        [Given(@"Пользователь подключает доп переключатель")]
        public void GivenПользовательПодключаетДопПереключатель()
        {
            _otauLeaf = _sut.AttachOtauToRtu(_rtuLeaf, 2);
        }

        [When(@"Пользователь выбирает присоединить к (.*) порту переключателя трассу и жмет Сохранить")]
        public void WhenПользовательВыбираетПрисоединитьКПортуПереключателяТрассуИЖметСохранить(int p0)
        {
            _portNumber = p0;
            _sut.AttachTraceTo(_traceId, _otauLeaf, _portNumber, Answer.Yes);
            _sut.Poller.Tick();
        }

        [When(@"Пользователь выбирает присоединить к порту (.*) трассу и жмет Сохранить")]
        public void WhenПользовательВыбираетПрисоединитьКПортуТрассуИЖметСохранить(int p0)
        {
            _portNumber = p0;
            _sut.AttachTraceTo(_traceId, _rtuLeaf, _portNumber, Answer.Yes);
            _sut.Poller.Tick();
        }

        [When(@"Пользователь выбирает присоединить к порту (.*) трассу а жмет Отмена")]
        public void WhenПользовательВыбираетПрисоединитьКПортуТрассуАЖметОтмена(int p0)
        {
            _portNumber = p0;
            _sut.AttachTraceTo(_traceId, _rtuLeaf, _portNumber, Answer.Cancel);
        }

        [When(@"Пользователь выбирает отсоединить трассу")]
        public void WhenПользовательВыбираетОтсоединитьТрассу()
        {
            var traceLeaf = (TraceLeaf)_sut.ShellVm.MyLeftPanelViewModel.TreeReadModel.Tree.GetById(_traceId);
            traceLeaf.DetachTraceAction(null);
            _sut.Poller.Tick();
        }

        [Then(@"Трасса присоединяется к порту РТУ")]
        public void ThenТрассаПрисоединяетсяКПортуРту()
        {
            _sut.ReadModel.Traces.First(t => t.Id == _traceId).Port.Should().Be(_portNumber);
            (_sut.ShellVm.MyLeftPanelViewModel.TreeReadModel.Tree.GetById(_rtuId).Children[_portNumber - 1] is TraceLeaf)
                .Should().BeTrue();
            _sut.ShellVm.MyLeftPanelViewModel.TreeReadModel.Tree.GetById(_rtuId).Children[_portNumber - 1].Id.Should().Be(_traceId);
        }

        [Then(@"Трасса присоединяется к (.*) порту переключателя")]
        public void ThenТрассаПрисоединяетсяКПортуПереключателя(int p0)
        {
            (_otauLeaf.Children[p0 - 1] as TraceLeaf).Should().NotBeNull();
            _otauLeaf.Children[p0 - 1].Id.Should().Be(_traceId);
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

using System.Linq;
using FluentAssertions;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuInitializedSteps
    {
        private readonly SutForTraceAttach _sut = new SutForTraceAttach();
        private RtuLeaf _rtuLeaf;

        [Given(@"Создан РТУ даже с трассой")]
        public void GivenСозданРтуДажеСТрассой()
        {
            _sut.CreateTraceRtuEmptyTerminal();
            var rtuId = _sut.ReadModel.Rtus.Single().Id;
            _rtuLeaf = (RtuLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(rtuId);
        }

        [When(@"Пользователь открывает форму инициализации и жмет Инициализировать")]
        public void WhenПользовательОткрываетФормуИнициализацииИЖметИнициализировать()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuInitializeHandler(model, Answer.Yes));

            _rtuLeaf.RtuSettingsAction(null);
            _sut.Poller.Tick();
        }

        [When(@"Пользователь открывает форму инициализации и жмет Отмена")]
        public void WhenПользовательОткрываетФормуИнициализацииИЖметОтмена()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuInitializeHandler(model, Answer.Cancel));

            _rtuLeaf.RtuSettingsAction(null);
            _sut.Poller.Tick();
        }

        [Then(@"РТУ инициализирован")]
        public void ThenРтуИнициализирован()
        {
            _rtuLeaf.ChildrenImpresario.Children.Count.Should().BeGreaterThan(1);
        }

        [Then(@"РТУ НЕ инициализирован")]
        public void ThenРтунеИнициализирован()
        {
            _rtuLeaf.ChildrenImpresario.Children.Count.Should().Be(1);
        }

    }
}

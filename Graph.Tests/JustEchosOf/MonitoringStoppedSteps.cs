using System;
using System.Collections.Generic;
using Iit.Fibertest.Client;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class MonitoringStoppedSteps
    {
        private readonly SutForMonitoringStatePictograms _sut = new SutForMonitoringStatePictograms();
        private Guid _rtuId;
        private RtuLeaf _rtuLeaf;
        private OtauLeaf _otauLeaf;
        private List<Iit.Fibertest.Graph.Trace> _traces;

        [Given(@"Существует RTU с БОПом")]
        public void GivenСуществуетRtuсбоПом()
        {
            _traces = _sut.CreateThreeTracesRtuEmptyTerminal();

            _rtuId = _traces[0].RtuId;
            _rtuLeaf = _sut.InitializeRtu(_rtuId);

            int portNumber = 3;
            _otauLeaf = _sut.AttachOtauToRtu(_rtuLeaf, portNumber);

        }

        [Given(@"К RTU подключена трасса с базовыми")]
        public void GivenКrtuПодключенаТрассаСБазовыми()
        {
            _sut.AttachTraceTo(_traces[0].Id, _rtuLeaf, 1, Answer.Yes);
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BaseRefAssignHandler(model, _traces[0].Id,
                SystemUnderTest.Base1625, SystemUnderTest.Base1625, null, Answer.Yes));
            var traceLeaf = (TraceLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_traces[0].Id);
            _sut.TraceLeafActions.AssignBaseRefs(traceLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }


        [Given(@"К БОПу подключено две трассы и второй назначены базовые")]
        public void GivenКбоПуПодключеноДвеТрассыИВторойНазначеныБазовые()
        {
            _sut.AttachTraceTo(_traces[1].Id, _otauLeaf, 3, Answer.Yes);
            _sut.AttachTraceTo(_traces[2].Id, _otauLeaf, 4, Answer.Yes);

            _sut.FakeWindowManager.RegisterHandler(model => _sut.BaseRefAssignHandler(model, _traces[2].Id,
                SystemUnderTest.Base1625, SystemUnderTest.Base1625, null, Answer.Yes));
            var traceLeaf = (TraceLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_traces[2].Id);
            _sut.TraceLeafActions.AssignBaseRefs(traceLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Пользователь жмет Автоматический режим")]
        public void GivenПользовательЖметАвтоматическийРежим()
        {
        }

        [Then(@"Загорается пиктограмма мониторинга у RTU и трасс")]
        public void ThenЗагораетсяПиктограммаМониторингаУrtuиТрасс()
        {
        }
    }
}
using System;
using System.Linq;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class BaseRefAdjustedBehindTheScenesSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Iit.Fibertest.Graph.Rtu _rtu;
        private Iit.Fibertest.Graph.Trace _trace;
        private TraceLeaf _traceLeaf;

        private TraceStatisticsViewModel _vm;
        private string _assignedBy;
        private DateTime _assignedAt;

        [Given(@"Существует трасса с заданной базовой")]
        public void GivenСуществуетТрассаСЗаданнойБазовой()
        {
            _rtu = _sut.SetInitializedRtu();
            _trace = _sut.SetTrace(_rtu.NodeId, @"Trace1");
            _traceLeaf = (TraceLeaf)_sut.ShellVm.TreeOfRtuModel.Tree.GetById(_trace.Id);
            _sut.AssignBaseRef(_traceLeaf, SystemUnderTest.Base1550Lm4YesThresholds, null, null, Answer.Yes);
        }

        [When(@"Пользователь открывает статистику по трассе")]
        public async void WhenПользовательОткрываетСтатистикуПоТрассе()
        {
            _vm = _sut.Container.Resolve<TraceStatisticsViewModel>();
            await _vm.Initialize(_trace.Id);
        }

        [Then(@"Там есть строка для базовой с именем и временем задания")]
        public void ThenТамЕстьСтрокаДляБазовойСИменемИВременемЗадания()
        {
            var line = _vm.BaseRefs.First(i => i.BaseRefType == BaseRefType.Precise);
            line.Should().NotBe(null);
            _assignedBy = line.AssignedBy;
            _assignedAt = line.AssignedAt;
        }

    }
}

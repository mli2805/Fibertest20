using System;
using System.Linq;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.StringResources;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceAddedIntegrationalSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _rtuNodeId;
        private Guid _lastNodeId;
        private TraceLeaf _traceLeaf;
        private TraceInfoViewModel _traceInfoViewModel;

        private const string TraceTitle = "Some trace";
        private const string TraceComment = "Comment for trace";

        private int _traceCountCutOff;

        [Given(@"Предусловия выполнены")]
        public void GivenПредусловияВыполнены()
        {
            _sut.CreateFieldForPathFinderTest(out _rtuNodeId, out _lastNodeId, out _, out _);

            _sut.FakeWindowManager.RegisterHandler(model => _sut.OneLineMessageBoxAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.TraceContentChoiceHandler(model, Answer.Yes, 0));

            _traceCountCutOff = _sut.ReadModel.Traces.Count;
        }

        [When(@"Открылась форма сохранения трассы")]
        public void WhenОткрыласьФормаСохраненияТрассы()
        {
            _traceInfoViewModel = _sut.ShellVm.GlobalScope.Resolve<TraceInfoViewModel>();
        }

        [Then(@"Кнопка Сохранить недоступна пока поле названия трассы пустое")]
        public void ThenКнопкаСохранитьНедоступнаПокаПолеНазванияТрассыПустое()
        {
            _traceInfoViewModel.Model.IsButtonSaveEnabled.Should().BeFalse();
        }

        [When(@"Пользователь вводит название и коммент трассы и жмет Сохранить")]
        public void WhenПользовательВводитНазваниеИКомментТрассыИЖметСохранить()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.AddTraceViewHandler(model, TraceTitle, TraceComment, Answer.Yes));

            _sut.ShellVm.ComplyWithRequest(new RequestAddTrace() {LastNodeId = _lastNodeId, NodeWithRtuId = _rtuNodeId});
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Трасса сохраняется")]
        public void ThenТрассаСохраняется()
        {
            var trace = _sut.ReadModel.Traces.Last();
            trace.Title.Should().Be(TraceTitle);
            trace.Comment.Should().Be(TraceComment);
            _traceLeaf = (TraceLeaf)_sut.ShellVm.TreeOfRtuModel.Tree.GetById(trace.Id);
            trace.Equipments.Contains(Guid.Empty).Should().BeFalse();
        }

        [Then(@"Имя в дереве совпадает с именем трассы")]
        public void ThenИмяВДеревеСовпадаетСИменемТрассы()
        {
            _traceLeaf.Name.Should().Be(_traceLeaf.Title);
        }

        [Then(@"У неприсоединенной трассы есть пункты Очистить и Удалить")]
        public void ThenУНеприсоединеннойТрассыЕстьПунктыОчиститьИУдалить()
        {
            _traceLeaf.MyContextMenu.FirstOrDefault(item => item?.Header == Resources.SID_Clean).Should().NotBeNull();
            _traceLeaf.MyContextMenu.FirstOrDefault(item => item?.Header == Resources.SID_Remove).Should().NotBeNull();
        }

        [Then(@"Нет пункта Отсоединить и трех пунктов Измерения")]
        public void ThenНетПунктаОтсоединитьИТрехПунктовИзмерения()
        {
            _traceLeaf.MyContextMenu.FirstOrDefault(item => item?.Header == Resources.SID_Detach_trace).Should().BeNull();
            _traceLeaf.MyContextMenu.FirstOrDefault(item => item?.Header == Resources.SID_Precise_monitoring_out_of_turn).Should().BeNull();
            _traceLeaf.MyContextMenu.FirstOrDefault(item => item?.Header == Resources.SID_Measurement__Client_).Should().BeNull();
            _traceLeaf.MyContextMenu.FirstOrDefault(item => item?.Header == Resources.SID_Measurement__RFTS_Reflect_).Should().BeNull();
        }

        [When(@"Пользователь что-то вводит но жмет Отмена")]
        public void WhenПользовательЧто_ТоВводитНоЖметОтмена()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.AddTraceViewHandler(model, TraceTitle, TraceComment, Answer.Cancel));

            _sut.ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = _lastNodeId, NodeWithRtuId = _rtuNodeId });
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Трасса не сохраняется")]
        public void ThenТрассаНеСохраняется()
        {
            _sut.ReadModel.Traces.Count.Should().Be(_traceCountCutOff);
        }
    }
}

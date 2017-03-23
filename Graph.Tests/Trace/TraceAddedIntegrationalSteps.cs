using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceAddedIntegrationalSteps
    {
        private readonly SutForTraceAdded _sut = new SutForTraceAdded();
        private Guid _rtuNodeId;
        private Guid _lastNodeId;
        private TraceLeaf _traceLeaf;

        private const string TraceTitle = "Some trace";
        private const string TraceComment = "Comment for trace";

        private int _traceCountCutOff;

        [Given(@"Предусловия выполнены")]
        public void GivenПредусловияВыполнены()
        {
            Guid wrongNodeId, wrongNodeWithEqId;
            _sut.CreateFieldForPathFinderTest(out _rtuNodeId, out _lastNodeId, out wrongNodeId, out wrongNodeWithEqId);

            _sut.FakeWindowManager.RegisterHandler(model => _sut.QuestionAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));

            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));

            _traceCountCutOff = _sut.ReadModel.Traces.Count;
        }

        [When(@"Пользователь вводит название и коммент трассы и жмет Сохранить")]
        public void WhenПользовательВводитНазваниеИКомментТрассыИЖметСохранить()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.AddTraceViewHandler(model, TraceTitle, TraceComment, Answer.Yes));

            _sut.ShellVm.ComplyWithRequest(new RequestAddTrace() {LastNodeId = _lastNodeId, NodeWithRtuId = _rtuNodeId});
            _sut.Poller.Tick();
        }

        [Then(@"Трасса сохраняется")]
        public void ThenТрассаСохраняется()
        {
            var trace = _sut.ReadModel.Traces.Last();
            trace.Title.Should().Be(TraceTitle);
            trace.Comment.Should().Be(TraceComment);
            _traceLeaf = (TraceLeaf)_sut.ShellVm.TreeOfRtuModel.Tree.GetById(trace.Id);
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
            _traceLeaf.MyContextMenu.FirstOrDefault(item => item?.Header == Resources.SID_Presice_out_of_turn_measurement).Should().BeNull();
            _traceLeaf.MyContextMenu.FirstOrDefault(item => item?.Header == Resources.SID_Measurement__Client_).Should().BeNull();
            _traceLeaf.MyContextMenu.FirstOrDefault(item => item?.Header == Resources.SID_Measurement__RFTS_Reflect_).Should().BeNull();
        }

        [When(@"Пользователь что-то вводит но жмет Отмена")]
        public void WhenПользовательЧто_ТоВводитНоЖметОтмена()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.AddTraceViewHandler(model, TraceTitle, TraceComment, Answer.Cancel));

            _sut.ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = _lastNodeId, NodeWithRtuId = _rtuNodeId });
            _sut.Poller.Tick();
        }

        [Then(@"Трасса не сохраняется")]
        public void ThenТрассаНеСохраняется()
        {
            _sut.ReadModel.Traces.Count.Should().Be(_traceCountCutOff);
        }
    }
}

using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class OtauDetachSteps
    {
        private readonly SutForTraceAttach _sut = new SutForTraceAttach();
        private Guid _rtuId;
        private Iit.Fibertest.Graph.Trace _trace;
        private RtuLeaf _rtuLeaf;
        private OtauLeaf _otauLeaf;
        private int portNumber = 3;


        [Given(@"Существует инициализированый RTU с неприсоединенной трассой")]
        public void GivenСуществуетИнициализированыйRtuсНеприсоединеннойТрассой()
        {
            _trace = _sut.CreateTraceRtuEmptyTerminal();
            _rtuId = _trace.RtuId;
            _rtuLeaf = _sut.InitializeRtu(_rtuId);
        }

        [Given(@"К RTU подключен доп оптический переключатель")]
        public void GivenКrtuПодключенДопОптическийПереключатель()
        {
            _otauLeaf = _sut.AttachOtauToRtu(_rtuLeaf, portNumber);
        }

        [Given(@"Трасса подключена к переключателю")]
        public void GivenТрассаПодключенаКПереключателю()
        {
            _sut.AttachTraceTo(_trace.Id, _otauLeaf, 3, Answer.Yes);
        }

        [Then(@"Пункт удаление переключателя недоступен")]
        public void ThenПунктУдалениеПереключателяНедоступен()
        {
            _otauLeaf.MyContextMenu.First(i => i.Header == Resources.SID_Remove)
                .Command.CanExecute(null)
                .Should()
                .BeFalse();
        }

        [Given(@"Пользователь жмет удалить оптический переключатель")]
        public void GivenПользовательЖметУдалитьОптическийПереключатель()
        {
            _otauLeaf.OtauRemoveAction(null);
            _sut.Poller.Tick();
        }

        [Then(@"Оптический переключатель удален")]
        public void ThenОптическийПереключательУдален()
        {
            _sut.ReadModel.Otaus.FirstOrDefault(o => o.Id == _otauLeaf.Id).Should().BeNull();
        }
    }
}

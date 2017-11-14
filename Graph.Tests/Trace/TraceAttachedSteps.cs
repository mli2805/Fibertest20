using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.StringResources;
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
        private TraceLeaf _traceLeaf;

        [Given(@"Создаем трассу РТУ инициализирован")]
        public void GivenСоздаемТрассуРтуИнициализирован()
        {
            _rtuLeaf = _sut.TraceCreatedAndRtuInitialized(out _traceId, out _rtuId);
            _traceLeaf = (TraceLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_traceId);
        }

        [When(@"Пользователь выбирает присоединить к порту (.*) трассу и жмет Сохранить")]
        public void WhenПользовательВыбираетПрисоединитьКПортуТрассуИЖметСохранить(int p0)
        {
            _portNumber = p0;
            _sut.AttachTraceTo(_traceId, _rtuLeaf, _portNumber, Answer.Yes);
            _sut.Poller.Tick();
            _traceLeaf = (TraceLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_traceId);
        }

        [Then(@"У присоединенной трассы нет пунктов Очистить и Удалить")]
        public void ThenУПрисоединеннойТрассыНетПунктовОчиститьИУдалить()
        {
            _traceLeaf.MyContextMenu.FirstOrDefault(item => item?.Header == Resources.SID_Clean).Should().BeNull();
            _traceLeaf.MyContextMenu.FirstOrDefault(item => item?.Header == Resources.SID_Remove).Should().BeNull();
        }

        [Then(@"Есть пункт Отсоединить и три пункта Измерения")]
        public void ThenЕстьПунктОтсоединитьИТриПунктаИзмерения()
        {
            _traceLeaf.MyContextMenu.FirstOrDefault(item => item?.Header == Resources.SID_Detach_trace).Should().NotBeNull();
            _traceLeaf.MyContextMenu.FirstOrDefault(item => item?.Header == Resources.SID_Presice_out_of_turn_measurement).Should().NotBeNull();
            _traceLeaf.MyContextMenu.FirstOrDefault(item => item?.Header == Resources.SID_Measurement__Client_).Should().NotBeNull();
            _traceLeaf.MyContextMenu.FirstOrDefault(item => item?.Header == Resources.SID_Measurement__RFTS_Reflect_).Should().NotBeNull();
        }

        [Then(@"Имя в дереве это номер порта и имя трассы")]
        public void ThenИмяВДеревеЭтоНомерПортаИИмяТрассы()
        {
            _traceLeaf.Name.Should().Be(string.Format(Resources.SID_Port_trace, _portNumber, _traceLeaf.Title));
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
            _traceLeaf = (TraceLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_traceId);
        }

        [Then(@"Имя в дереве это номер порта переключателя номер расширенного порта RTU и имя трассы")]
        public void ThenИмяВДеревеЭтоНомерПортаПереключателяНомерРасширенногоПортаRtuиИмяТрассы()
        {
            _traceLeaf.Name.Should().
                Be(string.Format(Resources.SID_Port_trace, _portNumber, _traceLeaf.Title));
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
            var traceLeaf = (TraceLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_traceId);
            _sut.TraceLeafActions.DetachTrace(traceLeaf);
            _sut.Poller.Tick();
        }

        [Then(@"Трасса присоединяется к порту РТУ")]
        public void ThenТрассаПрисоединяетсяКПортуРту()
        {
            _sut.ReadModel.Traces.First(t => t.Id == _traceId).Port.Should().Be(_portNumber);
            var rtuLeaf = (RtuLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_rtuId);
            (rtuLeaf.ChildrenImpresario.Children[_portNumber - 1] is TraceLeaf).Should().BeTrue();
            rtuLeaf.ChildrenImpresario.Children[_portNumber - 1].Id.Should().Be(_traceId);
        }

        [Then(@"Трасса присоединяется к (.*) порту переключателя")]
        public void ThenТрассаПрисоединяетсяКПортуПереключателя(int p0)
        {
            (_otauLeaf.ChildrenImpresario.Children[p0 - 1] as TraceLeaf).Should().NotBeNull();
            _otauLeaf.ChildrenImpresario.Children[p0 - 1].Id.Should().Be(_traceId);
        }

        [Then(@"Трасса НЕ присоединяется к порту РТУ")]
        public void ThenТрассаНеПрисоединяетсяКПортуРту()
        {
            _sut.ReadModel.Traces.First(t => t.Id == _traceId).Port.Should().BeLessThan(1);
            var rtuLeaf = (RtuLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_rtuId);
            (rtuLeaf.ChildrenImpresario.Children[_portNumber - 1] is PortLeaf).Should().BeTrue();
        }

    }
}

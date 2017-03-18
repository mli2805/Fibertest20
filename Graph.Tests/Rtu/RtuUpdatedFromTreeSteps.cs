using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuUpdatedFromTreeSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _saidRtuId;
        private const string Title = "Title for test";
        private const string Comment = "Comment for test";

        [Given(@"Существует RTU")]
        public void GivenСуществуетRtu()
        {
            _sut.ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation()).Wait();
            _sut.Poller.Tick();
            _saidRtuId = _sut.ShellVm.ReadModel.Rtus.Last().Id;
        }

        [When(@"Пользователь кликает Информация в меню RTU что-то вводит и жмет сохранить")]
        public void WhenПользовательКликаетИнформацияВМенюRTUЧто_ТоВводитИЖметСохранить()
        {
            //_sut.FakeWindowManager.RegisterHandler(model => model is RtuUpdateViewModel);
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuUpdateHandler(model, Title, Comment, Answer.Yes));

            var rtuLeaf = (RtuLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.First(r => r.Id == _saidRtuId);
            var menuItem = rtuLeaf.MyContextMenu.First(i => i.Header == Resources.SID_Information);
            menuItem.Command.Execute(null);
            _sut.Poller.Tick();
        }

        [Then(@"Изменения применяются")]
        public void ThenИзмененияПрименяются()
        {
            var rtuUpdateViewModel = _sut.FakeWindowManager.Log.OfType<RtuUpdateViewModel>().LastOrDefault();
            rtuUpdateViewModel.Should().NotBeNull();
            rtuUpdateViewModel?.RtuId.Should().Be(_saidRtuId);

            _sut.ReadModel.Rtus.First(r => r.Id == _saidRtuId).Title.Should().Be(Title);
            _sut.ReadModel.Rtus.First(r => r.Id == _saidRtuId).Comment.Should().Be(Comment);

            _sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.First(r => r.Id == _saidRtuId).Title.Should().Be(Title);
        }
    }
}

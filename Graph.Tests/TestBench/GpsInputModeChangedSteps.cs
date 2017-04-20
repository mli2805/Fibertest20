using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class GpsInputModeChangedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _rtuId;
        private RtuUpdateViewModel _rtuUpdateViewModel;

        [Given(@"Пользователь создает RTU в точке с координатами (.*) и (.*)")]
        public void GivenПользовательСоздаетRtuвТочкеСКоординатамиИ(double p0, double p1)
        {
            _sut.ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation() { Latitude = p0, Longitude = p1 }).Wait();
            _sut.Poller.Tick();
            _rtuId = _sut.ReadModel.Rtus.Last().Id;
        }

        [When(@"Пользователь открывает окно для редактирования")]
        public void WhenПользовательОткрываетОкноДляРедактирования()
        {
            _rtuUpdateViewModel = new RtuUpdateViewModel(_rtuId, _sut.ReadModel);
        }

        [Then(@"Координаты должны быть ""(.*)"" ""(.*)""  ""(.*)"" ""(.*)""")]
        public void ThenКоординатыДолжныБыть(string p0, string p1, string p2, string p3)
        {
            _rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLatitude.Degrees.Should().Be(p0);
            _rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLatitude.Minutes.Should().Be(p1);
            _rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLongitude.Degrees.Should().Be(p2);
            _rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLongitude.Minutes.Should().Be(p3);
        }

        [When(@"Пользователь выбирает формат градусы")]
        public void WhenПользовательВыбираетФорматГрадусы()
        {
            _rtuUpdateViewModel.GpsInputViewModel.SelectedGpsInputMode =
                _rtuUpdateViewModel.GpsInputViewModel.GpsInputModes.First(i => i.Mode == GpsInputMode.Degrees);
        }

        [Then(@"Координаты в таком формате должны быть ""(.*)"" ""(.*)""")]
        public void ThenКоординатыВТакомФорматеДолжныБыть(string p0, string p1)
        {
            _rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLatitude.Degrees.Should().Be(p0);
            _rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLongitude.Degrees.Should().Be(p1);
        }

        [When(@"Пользователь выбирает формат градусы-минуты-секунды")]
        public void WhenПользовательВыбираетФорматГрадусы_Минуты_Секунды()
        {
            _rtuUpdateViewModel.GpsInputViewModel.SelectedGpsInputMode =
                _rtuUpdateViewModel.GpsInputViewModel.GpsInputModes.First(
                    i => i.Mode == GpsInputMode.DegreesMinutesAndSeconds);
        }

        [Then(@"Координаты должны быть ""(.*)"" ""(.*)"" ""(.*)"" ""(.*)"" ""(.*)"" ""(.*)""")]
        public void ThenКоординатыДолжныБыть(string p0, string p1, string p2, string p3, string p4, string p5)
        {
            _rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLatitude.Degrees.Should().Be(p0);
            _rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLatitude.Minutes.Should().Be(p1);
            _rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLatitude.Seconds.Should().Be(p2);
            _rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLongitude.Degrees.Should().Be(p3);
            _rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLongitude.Minutes.Should().Be(p4);
            _rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLongitude.Seconds.Should().Be(p5);
        }

        [When(@"Пользователь изменяет координаты а затем нажимает Отменить изменения")]
        public void WhenПользовательИзменяетКоординатыАЗатемНажимаетОтменитьИзменения()
        {
            _rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLatitude.Degrees = @"41";
            _rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLatitude.Minutes = @"36";
            _rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLatitude.Seconds = @"28.97";
            _rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLongitude.Degrees = @"111";
            _rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLongitude.Minutes = @"53";
            _rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLongitude.Seconds = @"27.83";

            _rtuUpdateViewModel.GpsInputViewModel.DropChanges();
        }

    }
}

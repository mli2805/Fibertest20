using System;
using System.Linq;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.UtilsLib;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class GpsInputModeChangedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _rtuId;
        private RtuUpdateViewModel _rtuUpdateViewModel;

        [Given(@"Ранее пользователь установил формат градусы-минуты")]
        public void GivenРанееПользовательУстановилФорматГрадусы_Минуты()
        {
            var iniFile = _sut.ClientContainer.Resolve<IniFile>();
            iniFile.Write(IniSection.Miscellaneous, IniKey.GpsInputMode, 1);
        }

        [Given(@"Пользователь создает RTU в точке с координатами (.*) и (.*)")]
        public void GivenПользовательСоздаетRtuвТочкеСКоординатамиИ(double p0, double p1)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Yes));
            _sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation() { Latitude = p0, Longitude = p1 });
            _sut.Poller.EventSourcingTick().Wait();
            _rtuId = _sut.ReadModel.Rtus.Last().Id;
        }

        [When(@"Открывается окно для редактирования")]
        public void WhenОткрываетсяОкноДляРедактирования()
        {
             _rtuUpdateViewModel = _sut.ClientContainer.Resolve<RtuUpdateViewModel>();
            _rtuUpdateViewModel.Initialize(_rtuId);
            _rtuUpdateViewModel.GpsInputViewModel.SelectedGpsInputModeComboItem =
                _rtuUpdateViewModel.GpsInputViewModel.GpsInputModes.First(i =>
                    i.Mode == GpsInputMode.DegreesAndMinutes);
        }

        [Then(@"Координаты должны быть ""(.*)"" ""(.*)""  ""(.*)"" ""(.*)""")]
        public void ThenКоординатыДолжныБыть(double p0, double p1, double p2, double p3)
        {
            double.Parse(_rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLatitude.Degrees).Should().BeApproximately(p0, 0.1F);
            double.Parse(_rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLatitude.Minutes).Should().BeApproximately(p1, 0.00019F);
            double.Parse(_rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLongitude.Degrees).Should().BeApproximately(p2, 0.1F);
            double.Parse(_rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLongitude.Minutes).Should().BeApproximately(p3, 0.00019F);
        }

        [When(@"Пользователь выбирает формат градусы")]
        public void WhenПользовательВыбираетФорматГрадусы()
        {
            _rtuUpdateViewModel.GpsInputViewModel.SelectedGpsInputModeComboItem =
                _rtuUpdateViewModel.GpsInputViewModel.GpsInputModes.First(i => i.Mode == GpsInputMode.Degrees);
        }

        [Then(@"Координаты в таком формате должны быть ""(.*)"" ""(.*)""")]
        public void ThenКоординатыВТакомФорматеДолжныБыть(double p0, double p1)
        {
            double.Parse(_rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLatitude.Degrees).Should().BeApproximately(p0, 0.0000019F);
            double.Parse(_rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLongitude.Degrees).Should().BeApproximately(p1, 0.0000019F);
        }

        [When(@"Пользователь выбирает формат градусы-минуты-секунды")]
        public void WhenПользовательВыбираетФорматГрадусы_Минуты_Секунды()
        {
            _rtuUpdateViewModel.GpsInputViewModel.SelectedGpsInputModeComboItem =
                _rtuUpdateViewModel.GpsInputViewModel.GpsInputModes.First(
                    i => i.Mode == GpsInputMode.DegreesMinutesAndSeconds);
        }

        [Then(@"Координаты должны быть ""(.*)"" ""(.*)"" ""(.*)"" ""(.*)"" ""(.*)"" ""(.*)""")]
        public void ThenКоординатыДолжныБыть(double p0, double p1, double p2, double p3, double p4, double p5)
        {
            double.Parse(_rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLatitude.Degrees).Should().BeApproximately(p0, 0.1F);
            double.Parse(_rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLatitude.Minutes).Should().BeApproximately(p1, 0.1F);
            double.Parse(_rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLatitude.Seconds).Should().BeApproximately(p2, 0.019F);
            double.Parse(_rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLongitude.Degrees).Should().BeApproximately(p3, 0.1F);
            double.Parse(_rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLongitude.Minutes).Should().BeApproximately(p4, 0.1F);
            double.Parse(_rtuUpdateViewModel.GpsInputViewModel.OneCoorViewModelLongitude.Seconds).Should().BeApproximately(p5, 0.019F);
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

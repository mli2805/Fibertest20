using System.IO;
using System.Windows;
using FluentAssertions;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RftsEventsViewModelSteps
    {
        private RftsEventsViewModel _vm;

        [When(@"Открываем форму событий для измерения (.*)")]
        public void WhenОткрываемФормуСобытийДляИзмерения(string filename)
        {
            var sorBytes = File.ReadAllBytes($@"..\..\Sut\MoniResults\{filename}.sor");
            var sorData = SorData.FromBytes(sorBytes);
            _vm = new RftsEventsViewModel(sorData);
        }

        [Then(@"Форма имеет 3 вкладки")]
        public void ThenФормаИмеет3Вкладки()
        {
            _vm.RftsEventsTableVisibility.Should().Be(Visibility.Visible);
            _vm.NoFiberLabelVisibility.Should().Be(Visibility.Collapsed);

            _vm.LevelsContent.IsMinorExists.Should().BeTrue();
            _vm.LevelsContent.IsMajorExists.Should().BeTrue();
            _vm.LevelsContent.IsCriticalExists.Should().BeTrue();
            _vm.LevelsContent.IsUsersExists.Should().BeFalse();
        }

        [Then(@"На всех вкладках по (.*) событий")]
        public void ThenНаВсехВкладкахПоСобытий(int p0)
        {
            _vm.LevelsContent.MinorLevelViewModel.BindableTable.Columns.Count.Should().Be(p0);

            _vm.LevelsContent.MinorLevelViewModel.OneLevelTableContent.IsFailed.Should().BeTrue();
            _vm.LevelsContent.MinorLevelViewModel.OneLevelTableContent.Table[104][4].Should().Be(@" L C");
            _vm.LevelsContent.MinorLevelViewModel.OneLevelTableContent.Table[104][6].Should().Be(@" R");

            _vm.LevelsContent.MajorLevelViewModel.OneLevelTableContent.IsFailed.Should().BeTrue();
            _vm.LevelsContent.MajorLevelViewModel.OneLevelTableContent.Table[104][6].Should().Be(@" R");

            _vm.LevelsContent.CriticalLevelViewModel.OneLevelTableContent.IsFailed.Should().BeTrue();
            _vm.LevelsContent.CriticalLevelViewModel.OneLevelTableContent.Table[104][6].Should().Be(@" R");
        }

        [Then(@"Все пороги состояние плохо")]
        public void ThenВсеПорогиСостояниеПлохо()
        {
            _vm.FooterViewModel.Minor.Should().StartWith(@"fail");
            _vm.FooterViewModel.Major.Should().StartWith(@"fail");
            _vm.FooterViewModel.Critical.Should().StartWith(@"fail");
        }


        [Then(@"Состояние трассы (.*)")]
        public void ThenСостояниеТрассы(string state)
        {
            _vm.FooterViewModel.TraceState.Should().StartWith(state);
        }

        [Then(@"Нет вкладок есть только надпись Нет волокна")]
        public void ThenНетВкладокЕстьТолькоНадписьНетВолокна()
        {
            _vm.RftsEventsTableVisibility.Should().Be(Visibility.Collapsed);
            _vm.NoFiberLabelVisibility.Should().Be(Visibility.Visible);
        }

    }
}

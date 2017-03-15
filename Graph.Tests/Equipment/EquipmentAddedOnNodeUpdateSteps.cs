using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class EquipmentAddedOnNodeUpdateSteps
    {
        private readonly SutForEquipmentAdded _sut = new SutForEquipmentAdded();
        private NodeUpdateViewModel _nodeUpdateViewModel;

        [Given(@"Через узел проходят три трассы")]
        public void GivenЧерезУзелПроходятТриТрассы()
        {
            _sut.SetThreeTraceThroughNode();
        }

        [Given(@"Пользователь открывает форму редактирования узла")]
        public void GivenПользовательОткрываетФормуРедактированияУзла()
        {
            _nodeUpdateViewModel = new NodeUpdateViewModel(_sut.NodeId, _sut.ReadModel, _sut.FakeWindowManager, _sut.ShellVm.Bus);
        }

        [When(@"Пользователь жмет добавить оборудование вводит парамы и сохраняет")]
        public void WhenПользовательЖметДобавитьОборудованиеВводитПарамыИСохраняет()
        {
            ScenarioContext.Current.Pending();
        }

    }
}

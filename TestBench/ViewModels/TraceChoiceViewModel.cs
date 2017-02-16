using System.Collections.Generic;
using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public class TraceChoiceViewModel : Screen
    {
        public string Caption { get; set; }
        public List<CheckBoxModel> Choices { get; set; } // for binding
        public bool ShouldWeContinue { get; set; }
        public bool IsClosed { get; set; }

        private List<TraceVm> _tracesInNode;
        public TraceChoiceViewModel(List<TraceVm> tracesInNode)
        {
            _tracesInNode = tracesInNode;
            InitializeChoices();
        }

        private void InitializeChoices()
        {
            Caption = "Укажите трассы, которые будут использовать добавляемое оборудование \n Если выбранная трасса уже содержит оборудование в данном узле, оно будет заменено";
            Choices = new List<CheckBoxModel>();
            foreach (var trace in _tracesInNode)
            {
                var checkBoxModel = new CheckBoxModel() { Id = trace.Id, Title = trace.Title, IsChecked = false, IsEnabled = !trace.HasBase};
                Choices.Add(checkBoxModel);
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Выбор трасс для оборудования";
        }

        public void Accept()
        {
            ShouldWeContinue = true;
            CloseView();
        }

        public void Cancel()
        {
            ShouldWeContinue = false;
            CloseView();
        }

        private void CloseView()
        {
            IsClosed = true;
            TryClose();
        }

    }
}

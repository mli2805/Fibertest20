using Caliburn.Micro;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public class QuestionViewModel : Screen
    {
        public string QuestionMessage { get; set; }
        public bool IsClosed { get; set; }

        public bool IsAnswerPositive { get; set; }
        public QuestionViewModel(string questionMessage)
        {
            QuestionMessage = questionMessage;
            IsAnswerPositive = false;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Question!";
        }

        public void OkButton()
        {
            IsAnswerPositive = true;
            CloseView();
        }

        public void CancelButton()
        {
            IsAnswerPositive = false;
            CloseView();
        }

        private void CloseView()
        {
            IsClosed = true;
            TryClose();
        }
    }
}

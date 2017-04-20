using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class QuestionViewModel : Screen
    {
        public string QuestionMessage { get; set; }
        public bool IsClosed { get; set; }

        public bool IsAnswerPositive { get; set; }
        public QuestionViewModel(string questionMessage)
        {
            QuestionMessage = questionMessage;
            IsAnswerPositive = true;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Question;
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
        /// <summary>Just for debug purposes </summary>
        [Localizable(false)]
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return "QuestionViewModel:" + QuestionMessage;
        }
    }
}

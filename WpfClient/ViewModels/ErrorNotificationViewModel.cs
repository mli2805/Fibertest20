using Caliburn.Micro;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public class ErrorNotificationViewModel : Screen
    {
        public string ErrorMessage { get; set; }
        public bool IsClosed { get; set; }

        public ErrorNotificationViewModel(string errorMessage)
        {
            ErrorMessage = errorMessage;
            IsClosed = false;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Error!";
        }

        public void CloseView()
        {
            IsClosed = true;
            TryClose();
        }
    }
}

using Caliburn.Micro;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public class NotificationViewModel : Screen
    {
        public string Message { get; set; }
        public bool IsClosed { get; set; }

        public NotificationViewModel(string message)
        {
            Message = message;
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

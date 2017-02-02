using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public class NotificationViewModel : Screen
    {
        private readonly string _caption;
        public string Message { get; set; }
        public bool IsClosed { get; set; }

        public NotificationViewModel(string caption, string message)
        {
            _caption = caption;
            Message = message;
            IsClosed = false;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _caption;
        }

        public void CloseView()
        {
            IsClosed = true;
            TryClose();
        }
    }
}

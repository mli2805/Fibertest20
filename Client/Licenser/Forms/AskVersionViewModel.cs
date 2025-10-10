using Caliburn.Micro;

namespace Iit.Fibertest.Licenser
{
    public class AskVersionViewModel : Screen
    {
        public string SelectedVersion { get; private set; }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "";
        }

        public void Ft2Button()
        {
            SelectedVersion = "2";
            TryClose();
        }
        
        public void Ft3Button()
        {
            SelectedVersion = "3";
            TryClose();
        }
    }
}

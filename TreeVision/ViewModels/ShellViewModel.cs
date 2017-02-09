using Caliburn.Micro;

namespace TreeVision {
    public class ShellViewModel : Screen, IShell
    {
        public LeftPanelViewModel MyLeftPanelViewModel { get; set; } = new LeftPanelViewModel();

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Fibertest 2.0";
        }
    }
}
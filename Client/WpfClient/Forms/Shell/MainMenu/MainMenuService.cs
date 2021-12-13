namespace Iit.Fibertest.Client
{
    public partial class MainMenuViewModel
    {

        public async void ExportEvents()
        {
           await _wcfDesktopC2D.ExportEvents();
        }
    }
}

using Caliburn.Micro;

namespace KadastrLoader 
{
    public class KadastrLoaderViewModel : Screen, IShell
    {
        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Load from Kadastr";
        }

        public void Close()
        {
            TryClose();
        }
    }
}
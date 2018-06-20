namespace Uninstall
{
    public class UninstallOperations
    {
        public void Do()
        {
            if (!UninstallServices()) return;
            if (!CleanRegistry()) return;
            DeleteFilesAndShortCuts();
        }

        private bool UninstallServices()
        {
            return true;
        }

        private bool CleanRegistry()
        {
            return true;
        }

        private void DeleteFilesAndShortCuts()
        {
        }
    }
}

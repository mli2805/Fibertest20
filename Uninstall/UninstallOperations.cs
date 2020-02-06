using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Uninstall
{
    public class UninstallOperations
    {
        private readonly Dictionary<string, string> _services = new Dictionary<string, string>()
        {
            {"FibertestDcService", "Fibertest 2.0 DataCenter Server"},
            {"FibertestRtuWatchdog", "Fibertest 2.0 RTU Watchdog"},
            {"FibertestRtuService", "Fibertest 2.0 RTU Manager"},
        };

        private readonly List<string> _sites = new List<string>() { "fibertest_web_api", "fibertest_web_client" };

        private readonly Dictionary<string, string> _componentFolders = new Dictionary<string, string>
        {
            {"Client", "bin"}, {"SuperClient", "bin"},
            {"DataCenter", "bin"}, {"RtuManager", "bin"},
            {"WebApi", "publish"}, {"WebClient", ""},
            {"RftsReflect", "" },
        };

        public bool Do(BackgroundWorker worker, string fibertestFolder, bool isFullUninstall)
        {
            worker.ReportProgress((int)BwReturnProgressCode.UninstallStarted);

            if (!UninstallServices(worker)) return false;
            UninstallSites(worker);

            if (!DeleteFiles(worker, fibertestFolder, isFullUninstall)) return false;
            ShortcutOperatios.DeleteAllShortcuts();
            worker.ReportProgress((int)BwReturnProgressCode.ShortcutsDeleted);

            RegistryOperations.RemoveFibertestBranch();
            worker.ReportProgress((int)BwReturnProgressCode.RegistryCleaned);

            worker.ReportProgress((int)BwReturnProgressCode.UninstallFinished);
            return true;
        }

        private bool UninstallServices(BackgroundWorker worker)
        {
            foreach (var pair in _services)
            {
                if (!ServiceOperations.UninstallServiceIfExist(pair.Key, pair.Value, worker))
                    return false;
            }
            return true;
        }

        private void UninstallSites(BackgroundWorker worker)
        {
            foreach (var site in _sites)
                IisOperations.DeleteWebsite(site, worker);
        }

        private bool DeleteFiles(BackgroundWorker worker, string fibertestFolder, bool isFullUninstall)
        {
            worker.ReportProgress((int)BwReturnProgressCode.DeletingFiles);
            try
            {
                foreach (var componentFolder in _componentFolders)
                {
                    var dir = fibertestFolder + $@"\{componentFolder.Key}";
                    if (!isFullUninstall && componentFolder.Value != "")
                        dir += $@"\{componentFolder.Value}";

                    if (!Directory.Exists(dir)) continue;

                    Thread.Sleep(500); // for slow RTU PC sake
                    Directory.Delete(dir, true);
                }

            }
            catch (Exception e)
            {
                worker.ReportProgress((int)BwReturnProgressCode.CannotDeleteSpecifiedFolder, e.Message);
                MessageBox.Show(Resources.SID_Cannot_delete_specified_folder_, Resources.SID_Error_, MessageBoxButton.OK);
                return false;
            }

            worker.ReportProgress((int)BwReturnProgressCode.FilesAreDeletedSuccessfully);
            return true;
        }

    }
}

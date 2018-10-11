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
       
        private readonly List<string> _componentFolders = new List<string>{"Client", "DataCenter", "RtuManager", "SuperClient"};

        public bool Do(BackgroundWorker worker, string fibertestFolder, bool isFullUninstall)
        {
            worker.ReportProgress((int)BwReturnProgressCode.UninstallStarted);

            if (!UninstallServices(worker)) return false;

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

        private bool DeleteFiles(BackgroundWorker worker, string fibertestFolder, bool isFullUninstall)
        {
            worker.ReportProgress((int)BwReturnProgressCode.DeletingFiles);
            try
            {
                foreach (var componentFolder in _componentFolders)
                {
                    var dir = isFullUninstall
                        ? fibertestFolder + $@"\{componentFolder}"
                        : fibertestFolder + $@"\{componentFolder}\bin";

                    if (!Directory.Exists(dir)) continue;

                    Thread.Sleep(500); // for slow RTU PC sake
                    Directory.Delete(dir, true);
                }

                if (Directory.Exists(fibertestFolder + @"\RftsReflect"))
                    Directory.Delete(fibertestFolder + @"\RftsReflect", true);

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

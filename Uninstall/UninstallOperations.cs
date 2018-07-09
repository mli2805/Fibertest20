using System;
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
        private const string DataCenterServiceName = "FibertestDcService";
        private const string DataCenterDisplayName = "Fibertest 2.0 DataCenter Server";

        private const string RtuManagerServiceName = "FibertestRtuService";
        private const string RtuManagerDisplayName = "Fibertest 2.0 RTU Manager";

        private const string RtuWatchdogServiceName = "FibertestRtuWatchdog";
        private const string RtuWatchdogDisplayName = "Fibertest 2.0 RTU Watchdog";

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
            if (!ServiceOperations.UninstallServiceIfExist(DataCenterServiceName, DataCenterDisplayName, worker))
                return false;
            if (!ServiceOperations.UninstallServiceIfExist(RtuWatchdogServiceName, RtuWatchdogDisplayName, worker))
                return false;
            if (!ServiceOperations.UninstallServiceIfExist(RtuManagerServiceName, RtuManagerDisplayName, worker))
                return false;
            return true;
        }

        private bool DeleteFiles(BackgroundWorker worker, string fibertestFolder, bool isFullUninstall)
        {
            worker.ReportProgress((int)BwReturnProgressCode.DeletingFiles);
            try
            {
                if (isFullUninstall)
                {
                    if (Directory.Exists(fibertestFolder + @"\Client"))
                        Directory.Delete(fibertestFolder + @"\Client", true);

                    if (Directory.Exists(fibertestFolder + @"\DataCenter"))
                        Directory.Delete(fibertestFolder + @"\DataCenter", true);

                    if (Directory.Exists(fibertestFolder + @"\RftsReflect"))
                        Directory.Delete(fibertestFolder + @"\RftsReflect", true);

                    if (Directory.Exists(fibertestFolder + @"\RtuManager"))
                    {
                        Thread.Sleep(500);
                        Directory.Delete(fibertestFolder + @"\RtuManager", true);
                    }
                }
                else
                {
                    if (Directory.Exists(fibertestFolder + @"\Client\bin"))
                        Directory.Delete(fibertestFolder + @"\Client\bin", true);

                    if (Directory.Exists(fibertestFolder + @"\DataCenter\bin"))
                        Directory.Delete(fibertestFolder + @"\DataCenter\bin", true);

                    if (Directory.Exists(fibertestFolder + @"\RftsReflect"))
                        Directory.Delete(fibertestFolder + @"\RftsReflect", true);

                    if (Directory.Exists(fibertestFolder + @"\RtuManager\bin"))
                    {
                        Thread.Sleep(500);
                        Directory.Delete(fibertestFolder + @"\RtuManager\bin", true);
                    }
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

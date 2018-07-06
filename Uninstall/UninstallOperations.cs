using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
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

        public void Do(BackgroundWorker worker, string fibertestFolder, bool isFullUninstall)
        {
            worker.ReportProgress(0, "Uninstall started...");

            if (!UninstallServices(worker)) return;

            if (!DeleteFiles(worker, fibertestFolder, isFullUninstall)) return;
            ShortcutOperatios.DeleteAllShortcuts();
            worker.ReportProgress(0, "Shortcuts deleted.");

            RegistryOperations.RemoveFibertestBranch();
            worker.ReportProgress(0, "Registry cleaned.");

            worker.ReportProgress(0, "Uninstall finished.");
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
            worker.ReportProgress(0, "Deleting files...");
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
                worker.ReportProgress(0, "Cannot delete specified folder!");
                worker.ReportProgress(0, e.Message);
                MessageBox.Show("Cannot delete specified folder!", "Error!", MessageBoxButton.OK);
                return false;
            }

            worker.ReportProgress(0, "Files are deleted successfully.");
            return true;
        }

    }
}

using System;
using System.Collections.ObjectModel;
using System.IO;
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

        public void Do(ObservableCollection<string> progressLines, string fibertestFolder, bool isFullUninstall)
        {
            progressLines.Add("Uninstall started...");

            if (!UninstallServices(progressLines)) return;

            if (!DeleteFiles(progressLines, fibertestFolder, isFullUninstall)) return;
            ShortcutOperatios.DeleteAllShortcuts();
            progressLines.Add("Shortcuts deleted.");

            RegistryOperations.RemoveFibertestBranch();
            progressLines.Add("Registry cleaned.");

            progressLines.Add("Uninstall finished.");
        }

        private bool UninstallServices(ObservableCollection<string> progressLines)
        {
            if (!ServiceOperations.UninstallServiceIfExist(DataCenterServiceName, DataCenterDisplayName, progressLines))
                return false;
            if (!ServiceOperations.UninstallServiceIfExist(RtuWatchdogServiceName, RtuWatchdogDisplayName, progressLines))
                return false;
            if (!ServiceOperations.UninstallServiceIfExist(RtuManagerServiceName, RtuManagerDisplayName, progressLines))
                return false;
            return true;
        }

        private bool DeleteFiles(ObservableCollection<string> progressLines, string fibertestFolder, bool isFullUninstall)
        {
            progressLines.Add("Deleting files...");
            try
            {
                if (isFullUninstall)
                {
                    if (Directory.Exists(fibertestFolder + @"\Client"))
                        Directory.Delete(fibertestFolder + @"\Client", true);

                    if (Directory.Exists(fibertestFolder + @"\DataCenter"))
                        Directory.Delete(fibertestFolder + @"\DataCenter", true);

                    if (Directory.Exists(fibertestFolder + @"\RtuManager"))
                        Directory.Delete(fibertestFolder + @"\RtuManager", true);
                }
                else
                {
                    if (Directory.Exists(fibertestFolder + @"\Client\bin"))
                        Directory.Delete(fibertestFolder + @"\Client\bin", true);

                    if (Directory.Exists(fibertestFolder + @"\DataCenter\bin"))
                        Directory.Delete(fibertestFolder + @"\DataCenter\bin", true);

                    if (Directory.Exists(fibertestFolder + @"\RtuManager\bin"))
                        Directory.Delete(fibertestFolder + @"\RtuManager\bin", true);
                }
            }
            catch (Exception e)
            {
                progressLines.Add("Cannot delete specified folder!");
                progressLines.Add(e.Message);
                MessageBox.Show("Cannot delete specified folder!", "Error!", MessageBoxButton.OK);
                return false;
            }

            progressLines.Add("Files are deleted successfully.");
            return true;
        }

    }
}

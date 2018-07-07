using System;
using System.ComponentModel;
using System.Globalization;
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

        public bool Do(BackgroundWorker worker, string fibertestFolder, bool isFullUninstall, CultureInfo culture)
        {
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            worker.ReportProgress(0, Resources.SID_Uninstall_started___);

            if (!UninstallServices(worker)) return false;

            if (!DeleteFiles(worker, fibertestFolder, isFullUninstall)) return false;
            ShortcutOperatios.DeleteAllShortcuts();
            worker.ReportProgress(0, Resources.SID_Shortcuts_deleted_);

            RegistryOperations.RemoveFibertestBranch();
            worker.ReportProgress(0, Resources.SID_Registry_cleaned_);

            worker.ReportProgress(0, Resources.SID_Uninstall_finished_);
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
            worker.ReportProgress(0, Resources.SID_Deleting_files___);
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
                worker.ReportProgress(0, Resources.SID_Cannot_delete_specified_folder_);
                worker.ReportProgress(0, e.Message);
                MessageBox.Show(Resources.SID_Cannot_delete_specified_folder_, Resources.SID_Error_, MessageBoxButton.OK);
                return false;
            }

            worker.ReportProgress(0, Resources.SID_Files_are_deleted_successfully_);
            return true;
        }

    }
}

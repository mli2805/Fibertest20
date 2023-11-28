using System;
using System.Windows.Forms;
using Iit.Fibertest.StringResources;
using Microsoft.Win32;

namespace Iit.Fibertest.UtilsLib
{

/*
 * Computer\HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\FibertestRtuService
 * Computer\HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\FibertestRtuService
 */

    public static class RegistryOperations
    {
        private const string UserRoot = "HKEY_LOCAL_MACHINE";
        private const string FibertestBranch = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Iit\FiberTest20\";
        private const string ServicesBranch = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\";

        public static string GetPreviousInstallationCulture()
        {
            return (string)Registry.GetValue(UserRoot + "\\" + FibertestBranch, "Culture", "");
        }

        public static string GetFibertestValue(string key, string defaultValue)
        {
            return (string)Registry.GetValue(UserRoot + "\\" + FibertestBranch, key, defaultValue);
        }

        public static void SetServiceDescription(string serviceName, string description)
        {
            Registry.SetValue(ServicesBranch + serviceName, "Description", description, RegistryValueKind.String);
        }

        public static void SetRestartAsServiceFailureActions(string serviceName)
        {
            byte[] data = new byte[44];
            data[12] = 3;
            data[16] = 0x14;
            data[20] = 1;
            data[28] = 1;
            data[36] = 1;
            Registry.SetValue(ServicesBranch + serviceName, "FailureActions", data, RegistryValueKind.Binary);
        }

        public static void SaveSetupCultureInRegistry(string culture)
        {
            try
            {
                var result = Registry.LocalMachine.CreateSubKey(FibertestBranch);
                result?.SetValue("Culture", culture);
            }
            catch (Exception e)
            {
                MessageBox.Show(Resources.SID_Save_in_Registry_problem_ + e.Message);
            }

        }

        public static void SaveFibertestValue(string key, string value)
        {
            try
            {
                var result = Registry.LocalMachine.CreateSubKey(FibertestBranch);
                result?.SetValue(key, value);
            }
            catch (Exception e)
            {
                MessageBox.Show(Resources.SID_Save_in_Registry_problem_ + $@"({key}:{value})" + e.Message);
            }
        }

        public static void RemoveFibertestBranch()
        {
            Registry.LocalMachine.DeleteSubKeyTree(FibertestBranch, false);
        }

        public static int CheckIisVersion()
        {
            RegistryKey iisKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\InetStp", false);
            if (iisKey == null) return -1;
            var res = (int)iisKey.GetValue("MajorVersion");
            return res;
        }
    }
}

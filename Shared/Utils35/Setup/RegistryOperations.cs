using System;
using System.Windows.Forms;
using Iit.Fibertest.StringResources;
using Microsoft.Win32;

namespace Iit.Fibertest.UtilsLib
{
    public static class RegistryOperations
    {
        private const string UserRoot = "HKEY_LOCAL_MACHINE";
        private const string FibertestBranch = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Iit\FiberTest20\";

        public static string GetPreviousInstallationCulture()
        {
            return (string)Registry.GetValue(UserRoot + "\\" + FibertestBranch, "Culture", "");
        }

        public static string GetFibertestValue(string key, string defaultValue)
        {
            return (string)Registry.GetValue(UserRoot + "\\" + FibertestBranch, key, defaultValue);
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
                MessageBox.Show(Resources.SID_Save_in_Registry_problem_ + e.Message);
            }
        }

        public static void RemoveFibertestBranch()
        {
            Registry.LocalMachine.DeleteSubKeyTree(FibertestBranch, false);
        }
    }
}

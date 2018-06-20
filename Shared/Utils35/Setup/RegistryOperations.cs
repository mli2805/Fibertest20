using Microsoft.Win32;

namespace Iit.Fibertest.UtilsLib
{
    public static class RegistryOperations
    {
        const string UserRoot = "HKEY_LOCAL_MACHINE";
        const string RegistryBranch = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Iit\FiberTest20\";

        public static string GetPreviousInstallationCulture()
        {
            return (string)Registry.GetValue(UserRoot + "\\" + RegistryBranch, "Culture", "");
        }

        public static string GetFibertestValue(string key, string defaultValue)
        {
            return (string)Registry.GetValue(UserRoot + "\\" + RegistryBranch, key, defaultValue);
        }

        public static void SaveSetupCultureInRegistry(string culture)
        {
            var result = Registry.LocalMachine.CreateSubKey(RegistryBranch);
            result?.SetValue("Culture", culture);
        }

        public static void SaveFibertestValue(string key, string value)
        {
            var result = Registry.LocalMachine.CreateSubKey(RegistryBranch);
            result?.SetValue(key, value);
        }
    }
}

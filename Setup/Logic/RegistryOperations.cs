using Microsoft.Win32;

namespace Setup
{
    public static class RegistryOperations
    {
        const string UserRoot = "HKEY_LOCAL_MACHINE";
        const string RegistryBranch = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Iit\FiberTest20\";

        public static string GetPreviousInstallationCulture()
        {
            return (string)Registry.GetValue(UserRoot + "\\" + RegistryBranch, "Culture", "");
        }

        public static void SaveSetupCultureInRegistry(string culture)
        {
            var result = Registry.LocalMachine.CreateSubKey(RegistryBranch);
            result?.SetValue("Culture", culture);
        }
    }
}

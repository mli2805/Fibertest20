using System;
using System.IO;
using IWshRuntimeLibrary;

namespace Iit.Fibertest.UtilsLib
{
    public static class ShortcutOperatios
    {
        private static string _clientLnk = @"FtClient20.lnk";
        private static string _uninstallLnk = @"FtUninstall20.lnk";
        public static void CreateClientShortcut(string fullClientPath)
        {
            object shDesktop = "Desktop";
            WshShell shell = new WshShell();
            string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + "\\" + _clientLnk;
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.Description = "Fibertest 2.0 Client";
            shortcut.TargetPath = fullClientPath + @"\Iit.Fibertest.Client.exe";
            shortcut.IconLocation = fullClientPath + @"\Iit.Fibertest.Client.exe";
            shortcut.Save();
        }
        public static void CreateUninstallShortcut(string fullUninstallPath)
        {
            object shDesktop = "Desktop";
            WshShell shell = new WshShell();
            string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + "\\" + _uninstallLnk;
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.Description = "Fibertest 2.0 Uninstall";
            shortcut.TargetPath = fullUninstallPath + @"\Iit.Fibertest.Uninstall.exe";
            shortcut.IconLocation = fullUninstallPath + @"\Iit.Fibertest.Uninstall.exe";
            shortcut.Save();
        }

        public static void DeleteAllShortcuts()
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            System.IO.File.Delete(Path.Combine(desktopPath, _clientLnk));
            System.IO.File.Delete(Path.Combine(desktopPath, _uninstallLnk));
        }
    }
}
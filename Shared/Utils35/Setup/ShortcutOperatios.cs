﻿using System;
using System.IO;
using IWshRuntimeLibrary;

namespace Iit.Fibertest.UtilsLib
{
    public static class ShortcutOperatios
    {
        private static string _clientLnk = @"FtClient20.lnk";
        private static string _reflectLnk = @"RftsReflect.lnk";
        private static string _superclientLnk = @"FtSuperClient.lnk";
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
        public static void CreateSuperClientShortcut(string fullSuperClientPath)
        {
            object shDesktop = "Desktop";
            WshShell shell = new WshShell();
            string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + "\\" + _superclientLnk;
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.Description = "Fibertest 2.0 Super Client";
            shortcut.TargetPath = fullSuperClientPath + @"\Iit.Fibertest.SuperClient.exe";
            shortcut.IconLocation = fullSuperClientPath + @"\Iit.Fibertest.SuperClient.exe";
            shortcut.Save();
        }

        public static void CreateReflectShortcut(string fullReflectPath)
        {
            object shDesktop = "Desktop";
            WshShell shell = new WshShell();
            string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + "\\" + _reflectLnk;
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.Description = "Fibertest 2.0 Reflect";
            shortcut.TargetPath = fullReflectPath + @"\reflect.exe";
            shortcut.IconLocation = fullReflectPath + @"\reflect.exe";
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
            System.IO.File.Delete(Path.Combine(desktopPath, _superclientLnk));
            System.IO.File.Delete(Path.Combine(desktopPath, _reflectLnk));
            System.IO.File.Delete(Path.Combine(desktopPath, _uninstallLnk));
        }
    }
}
using System.Collections.ObjectModel;
using System.IO;
using IWshRuntimeLibrary;

namespace Setup
{
    public static class SetupOperations
    {
       
        public static void CreateShortcuts(string clientPath)
        {
            object shDesktop = "Desktop";
            WshShell shell = new WshShell();
            string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\FtClient20.lnk";
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.Description = "Fibertest 2.0 Client";
            shortcut.TargetPath = clientPath + @"\Iit.Fibertest.Client.exe";
            shortcut.IconLocation = clientPath + @"\Iit.Fibertest.Client.exe";
            shortcut.Save();
        }

        public static bool DirectoryCopy(string sourceDirName, string destDirName, ObservableCollection<string> progressLines)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                progressLines.Add("Error! Source folder not found!");
                return false;
            }

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                progressLines.Add(temppath);
                file.CopyTo(temppath, true);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, progressLines);
            }

            return true;
        }

    }
}
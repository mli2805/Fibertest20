using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace Iit.Fibertest.UtilsLib
{
    public static class FileOperations
    {
      

        public static bool DirectoryCopyWithDecorations(string sourceDirName, string destDirName,
            BackgroundWorker worker)
        {
            worker.ReportProgress(0, "Files are copied...");

            var currentDomain = AppDomain.CurrentDomain.BaseDirectory;
            var fullSourcePath = Path.Combine(currentDomain, sourceDirName);
            var result = DirectoryCopyRecursively(fullSourcePath, destDirName, worker);
            if (result)
                worker.ReportProgress(0, "Files are copied successfully.");
            return result;
        }

        private static bool DirectoryCopyRecursively(string sourceDirName, string destDirName, BackgroundWorker worker)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                worker.ReportProgress(0, $"Error! Source folder {sourceDirName} not found!");
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
                var ss = file.CopyTo(temppath, true);
                worker.ReportProgress(0, ss.Name);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopyRecursively(subdir.FullName, temppath, worker);
            }

            return true;
        }

    }
}
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace Iit.Fibertest.UtilsLib
{
    public static class FileOperations
    {
      

        public static int DirectoryCopyWithDecorations(string sourceDirName, string destDirName,
            ObservableCollection<string> progressLines)
        {
            progressLines.Add("Files are copied...");

            var currentDomain = AppDomain.CurrentDomain.BaseDirectory;
            var fullSourcePath = Path.Combine(currentDomain, sourceDirName);
            var result = DirectoryCopyRecursively(fullSourcePath, destDirName, progressLines);
            if (result > -1)
                progressLines.Add("Files are copied successfully.");
            return result;
        }

        private static int DirectoryCopyRecursively(string sourceDirName, string destDirName, ObservableCollection<string> progressLines)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                progressLines.Add("Error! Source folder not found!");
                return -1;
            }

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            var count = 0;
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                var ss = file.CopyTo(temppath, true);
                progressLines.Add(ss.Name);
                count++;
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopyRecursively(subdir.FullName, temppath, progressLines);
            }

            return count;
        }

    }
}
﻿using System;
using System.ComponentModel;
using System.IO;

namespace Iit.Fibertest.UtilsLib
{
    public static class FileOperations
    {
        public static string GetParentFolder(string path)
        {
            var index = path.Substring(0, path.Length - 1).LastIndexOf(@"\", StringComparison.CurrentCulture);
            return path.Substring(0, index);
        }

        public static bool DirectoryCopyWithDecorations(string sourceDirName, string destDirName,
            BackgroundWorker worker)
        {
            var currentDomain = AppDomain.CurrentDomain.BaseDirectory;
            var fullSourcePath = Path.Combine(currentDomain, sourceDirName);
            try
            {
                return DirectoryCopyRecursively(fullSourcePath, destDirName, worker);
            }
            catch (Exception e)
            {
                worker.ReportProgress((int)BwReturnProgressCode.CopyFilesError, e.Message);
                return false;
            }
        }

        private static bool DirectoryCopyRecursively(string sourceDirName, string destDirName, BackgroundWorker worker)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                worker.ReportProgress((int)BwReturnProgressCode.ErrorSourceFolderNotFound, sourceDirName);
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
                worker.ReportProgress(1, ss.Name);
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
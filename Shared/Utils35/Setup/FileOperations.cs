using System.Collections.ObjectModel;
using System.IO;

namespace Iit.Fibertest.UtilsLib
{
    public static class FileOperations
    {
      

        public static bool DirectoryCopyWithDecorations(string sourceDirName, string destDirName,
            ObservableCollection<string> progressLines)
        {
            progressLines.Add("Files are copied...");
            var result = DirectoryCopyRecursively(sourceDirName, destDirName, progressLines);
            if (result)
                progressLines.Add("Files are copied successfully.");
            return result;
        }

        private static bool DirectoryCopyRecursively(string sourceDirName, string destDirName, ObservableCollection<string> progressLines)
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
                DirectoryCopyRecursively(subdir.FullName, temppath, progressLines);
            }

            return true;
        }

    }
}
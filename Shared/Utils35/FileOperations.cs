using System;
using System.IO;
using System.Reflection;

namespace Iit.Fibertest.UtilsLib
{
    public static class FileOperations
    {
        public static string GetMainFolder()
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyPath = Path.GetDirectoryName(assemblyLocation)!;
            return Directory.GetParent(assemblyPath)!.FullName;
        }

        public static string GetParentFolder(string path, int depth = 1)
        {
            for (int i = 0; i < depth; i++)
            {
                var index = path.Substring(0, path.Length - 1).LastIndexOf(@"\", StringComparison.CurrentCulture);
                if (index == -1) return string.Empty;
                path = path.Substring(0, index);
            }
            return path;
        }
    }
}
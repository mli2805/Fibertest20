using System.IO;

namespace Iit.Fibertest.UtilsLib
{
    public enum DestinationComputer
    {
        DataCenter,
        Rtu
    }
    public class FtService
    {
        public readonly string Name;
        public readonly string DisplayName;
        public string Description => DisplayName;

        public string SourcePath;
        public string FolderInsideFibertest;
        public string FolderForBinaries;
        public string MainExe;
        public DestinationComputer DestinationComputer;

        public FtService(string name, string displayName)
        {
            Name = name;
            DisplayName = displayName;
        }

        public string GetFullBinariesFolder(string installationFolder)
        {
            var serviceFolder = Path.Combine(installationFolder, FolderInsideFibertest);
            return Path.Combine(serviceFolder, FolderForBinaries);
        }

        public string GetFullFilename(string installationFolder)
        {
            var serviceFolder = Path.Combine(installationFolder, FolderInsideFibertest);
            var binaryFolder = Path.Combine(serviceFolder, FolderForBinaries);
            return Path.Combine(binaryFolder, MainExe);
        }
            
    }
}
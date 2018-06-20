using System.Collections.ObjectModel;
using System.IO;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Setup
{
    public class SetupDataCenterOperations
    {
        private const string DataCenterServiceName = "FibertestDcService";
        private const string DataCenterDisplayName = "Fibertest 2.0 DataCenter Server";
        private const string DataCenterServiceDescription = "Fibertest 2.0 DataCenter Server Service";

        private const string SourcePathDataCenter = @"..\DcFiles";
        private const string DataCenterSubdir = @"DataCenter\bin";
        private const string ServiceFilename = @"Iit.Fibertest.DataCenterService.exe";

        public bool SetupDataCenter(ObservableCollection<string> progressLines, string installationFolder)
        {
            var fullDataCenterPath = Path.Combine(installationFolder, DataCenterSubdir);

            progressLines.Add("Data Center setup started.");
            if (!ServiceOperations.UninstallServiceIfExist(DataCenterServiceName, DataCenterDisplayName, progressLines))
                return false;

            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathDataCenter, fullDataCenterPath, progressLines))
                return false;

            var filename = Path.Combine(fullDataCenterPath, ServiceFilename);
            if (!ServiceOperations.InstallService(DataCenterServiceName, 
                DataCenterDisplayName, DataCenterServiceDescription, filename, progressLines)) return false;

            progressLines.Add("Data Center setup completed successfully.");
            return true;
        }
    }
}
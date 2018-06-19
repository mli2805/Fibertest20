using System.Collections.ObjectModel;
using System.IO;
using Iit.Fibertest.UtilsLib;

namespace Setup
{
    public class SetupDatacenterOperations
    {
        private const string DataCenterServiceName = "FibertestDcService";
        private const string DataCenterDisplayName = "Fibertest 2.0 DataCenter Server";
        private const string DataCenterServiceDescription = "Fibertest 2.0 DataCenter Server Service";

        private const string SourcePathDatacenter = @"..\DcFiles";
        private const string TargetPathDatacenter = @"C:\IIT-Fibertest\DataCenter\bin";
        private const string ServiceFilename = @"Iit.Fibertest.DataCenterService.exe";

        public bool SetupDataCenter(ObservableCollection<string> progressLines)
        {
            progressLines.Add("Data Center setup started.");
            if (!ServiceOperations.UninstallServiceIfNeeded(DataCenterServiceName, DataCenterDisplayName, progressLines))
                return false;

            if (!FileOperations.DirectoryCopyWithDecorations(SourcePathDatacenter, TargetPathDatacenter, progressLines))
                return false;

            var filename = Path.Combine(TargetPathDatacenter, ServiceFilename);
            if (!ServiceOperations.InstallService(DataCenterServiceName, 
                DataCenterDisplayName, DataCenterServiceDescription, filename, progressLines)) return false;

            progressLines.Add("Data Center setup completed successfully.");
            return true;
        }

     
    }
}
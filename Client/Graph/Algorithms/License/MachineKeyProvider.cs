using System;
using System.Management;

namespace Iit.Fibertest.Graph
{
    public interface IMachineKeyProvider
    {
        string Get();
    }
    public class MachineKeyProvider : IMachineKeyProvider
    {
        public string Get()
        {
            var cpuId = GetCpuId();
            var mbSerial = GetMotherBoardSerial();
            var ddSerial = GetDiskDriveSerial();
            return cpuId + mbSerial + ddSerial;
        }

        private string GetCpuId()
        {
            try
            {
                var managementClass = new ManagementClass(@"win32_processor");
                ManagementObjectCollection managementObjectCollection = managementClass.GetInstances();

                foreach (var o in managementObjectCollection)
                {
                    var managementObject = (ManagementObject) o;
                    return managementObject.Properties[@"processorID"].Value.ToString();
                }

            }
            catch (Exception)
            {
                return @"ExceptionWhileGettingCpuId";
            }
            return @"NoCpuId";
        }

        private static string GetMotherBoardSerial()
        {
            try
            {
                var managementClass = new ManagementClass(@"win32_baseboard");
                ManagementObjectCollection managementObjectCollection = managementClass.GetInstances();

                foreach (var o in managementObjectCollection)
                {
                    var managementObject = (ManagementObject) o;
                    return managementObject.Properties[@"SerialNumber"].Value.ToString();
                }
            }
            catch (Exception )
            {
                return @"ExceptionWhileGettingMotherBoardSerial";
            }

            return @"NoMotherBoardSerial";
        }  
        
        private static string GetDiskDriveSerial()
        {
            try
            {
                var managementClass = new ManagementClass(@"win32_DiskDrive");
                ManagementObjectCollection managementObjectCollection = managementClass.GetInstances();

                foreach (var o in managementObjectCollection)
                {
                    var managementObject = (ManagementObject) o;
                    return managementObject.Properties[@"SerialNumber"].Value.ToString();
                }
            }
            catch (Exception )
            {
                return @"ExceptionWhileGettingDiskDriveSerial";
            }

            return @"NoDiskDriveSerial";
        }
    }
}

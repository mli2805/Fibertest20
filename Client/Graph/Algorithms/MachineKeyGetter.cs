using System;
using System.Management;

namespace Iit.Fibertest.Graph
{
    public static class MachineKeyGetter
    {
        public static string GetMachineKey()
        {
            var cpuId = GetCpuId();
            var mbSerial = GetMotherBoardSerial();
            var ddSerial = GetDiskDriveSerial();
            var machineName = Environment.MachineName;
            return cpuId + mbSerial + ddSerial + machineName;
        }

        private static string GetCpuId()
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

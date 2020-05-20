using System;
using System.IO;

namespace Iit.Fibertest.UtilsLib
{
    public static class IniOperations
    {
        private const string DataCenterIniSubdir = @"DataCenter\ini";
        public static void SaveMysqlTcpPort(string installationFolder, string mysqlTcpPort, string bindingProtocol)
        {
            var iniDataCenterPath = Path.Combine(installationFolder, DataCenterIniSubdir);

            var iniFile = new IniFile();
            var iniFileName = Utils.FileNameForSure(iniDataCenterPath, "DataCenter.ini",
                false, true);
            iniFile.AssignFile(iniFileName, true);

            iniFile.Write(IniSection.MySql, IniKey.MySqlTcpPort, mysqlTcpPort);
            iniFile.Write(IniSection.WebApi, IniKey.BindingProtocol, bindingProtocol);
        }
        
        public static string GetMysqlTcpPort(string installationFolder)
        {
            try
            {
                var iniDataCenterPath = Path.Combine(installationFolder, DataCenterIniSubdir);

                var iniFile = new IniFile();
                var iniFileName = Utils.FileNameForSure(iniDataCenterPath, "DataCenter.ini",
                    false, true);
                iniFile.AssignFile(iniFileName, true);

                return iniFile.Read(IniSection.MySql, IniKey.MySqlTcpPort, "3306");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "error";
            }
        }
    }
}
using System.IO;

namespace Iit.Fibertest.UtilsLib
{
    public static class IniOperations
    {
        private const string DataCenterIniSubdir = @"DataCenter\ini";
        public static void SaveMysqlTcpPort(string installationFolder, string mysqlTcpPort)
        {
            var iniDataCenterPath = Path.Combine(installationFolder, DataCenterIniSubdir);

            var iniFile = new IniFile();
            var iniFileName = Utils.FileNameForSure(iniDataCenterPath, "DataCenter.ini",
                false, true);
            iniFile.AssignFile(iniFileName, true);

            iniFile.Write(IniSection.MySql, IniKey.MySqlTcpPort, mysqlTcpPort);
        }
        
        public static string GetMysqlTcpPort(string installationFolder)
        {
            var iniDataCenterPath = Path.Combine(installationFolder, DataCenterIniSubdir);

            var iniFile = new IniFile();
            var iniFileName = Utils.FileNameForSure(iniDataCenterPath, "DataCenter.ini",
                false, true);
            iniFile.AssignFile(iniFileName, true);

            return iniFile.Read(IniSection.MySql, IniKey.MySqlTcpPort, "3306");
        }
    }
}
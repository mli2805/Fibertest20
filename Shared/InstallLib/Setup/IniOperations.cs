using System;
using System.IO;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.InstallLib
{
    public static class IniOperations
    {
        private const string DataCenterIniSubdir = @"DataCenter\ini";
        public static void SetParamsIntoServerIniFile(CurrentInstallation currentInstallation)
        {
            var webApiBindingProtocol = currentInstallation.IsWebNeeded
                ? currentInstallation.IsWebByHttps
                    ? "https"
                    : "http"
                : "none";

            var iniDataCenterPath = Path.Combine(currentInstallation.InstallationFolder, DataCenterIniSubdir);

            var iniFile = new IniFile();
            var iniFileName = Utils.FileNameForSure(iniDataCenterPath, "DataCenter.ini",
                false, true);
            iniFile.AssignFile(iniFileName, true);

            iniFile.Write(IniSection.MySql, IniKey.MySqlTcpPort, currentInstallation.MySqlTcpPort);
            iniFile.Write(IniSection.WebApi, IniKey.DomainName,
                string.IsNullOrEmpty(currentInstallation.SslCertificateDomain) ? "localhost" : currentInstallation.SslCertificateDomain);
            iniFile.Write(IniSection.WebApi, IniKey.BindingProtocol, webApiBindingProtocol);
        }

        public static string GetMysqlTcpPort(string installationFolder)
        {
            try
            {
                var iniDataCenterPath = Path.Combine(installationFolder, DataCenterIniSubdir);

                // if DataCenter folder with ini does not exist it will be create only during installation (not before)
                if (!File.Exists(iniDataCenterPath + @"\DataCenter.ini"))
                {
                    return "3306";
                }

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

        public static void SetParamIntoClientIniFile(string fullClientFolder, bool isHighDensityGraph)
        {
            var iniClientPath = Path.Combine(fullClientFolder, @"ini");
            var iniFile = new IniFile();
            var iniFileName = Utils.FileNameForSure(iniClientPath, "Client.ini",
                false, true);
            iniFile.AssignFile(iniFileName, true);

            iniFile.Write(IniSection.Map, IniKey.IsHighDensityGraph, isHighDensityGraph);
            var thresholdZoom = iniFile.Read(IniSection.Map, IniKey.ThresholdZoom, isHighDensityGraph ? 16 : 12);
            if (isHighDensityGraph)
            {
                if (thresholdZoom < 14)
                    iniFile.Write(IniSection.Map, IniKey.ThresholdZoom, 16);
            }
            else
            {
                if (thresholdZoom > 12 || thresholdZoom < 10)
                    iniFile.Write(IniSection.Map, IniKey.ThresholdZoom, 12);
            }
        }

        public static bool GetIsHighDensityGraph(CurrentInstallation currentInstallation)
        {
            try
            {
                var iniClientPath = Path.Combine(currentInstallation.FullClientFolder, "ini");
                if (!File.Exists(iniClientPath + @"\Client.ini"))
                {
                    return false;
                }

                var iniFile = new IniFile();
                var iniFileName = Utils.FileNameForSure(iniClientPath, "Client.ini",
                    false, true);
                iniFile.AssignFile(iniFileName, true);

                return iniFile.Read(IniSection.Map, IniKey.IsHighDensityGraph, false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
}
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Install
{
    public class CurrentInstallation
    {
        public string ProductName { get; set; }
        public string ProductVersion { get; set; }

        public string MainName => $"{ProductName} {ProductVersion}";

        public string InstallationFolder { get; set; }
        public InstallationType InstallationType { get; set; }

        public string MySqlTcpPort { get; set; }
        public bool IsWebNeeded { get; set; }
        public bool IsWebByHttps { get; set; }
        public string SslCertificateName { get; set; }
        public string SslCertificatePath { get; set; }
        public string SslCertificatePassword { get; set; }

        public string GetApiSettingsJson()
        {
            return new WebClientSettings()
            {
                ApiProtocol = IsWebByHttps
                    ? "https"
                    : "http",
                ApiPort = (int)TcpPorts.WebApiListenTo,
                Version = ProductVersion,
                SslCertificateName = SslCertificateName,
                SslCertificatePath = SslCertificatePath,
                SslCertificatePassword = SslCertificatePassword,
            }
            .ToCamelCaseJson();
        }
    }
    public class WebClientSettings
    {
        public string ApiProtocol;
        public int ApiPort;
        public string Version;

        public string SslCertificateName;
        public string SslCertificatePath;
        public string SslCertificatePassword;
    }
}

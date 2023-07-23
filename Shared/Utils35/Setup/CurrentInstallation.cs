namespace Iit.Fibertest.UtilsLib
{
    public class CurrentInstallation
    {
        public string ProductName;
        public string ProductVersion;

        public string MainName => $"{ProductName} {ProductVersion}";

        public string InstallationFolder;
        public InstallationType InstallationType;

        public string ClientInstallationFolder = "Client";

        public bool IsHighDensityGraph;

        public string MySqlTcpPort;
        public bool IsWebNeeded;
        public bool IsWebByHttps;
        public string SslCertificateName;
        public string SslCertificatePath;
        public string SslCertificatePassword;
        public string SslCertificateDomain;

        public void SetClientInstallationFolder(bool isReinstall)
        {
            if (isReinstall)
            {
                ClientInstallationFolder = "Client";
                return;
            }

            var last = ProductVersion.LastIndexOf('.');
            var revision = ProductVersion.Substring(last + 1);

            ClientInstallationFolder = $"Client.{revision}";
        }
    }
}

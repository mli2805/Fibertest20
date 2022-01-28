namespace Iit.Fibertest.UtilsLib
{
    public class CurrentInstallation
    {
        public string ProductName;
        public string ProductVersion;

        public string MainName => $"{ProductName} {ProductVersion}";

        public string InstallationFolder;
        public InstallationType InstallationType;

        public bool IsHighDensityGraph;

        public string MySqlTcpPort;
        public bool IsWebNeeded;
        public bool IsWebByHttps;
        public string SslCertificateName;
        public string SslCertificatePath;
        public string SslCertificatePassword;
        public string SslCertificateDomain;
    }
}

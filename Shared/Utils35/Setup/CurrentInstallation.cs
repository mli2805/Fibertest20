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
        public string FullClientFolder => InstallationFolder + "/" + ClientInstallationFolder;

        public bool IsHighDensityGraph;

        public string MySqlTcpPort;
        public bool IsWebNeeded;
        public bool IsWebByHttps;
        public string SslCertificateName;
        public string SslCertificatePath;
        public string SslCertificatePassword;
        public string SslCertificateDomain;

        // installation type could be Client, but if SuperClient folder is found set Client's folder as Client.XXXX
        public void SetClientInstallationFolder(bool isUnderSuper)
        {
            ClientInstallationFolder = isUnderSuper ? $"Client.{ProductRevision()}" : "Client";
        }

        private string ProductRevision()
        {
            var last = ProductVersion.LastIndexOf('.');
            return ProductVersion.Substring(last + 1);
        }
    }
}

namespace Iit.Fibertest.Setup
{
    public class CurrentInstallation
    {
        public string ProductName { get; set; }
        public string ProductVersion { get; set; }
        public  string BuildNumber { get; set; }
        public  string Revision { get; set; }

        public string FullName => $"{ProductName} {ProductVersion}.{BuildNumber}.{Revision}";
        public string MainName => $"{ProductName} {ProductVersion}";

        public string InstallationFolder { get; set; }
        public InstallationType InstallationType { get; set; }

        public string MySqlTcpPort { get; set; }
    }
}

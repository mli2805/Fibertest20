namespace Iit.Fibertest.InstallRtu
{
    public class CurrentRtuInstallation
    {
        public string ProductName { get; set; }
        public string ProductVersion { get; set; }

        public string MainName => $"{ProductName} {ProductVersion}";

        public string InstallationFolder { get; set; }

        public bool IsAdmin { get; set; }

        public string[] Args { get; set; }
    }
}

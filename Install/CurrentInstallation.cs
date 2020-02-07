﻿namespace Iit.Fibertest.Install
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
        public string WebArchivePath { get; set; }
    }
}
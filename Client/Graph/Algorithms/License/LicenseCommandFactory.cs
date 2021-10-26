using System;

namespace Iit.Fibertest.Graph
{
    public static class LicenseCommandFactory
    {
        public static ApplyLicense CreateDemo()
        {
            return new ApplyLicense()
            {
                Owner = @"Demo license",
                IsIncremental = false,
                RtuCount = new LicenseParameter() {Value = 1, ValidUntil = DateTime.MaxValue},
                ClientStationCount = new LicenseParameter() {Value = 1, ValidUntil = DateTime.MaxValue},
                WebClientCount = new LicenseParameter() {Value = 1, ValidUntil = DateTime.MaxValue},
                SuperClientStationCount = new LicenseParameter() {Value = 1, ValidUntil = DateTime.MaxValue},
                IsMachineKeyRequired = false,
                Version = @"2.0.0.0"
            };
        }

        public static ApplyLicense CreateFromFile(LicenseInFile licenseInFile, Guid currentUserId)
        {
           return new ApplyLicense()
            {
                LicenseId = licenseInFile.LicenseId,
                UserId = currentUserId,
                IsIncremental = licenseInFile.IsIncremental,
                Owner = licenseInFile.Owner,
                RtuCount = new LicenseParameter(licenseInFile.RtuCount),
                ClientStationCount = new LicenseParameter(licenseInFile.ClientStationCount),
                WebClientCount = new LicenseParameter(licenseInFile.WebClientCount),
                SuperClientStationCount = new LicenseParameter(licenseInFile.SuperClientStationCount),
                IsMachineKeyRequired = licenseInFile.IsMachineKeyRequired,
                SecurityAdminPassword = licenseInFile.IsMachineKeyRequired
                    ? ((string)Cryptography.Decode(licenseInFile.SecurityAdminPassword)).GetHashString() : "",
                MachineKey = MachineKeyGetter.GetMachineKey(),
                CreationDate = licenseInFile.CreationDate,
                LoadingDate = DateTime.Today,
                Version = licenseInFile.Version,
            };
        }
    }
}
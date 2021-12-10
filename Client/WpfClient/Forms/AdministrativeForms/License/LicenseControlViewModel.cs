﻿using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class LicenseControlViewModel : PropertyChangedBase
    {
        private License _license;
        public License License
        {
            get => _license;
            set
            {
                if (Equals(value, _license)) return;
                _license = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsBasic));
            }
        }

        public bool IsBasic => !License.IsIncremental;
        public bool IsStandart => !License.IsMachineKeyRequired;

        public void FromFile(LicenseInFile licenseInFile)
        {
            License = Map(licenseInFile);
        }

        private License Map(LicenseInFile licenseInFile)
        {
            return new License
            {
                Owner = licenseInFile.Owner,
                IsIncremental = licenseInFile.IsIncremental,
                RtuCount = new LicenseParameter(licenseInFile.RtuCount),
                ClientStationCount = new LicenseParameter(licenseInFile.ClientStationCount),
                WebClientCount = new LicenseParameter(licenseInFile.WebClientCount),
                SuperClientStationCount = new LicenseParameter(licenseInFile.SuperClientStationCount),
                IsMachineKeyRequired = licenseInFile.IsMachineKeyRequired,
                CreationDate = licenseInFile.CreationDate,
                Version = licenseInFile.Version,
            };
        }
    }
}

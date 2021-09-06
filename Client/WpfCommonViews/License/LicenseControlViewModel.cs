﻿using System.Collections.Generic;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.WpfCommonViews
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
            }
        }

        public List<License> Licenses { get; set; }

        public void FromFile(LicenseInFile licenseInFile)
        {
            License = Map(licenseInFile);
        }

        private License Map(LicenseInFile licenseInFile)
        {
            return new License
            {
                Owner = licenseInFile.Owner,
                IsReplacementLicense = licenseInFile.IsReplacementLicense,
                RtuCount = new LicenseParameter(licenseInFile.RtuCount),
                ClientStationCount = new LicenseParameter(licenseInFile.ClientStationCount),
                WebClientCount = new LicenseParameter(licenseInFile.WebClientCount),
                SuperClientStationCount = new LicenseParameter(licenseInFile.SuperClientStationCount),
                CreationDate = licenseInFile.CreationDate,
                Version = licenseInFile.Version,
            };
        }
    }
}

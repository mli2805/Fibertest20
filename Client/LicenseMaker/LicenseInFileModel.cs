using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace LicenseMaker
{
    public class LicenseInFileModel : PropertyChangedBase
    {
        public List<string> TermUnit { get; set; } = new List<string>(){"years", "months"};

        private string _owner;
        private int _rtuCount = -1;
        private int _clientStationCount = -1;
        private int _superClientStationCount = -1;
        private int _rtuCountTerm = 999;
        private int _clientStationTerm = 999;
        private int _superClientTerm = 6;
        private string _rtuCountTermUnit;
        private string _clientStationTermUnit;
        private string _superClientTermUnit;
        private Guid _licenseId;
        private int _webClientCount = -1;
        private int _webClientTerm = 999;
        private string _webClientTermUnit;

        public Guid LicenseId
        {
            get => _licenseId;
            set
            {
                if (value.Equals(_licenseId)) return;
                _licenseId = value;
                NotifyOfPropertyChange();
            }
        }

        public string Owner
        {
            get => _owner;
            set
            {
                if (value == _owner) return;
                _owner = value;
                NotifyOfPropertyChange();
            }
        }

        public int RtuCount
        {
            get => _rtuCount;
            set
            {
                if (value == _rtuCount) return;
                _rtuCount = value;
                NotifyOfPropertyChange();
            }
        }

        public int RtuCountTerm
        {
            get => _rtuCountTerm;
            set
            {
                if (value == _rtuCountTerm) return;
                _rtuCountTerm = value;
                NotifyOfPropertyChange();
            }
        }

        public string RtuCountTermUnit
        {
            get => _rtuCountTermUnit;
            set
            {
                if (value == _rtuCountTermUnit) return;
                _rtuCountTermUnit = value;
                NotifyOfPropertyChange();
            }
        }

        public int ClientStationCount
        {
            get => _clientStationCount;
            set
            {
                if (value == _clientStationCount) return;
                _clientStationCount = value;
                NotifyOfPropertyChange();
            }
        }

        public int ClientStationTerm
        {
            get => _clientStationTerm;
            set
            {
                if (value == _clientStationTerm) return;
                _clientStationTerm = value;
                NotifyOfPropertyChange();
            }
        }

        public string ClientStationTermUnit
        {
            get => _clientStationTermUnit;
            set
            {
                if (value == _clientStationTermUnit) return;
                _clientStationTermUnit = value;
                NotifyOfPropertyChange();
            }
        }

        public int WebClientCount
        {
            get => _webClientCount;
            set
            {
                if (value == _webClientCount) return;
                _webClientCount = value;
                NotifyOfPropertyChange();
            }
        }

        public int WebClientTerm
        {
            get => _webClientTerm;
            set
            {
                if (value == _webClientTerm) return;
                _webClientTerm = value;
                NotifyOfPropertyChange();
            }
        }

        public string WebClientTermUnit
        {
            get => _webClientTermUnit;
            set
            {
                if (value == _webClientTermUnit) return;
                _webClientTermUnit = value;
                NotifyOfPropertyChange();
            }
        }

        public int SuperClientStationCount
        {
            get => _superClientStationCount;
            set
            {
                if (value == _superClientStationCount) return;
                _superClientStationCount = value;
                NotifyOfPropertyChange();
            }
        }

        public int SuperClientTerm
        {
            get => _superClientTerm;
            set
            {
                if (value == _superClientTerm) return;
                _superClientTerm = value;
                NotifyOfPropertyChange();
            }
        }

        public string SuperClientTermUnit
        {
            get => _superClientTermUnit;
            set
            {
                if (value == _superClientTermUnit) return;
                _superClientTermUnit = value;
                NotifyOfPropertyChange();
            }
        }

        public LicenseInFileModel()
        {
            LicenseId = Guid.NewGuid();
            RtuCountTermUnit = TermUnit.First();
            ClientStationTermUnit = TermUnit.First();
            WebClientTermUnit = TermUnit.First();
            SuperClientTermUnit = TermUnit.Skip(1).First();
        }

        public LicenseInFileModel(LicenseInFile licenseInFile)
        {
            LicenseId = licenseInFile.LicenseId;
            Owner = licenseInFile.Owner;
            RtuCount = licenseInFile.RtuCount.Value;
            ClientStationCount = licenseInFile.ClientStationCount.Value;
            WebClientCount = licenseInFile.WebClientCount.Value;
            SuperClientStationCount = licenseInFile.SuperClientStationCount.Value;

            RtuCountTerm = licenseInFile.RtuCount.Term;
            RtuCountTermUnit = licenseInFile.RtuCount.IsTermInYears ? TermUnit.First() : TermUnit.Last();
            ClientStationTerm = licenseInFile.ClientStationCount.Term;
            ClientStationTermUnit = licenseInFile.ClientStationCount.IsTermInYears ? TermUnit.First() : TermUnit.Last();
            WebClientTerm = licenseInFile.WebClientCount.Term;
            WebClientTermUnit = licenseInFile.WebClientCount.IsTermInYears ? TermUnit.First() : TermUnit.Last();
            SuperClientTerm = licenseInFile.SuperClientStationCount.Term;
            SuperClientTermUnit = licenseInFile.SuperClientStationCount.IsTermInYears ? TermUnit.First() : TermUnit.Last();

        }
    }
}
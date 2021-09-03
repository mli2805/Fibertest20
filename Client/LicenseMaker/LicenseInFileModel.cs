﻿using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace LicenseMaker
{
    public class LicenseInFileModel : PropertyChangedBase
    {
        public List<string> TermUnit { get; set; } = new List<string>() { "years", "months" };

        private Guid _licenseId;
        public Guid LicenseId
        {
            get => _licenseId;
            set
            {
                if (value.Equals(_licenseId)) return;
                _licenseId = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(LicenseKey));
            }
        }

        public string LicenseKey => Lk();

        private string Lk()
        {
            var id = LicenseId.ToString().ToUpper().Substring(0, 8);
            var licType = IsIncremental ? "I" : "B";
            return $"FT020-{id}-{licType}{RtuCount:D2}{ClientStationCount:D2}{WebClientCount:D2}{SuperClientStationCount:D2}-{CreationDate:yyMMdd}";
        }

        private string _owner;
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

        private bool _isIncremental;
        public bool IsIncremental
        {
            get => _isIncremental;
            set
            {
                if (value == _isIncremental) return;
                _isIncremental = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(LicenseKey));
            }
        }

        private int _rtuCount;
        public int RtuCount
        {
            get => _rtuCount;
            set
            {
                if (value == _rtuCount) return;
                _rtuCount = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(LicenseKey));
            }
        }

        private int _rtuCountTerm = 999;
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

        private string _rtuCountTermUnit;
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

        private int _clientStationCount;
        public int ClientStationCount
        {
            get => _clientStationCount;
            set
            {
                if (value == _clientStationCount) return;
                _clientStationCount = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(LicenseKey));
            }
        }

        private int _clientStationTerm = 999;
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

        private string _clientStationTermUnit;
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

        private int _webClientCount;
        public int WebClientCount
        {
            get => _webClientCount;
            set
            {
                if (value == _webClientCount) return;
                _webClientCount = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(LicenseKey));
            }
        }


        private int _webClientTerm = 6;
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

        private string _webClientTermUnit;

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


        private int _superClientStationCount;
        public int SuperClientStationCount
        {
            get => _superClientStationCount;
            set
            {
                if (value == _superClientStationCount) return;
                _superClientStationCount = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(LicenseKey));
            }
        }

        private int _superClientTerm = 999;
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

        private string _superClientTermUnit;
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

        private DateTime _creationDate;

        public DateTime CreationDate
        {
            get => _creationDate;
            set
            {
                if (value.Equals(_creationDate)) return;
                _creationDate = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(LicenseKey));
            }
        }

        public LicenseInFileModel()
        {
            LicenseId = Guid.NewGuid();
            RtuCountTermUnit = TermUnit.First();
            ClientStationTermUnit = TermUnit.First();
            WebClientTermUnit = TermUnit.Skip(1).First();
            SuperClientTermUnit = TermUnit.First();
            CreationDate = DateTime.Today;
        }

        public LicenseInFileModel(LicenseInFile licenseInFile)
        {
            LicenseId = licenseInFile.LicenseId;
            Owner = licenseInFile.Owner;
            IsIncremental = !licenseInFile.IsReplacementLicense;

            RtuCount = licenseInFile.RtuCount.Value;
            RtuCountTerm = licenseInFile.RtuCount.Term;
            RtuCountTermUnit = licenseInFile.RtuCount.IsTermInYears ? TermUnit.First() : TermUnit.Last();

            ClientStationCount = licenseInFile.ClientStationCount.Value;
            ClientStationTerm = licenseInFile.ClientStationCount.Term;
            ClientStationTermUnit = licenseInFile.ClientStationCount.IsTermInYears ? TermUnit.First() : TermUnit.Last();

            WebClientCount = licenseInFile.WebClientCount.Value;
            WebClientTerm = licenseInFile.WebClientCount.Term;
            WebClientTermUnit = licenseInFile.WebClientCount.IsTermInYears ? TermUnit.First() : TermUnit.Last();

            SuperClientStationCount = licenseInFile.SuperClientStationCount.Value;
            SuperClientTerm = licenseInFile.SuperClientStationCount.Term;
            SuperClientTermUnit = licenseInFile.SuperClientStationCount.IsTermInYears ? TermUnit.First() : TermUnit.Last();

            CreationDate = licenseInFile.CreationDate;
        }
    }
}
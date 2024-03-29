﻿using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class RtuInfoModel : PropertyChangedBase
    {
        private string _maker;
        public string Maker
        {
            get => _maker;
            set
            {
                if (value == _maker) return;
                _maker = value;
                NotifyOfPropertyChange();
            }
        }

        private string _mfid;
        private string _mfsn;
        private string _omid;
        private string _omsn;
        private string _version;
        private string _version2;
        private Visibility _visibility;

        public string Mfid
        {
            get => _mfid;
            set
            {
                if (value == _mfid) return;
                _mfid = value;
                NotifyOfPropertyChange();
            }
        }

        public string Mfsn
        {
            get => _mfsn;
            set
            {
                if (value == _mfsn) return;
                _mfsn = value;
                NotifyOfPropertyChange();
            }
        }

        public string Omid
        {
            get => _omid;
            set
            {
                if (value == _omid) return;
                _omid = value;
                NotifyOfPropertyChange();
            }
        }

        public string Omsn
        {
            get => _omsn;
            set
            {
                if (value == _omsn) return;
                _omsn = value;
                NotifyOfPropertyChange();
            }
        }

        public string Version
        {
            get => _version;
            set
            {
                if (value == _version) return;
                _version = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(MfidVersion));
            }
        }

        private readonly string _firmwareVersion = Resources.SID_firmwareVersion;
        public string MfidVersion => _firmwareVersion == @"firmware version"
            ? $@"{Mfid} {_firmwareVersion} {Version}"
            : $@"{_firmwareVersion} {Mfid} {Version}";

        public string OmidVersion => _firmwareVersion == @"firmware version"
            ? $@"OTDR {_firmwareVersion} {Version2}"
            : $@"{_firmwareVersion} OTDR {Version2}";

        public string Version2
        {
            get => _version2;
            set
            {
                if (value == _version2) return;
                _version2 = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(OmidVersion));
            }
        }

       
        public Visibility Visibility
        {
            get => _visibility;
            set
            {
                if (value == _visibility) return;
                _visibility = value;
                NotifyOfPropertyChange();
            }
        }


        public void FromRtu(Rtu rtu)
        {
            Maker = rtu.RtuMaker.ToString();
            Mfid = rtu.Mfid;
            Mfsn = rtu.Mfsn;
            Omid = rtu.Omid;
            Omsn = rtu.Omsn;
            Version = rtu.Version;
            Version2 = rtu.Version2;
        }

        public void FromDto(RtuInitializedDto dto)
        {
            Maker = dto.Maker.ToString();
            Mfid = dto.Mfid;
            Mfsn = dto.Mfsn;
            Omid = dto.Omid;
            Omsn = dto.Omsn;
            Version = dto.Version;
            Version2 = dto.Version2;
        }
    }
}
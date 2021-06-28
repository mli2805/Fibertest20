using System.Collections.Generic;
using System.Windows;
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
        private List<string> _bops;
        private Visibility _visibility;
        private int _fullPortCount;

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

        // private int _ownPortCount;
        // public int OwnPortCount
        // {
        //     get => _ownPortCount;
        //     set
        //     {
        //         if (value == _ownPortCount) return;
        //         _ownPortCount = value;
        //         NotifyOfPropertyChange();
        //     }
        // }

        public int FullPortCount
        {
            get => _fullPortCount;
            set
            {
                if (value == _fullPortCount) return;
                _fullPortCount = value;
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
            }
        }

        public List<string> Bops
        {
            get => _bops;
            set
            {
                if (Equals(value, _bops)) return;
                _bops = value;
                NotifyOfPropertyChange();
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
            // OwnPortCount = rtu.OwnPortCount;
            FullPortCount = rtu.FullPortCount - rtu.Children.Count;

            Bops = CreateBops(rtu);

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
            // OwnPortCount = dto.OwnPortCount;
            FullPortCount = dto.FullPortCount - dto.Children.Count;

            Bops = CreateBops(dto);

            Version = dto.Version;
            Version2 = dto.Version2;
        }

        private List<string> CreateBops(Rtu rtu)
        {
            var bops = new List<string>();
            foreach (var pair in rtu.Children)
            {
                bops.Add(string.Format(Resources.SID____on_port__0___optical_switch__1___,
                    pair.Key, pair.Value.NetAddress.ToStringA()));
                bops.Add(string.Format(Resources.SID_______________________serial__0____1__ports,
                    pair.Value.Serial, pair.Value.OwnPortCount));
            }
            return bops;
        }

        private List<string> CreateBops(RtuInitializedDto dto)
        {
            var bops = new List<string>();
            if (dto.Children != null)
                foreach (var pair in dto.Children)
                {
                    bops.Add(string.Format(Resources.SID____on_port__0___optical_switch__1___,
                        pair.Key, pair.Value.NetAddress.ToStringA()));
                    bops.Add(string.Format(Resources.SID_______________________serial__0____1__ports,
                        pair.Value.Serial, pair.Value.OwnPortCount));
                }
            return bops;
        }

    }
}
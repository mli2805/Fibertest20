﻿using System.Collections.Generic;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class PortsAndBopsViewModel : PropertyChangedBase
    {
        private int _fullPortCount;
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

        private List<string> _bops;
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

        public void FillInPortsAndBops(Rtu rtu)
        {
            FullPortCount = rtu.FullPortCount - rtu.Children.Count;
            Bops = FillInOtauList(rtu.Children);
        }

        public void FillInPortsAndBops(RtuInitializedDto dto)
        {
            FullPortCount = dto.FullPortCount - dto.Children.Count;
            Bops = FillInOtauList(dto.Children);
        }

        private List<string> FillInOtauList(Dictionary<int, OtauDto> otaus)
        {
            var bops = new List<string>();
            if (otaus != null)
                foreach (var pair in otaus)
                {
                    bops.Add(string.Format(Resources.SID____on_port__0___optical_switch__1___,
                        pair.Key, pair.Value.NetAddress.ToStringA()));

                    var bopSerial = pair.Value.Serial.Substring(0, pair.Value.Serial.Length - 1);
                    bops.Add(string.Format(Resources.SID_______________________serial__0____1__ports,
                        bopSerial, pair.Value.OwnPortCount));
                }
            return bops;
        }
    }
}

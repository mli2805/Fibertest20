using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class GponViewModel : PropertyChangedBase
    {
        private readonly Model _readModel;
        public GponModel GponInWork { get; set; }


        public List<Rtu> Rtus { get; set; }
        public ObservableCollection<Otau> Otaus { get; set; } = new ObservableCollection<Otau>();

        public GponViewModel(Model readModel)
        {
            _readModel = readModel;
        }

        public void Initialize(GponModel gponModel)
        {
            GponInWork = gponModel;
            GponInWork.PropertyChanged += GponInWork_PropertyChanged;

            Rtus = _readModel.Rtus;
            if (GponInWork.Rtu != null)
            {
                Otaus.Clear();
                foreach (var otau in _readModel.Otaus.Where(o => o.RtuId == GponInWork.Rtu.Id))
                {
                    Otaus.Add(otau);
                }
            }
        }

        private void GponInWork_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"Rtu" && GponInWork.Rtu != null)
            {
                var rtu = Rtus.FirstOrDefault(r => r.Id == GponInWork.Rtu.Id);
                CollectOtausOnRtuChanged(rtu);
                GponInWork.Otau = Otaus.First();
                GponInWork.OtauPort = "";
                GponInWork.Trace = null;
            }

            else if (e.PropertyName == @"Otau" && GponInWork.Rtu != null && GponInWork.Otau != null)
            {
                GponInWork.OtauPort = "";
                GponInWork.Trace = null;
            }

            else if (e.PropertyName == @"OtauPort" && GponInWork.Rtu != null
                        && GponInWork.Otau != null && GponInWork.OtauPort != "")
            {
                GponInWork.Trace = FindTrace();
            }
        }

        private Trace FindTrace()
        {
            if (GponInWork.Rtu.RtuMaker == RtuMaker.VeEX && GponInWork.Otau.Id == Guid.Empty // VEEX main otau
                || GponInWork.Rtu.RtuMaker == RtuMaker.IIT && GponInWork.Otau.Id == GponInWork.Otau.RtuId) // IIT main otau
            {
                return _readModel.Traces.FirstOrDefault(t => t.RtuId == GponInWork.Rtu.Id
                                                              && t.OtauPort != null
                                                              && t.OtauPort.IsPortOnMainCharon
                                                              && t.OtauPort.OpticalPort == int.Parse(GponInWork.OtauPort));
            }

            // bop
            return _readModel.Traces.FirstOrDefault(t => t.RtuId == GponInWork.Rtu.Id
                                                          && t.OtauPort != null
                                                          && t.OtauPort.OtauId == GponInWork.Otau.Id.ToString()
                                                          && t.OtauPort.OpticalPort == int.Parse(GponInWork.OtauPort));
        }

        private void CollectOtausOnRtuChanged(Rtu rtu)
        {
            if (rtu == null) return;
            Otaus.Clear();
            if (rtu.RtuMaker == RtuMaker.IIT)
            {
                var mainOtau = new Otau() { Id = rtu.Id, RtuId = rtu.Id, PortCount = rtu.OwnPortCount };
                Otaus.Add(mainOtau);
            }
            foreach (var otau in _readModel.Otaus.Where(o => o.RtuId == GponInWork.Rtu.Id))
            {
                Otaus.Add(otau);
            }
        }

        public void ClearRelation()
        {
            GponInWork.ClearRelation();
        }

        public GponPortRelation GetGponPortRelation()
        {
            if (GponInWork.Trace == null) return null;

            var otauPortDto = new OtauPortDto()
            {
                OpticalPort = int.Parse(GponInWork.OtauPort),
            };
            if (GponInWork.Rtu.RtuMaker == RtuMaker.VeEX && GponInWork.Otau.Id == Guid.Empty // VEEX main otau
                || GponInWork.Rtu.RtuMaker == RtuMaker.IIT && GponInWork.Otau.Id == GponInWork.Otau.RtuId) // IIT main otau
            {
                otauPortDto.IsPortOnMainCharon = true;
            }
            else
            {
                otauPortDto.IsPortOnMainCharon = false;
                otauPortDto.OtauId = GponInWork.Otau.Id.ToString();
            }


            return new GponPortRelation()
            {
                TceId = GponInWork.Tce.Id,
                SlotPosition = GponInWork.SlotPosition,
                GponInterface = GponInWork.GponInterface,
                RtuId = GponInWork.Rtu.Id,
                OtauPort = otauPortDto,
            };
        }

    }
}

﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class RelationOfGponViewModel : Screen
    {
        private readonly Model _readModel;
        public RelationOfGponModel RelationOfGponInWork { get; set; }


        public List<Rtu> Rtus { get; set; }
        public ObservableCollection<Otau> Otaus { get; set; } = new ObservableCollection<Otau>();

        public RelationOfGponViewModel(Model readModel)
        {
            _readModel = readModel;
        }

        public void Initialize(RelationOfGponModel relationOfGponModel)
        {
            RelationOfGponInWork = relationOfGponModel;
            RelationOfGponInWork.PropertyChanged += GponInWork_PropertyChanged;

            Rtus = _readModel.Rtus;
        }

        private void GponInWork_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"Rtu")
            {
                var rtu = Rtus.FirstOrDefault(r => r.Id == RelationOfGponInWork.Rtu.Id);
                if (rtu == null) return;
                Otaus.Clear();
                foreach (var otau in _readModel.Otaus.Where(o => o.RtuId == rtu.Id))
                {
                    Otaus.Add(otau);
                }
            }

            if (e.PropertyName == @"Otau")
            {
                var otau = Otaus.FirstOrDefault(o => o.Id == RelationOfGponInWork.Otau.Id);
                if (otau == null) return;
            }

            if (e.PropertyName == @"OtauPort")
            {
                var trace = _readModel.Traces.FirstOrDefault(t => t.RtuId == RelationOfGponInWork.Rtu.Id
                                                                  && t.OtauPort != null
                                                                  && t.OtauPort.OtauId ==
                                                                  RelationOfGponInWork.Otau.Id.ToString()
                                                                  && t.OtauPort.OpticalPort ==
                                                                  RelationOfGponInWork.OtauPort);
                if (trace != null) 
                    RelationOfGponInWork.TraceTitle = trace.Title;
            }
        }
    }
}

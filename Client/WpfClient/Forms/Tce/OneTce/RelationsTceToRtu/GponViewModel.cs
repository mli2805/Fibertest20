using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class GponViewModel : Screen
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

        private void GponInWork_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"Rtu" && GponInWork.Rtu != null)
            {
                var rtu = Rtus.FirstOrDefault(r => r.Id == GponInWork.Rtu.Id);
                if (rtu == null) return;
                Otaus.Clear();
                foreach (var otau in _readModel.Otaus.Where(o => o.RtuId == GponInWork.Rtu.Id))
                {
                    Otaus.Add(otau);
                }
            }

            if (e.PropertyName == @"Otau" && GponInWork.Rtu != null && GponInWork.Otau != null)
            {
                var otau = Otaus.FirstOrDefault(o => o.Id == GponInWork.Otau.Id);
                if (otau != null) 
                    GponInWork.Otau = otau;
            }

            if (e.PropertyName == @"OtauPort" && GponInWork.Rtu != null 
                      && GponInWork.Otau != null && GponInWork.OtauPort != 0)
            {
                var trace = _readModel.Traces.FirstOrDefault(t => t.RtuId == GponInWork.Rtu.Id
                                                                  && t.OtauPort != null
                                                                  && t.OtauPort.OtauId ==
                                                                  GponInWork.Otau.Id.ToString()
                                                                  && t.OtauPort.OpticalPort ==
                                                                  GponInWork.OtauPort);
                if (trace != null) 
                    GponInWork.TraceTitle = trace.Title;
            }
        }

        public void ClearRelation()
        {
            GponInWork.ClearRelation();
        }
    }
}

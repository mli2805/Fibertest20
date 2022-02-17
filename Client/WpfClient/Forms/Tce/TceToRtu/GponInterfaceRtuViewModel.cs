using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class GponInterfaceRtuViewModel : Screen
    {
        private readonly Model _readModel;
        public GponInterfaceRelationModel GponInWork { get; set; }


        public List<Rtu> Rtus { get; set; }
        public ObservableCollection<Otau> Otaus { get; set; } = new ObservableCollection<Otau>();
        public ObservableCollection<Trace> Traces { get; set; } = new ObservableCollection<Trace>();

        public GponInterfaceRtuViewModel(Model readModel)
        {
            _readModel = readModel;
        }

        public void Initialize(GponInterfaceRelationModel gponInterfaceRelationModel)
        {
            GponInWork = gponInterfaceRelationModel;
            GponInWork.PropertyChanged += GponInWork_PropertyChanged;

            Rtus = _readModel.Rtus;
        }

        private void GponInWork_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"Rtu")
            {
                var rtu = Rtus.FirstOrDefault(r => r.Id == GponInWork.Rtu.Id);
                if (rtu == null) return;
                Otaus.Clear();
                foreach (var otau in _readModel.Otaus.Where(o => o.RtuId == rtu.Id))
                {
                    Otaus.Add(otau);
                }
                Traces.Clear();
                if (GponInWork.Otau != null)
                    foreach (var trace in _readModel.Traces.Where(t => t.RtuId == rtu.Id))
                    {
                        Traces.Add(trace);
                    }
            }

            if (e.PropertyName == @"Otau")
            {
                var otau = Otaus.FirstOrDefault(o => o.Id == GponInWork.Otau.Id);
                if (otau == null) return;

                Traces.Clear();
                foreach (var trace in _readModel.Traces
                             .Where(t => t.RtuId == GponInWork.Rtu.Id && t.OtauPort != null
                                                                      && t.OtauPort.OtauId == GponInWork.Otau.Id.ToString()))
                {
                    Traces.Add(trace);
                }
            }
        }
    }
}

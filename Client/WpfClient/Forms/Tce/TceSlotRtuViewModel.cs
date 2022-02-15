using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TceSlotRtuViewModel : Screen
    {
        private readonly Model _readModel;
        public int Slot { get; set; }
        public string Title => $@"Slot {Slot}";

        public List<Rtu> Rtus { get; set; }

        public ObservableCollection<Otau> Otaus { get; set; } = new ObservableCollection<Otau>();
        public ObservableCollection<Trace> Traces { get; set; } = new ObservableCollection<Trace>();

        public ObservableCollection<GponInterfaceRelationModel> Gpons { get; set; } =
            new ObservableCollection<GponInterfaceRelationModel>();
        public GponInterfaceRelationModel SelectedGpon { get; set; }

        public TceSlotRtuViewModel(Model readModel)
        {
            _readModel = readModel;
        }

        public void Initialize(Tce tce, int slot, int gponInterfaceCount)
        {
            Rtus = _readModel.Rtus;

            for (int i = 0; i < gponInterfaceCount; i++)
            {
                var line = new GponInterfaceRelationModel() { Tce = tce, Slot = slot, GponInterface = i };
                line.PropertyChanged += Line_PropertyChanged;
                Gpons.Add(line);
            }
            SelectedGpon = Gpons.First();
        }

        private void Line_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"Rtu")
            {
                var rtu = Rtus.FirstOrDefault(r => r.Id == SelectedGpon.Rtu.Id);
                if (rtu == null) return;
                Otaus.Clear();
                foreach (var otau in _readModel.Otaus.Where(o => o.RtuId == rtu.Id))
                {
                    Otaus.Add(otau);
                }
                Traces.Clear();
                if (SelectedGpon.Otau != null)
                    foreach (var trace in _readModel.Traces.Where(t => t.RtuId == rtu.Id))
                    {
                        Traces.Add(trace);
                    }
            }

            if (e.PropertyName == @"Otau")
            {
                var otau = Otaus.FirstOrDefault(o => o.Id == SelectedGpon.Otau.Id);
                if (otau == null) return;

                Traces.Clear();
                foreach (var trace in _readModel.Traces
                             .Where(t => t.RtuId == SelectedGpon.Rtu.Id && t.OtauPort != null
                                         && t.OtauPort.OtauId == SelectedGpon.Otau.Id.ToString()))
                {
                    Traces.Add(trace);
                }
            }
        }
    }
}

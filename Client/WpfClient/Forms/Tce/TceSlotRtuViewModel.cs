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

        public List<Otau> Otaus { get; set; }
        public List<Trace> Traces { get; set; }

        public ObservableCollection<GponInterfaceRelationModel> Gpons { get; set; } =
            new ObservableCollection<GponInterfaceRelationModel>();
        public GponInterfaceRelationModel SelectedGpon { get; set; }

        public TceSlotRtuViewModel(Model readModel)
        {
            _readModel = readModel;
        }

        public void Initialize(int gponInterfaceCount)
        {
            Rtus = _readModel.Rtus;

            for (int i = 0; i < gponInterfaceCount; i++)
            {
                var line = new GponInterfaceRelationModel() { GponInterface = i };
                line.PropertyChanged += Line_PropertyChanged;
                Gpons.Add(line);
            }
            SelectedGpon = Gpons.First();
        }

        private void Line_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Rtu")
            {
                var rtu = Rtus.First(r => r.Title == SelectedGpon.RtuTitle);
                Traces = _readModel.Traces.Where(t => t.RtuId == rtu.Id).ToList();
            }
        }
    }
}

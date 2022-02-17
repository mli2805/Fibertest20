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

        public ObservableCollection<GponInterfaceRtuViewModel> Gpons { get; set; } =
            new ObservableCollection<GponInterfaceRtuViewModel>();

        public TceSlotRtuViewModel(Model readModel)
        {
            _readModel = readModel;
        }

        public void Initialize(Tce tce, int slot, int gponInterfaceCount)
        {
            Rtus = _readModel.Rtus;

            for (int i = 0; i < gponInterfaceCount; i++)
            {
                var line = new GponInterfaceRtuViewModel(_readModel);
                var lineModel = new GponInterfaceRelationModel()
                {
                    GponInterface = i, Slot = slot, Tce = tce
                };
                var relation = _readModel.GponPortRelations.FirstOrDefault(r => r.TceId == tce.Id
                    && r.TceSlot == slot
                    && r.GponInterface == i);
                if (relation != null)
                {
                    lineModel.Rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == relation.RtuId);
                    lineModel.Otau = _readModel.Otaus.FirstOrDefault(o => o.Id.ToString() == relation.OtauPort.OtauId);
                    lineModel.OtauPort = relation.OtauPort.OpticalPort;
                    lineModel.Trace = _readModel.Traces.FirstOrDefault(t =>
                        t.OtauPort != null
                        && t.OtauPort.OtauId == relation.OtauPort.OtauId
                        && t.OtauPort.OpticalPort == relation.OtauPort.OpticalPort);
                }
                line.Initialize(lineModel);
                Gpons.Add(line);
            }
        }
    }
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class RelationsOfSlotViewModel : Screen
    {
        private readonly Model _readModel;
        private TceS _tce;
        public int SlotPosition { get; set; }

        private int _interfaceCount;
        public int InterfaceCount
        {
            get => _interfaceCount;
            set
            {
                if (value == _interfaceCount) return;
                _interfaceCount = value;
                NotifyOfPropertyChange();
            }
        }

        public string Title => string.Format(Resources.SID_Slot__0_, SlotPosition);

        public List<Rtu> Rtus { get; set; }

        public ObservableCollection<RelationOfGponViewModel> Gpons { get; set; } =
            new ObservableCollection<RelationOfGponViewModel>();

        public RelationsOfSlotViewModel(Model readModel)
        {
            _readModel = readModel;
        }

        public void Initialize(TceS tce, int slot, int gponInterfaceCount)
        {
            Rtus = _readModel.Rtus;
            _tce = tce;
            InterfaceCount = gponInterfaceCount;

            InitializeGpons(tce, slot, gponInterfaceCount);
        }

        private void InitializeGpons(TceS tce, int slot, int gponInterfaceCount)
        {
            for (int i = 0; i < gponInterfaceCount; i++)
            {
                var line = new RelationOfGponViewModel(_readModel);
                var lineModel = new RelationOfGponModel()
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
                    lineModel.TraceTitle = _readModel.Traces.FirstOrDefault(t =>
                        t.OtauPort != null
                        && t.OtauPort.OtauId == relation.OtauPort.OtauId
                        && t.OtauPort.OpticalPort == relation.OtauPort.OpticalPort)?.Title ?? "";
                }

                line.Initialize(lineModel);
                Gpons.Add(line);
            }
        }

        public void ChangeInterfaceCount()
        {
            Gpons.Clear();
            InitializeGpons(_tce, SlotPosition, InterfaceCount);
        }
    }
}

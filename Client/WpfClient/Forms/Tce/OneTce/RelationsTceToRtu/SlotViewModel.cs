using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class SlotViewModel : Screen
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
                NotifyOfPropertyChange(nameof(Title));
            }
        }

        public string Title => InterfaceCount == 0 ? $@"{SlotPosition} -  " : $@"{SlotPosition} - {InterfaceCount}";

        public List<Rtu> Rtus { get; set; }

        public ObservableCollection<GponViewModel> Gpons { get; set; } =
            new ObservableCollection<GponViewModel>();

        public SlotViewModel(Model readModel)
        {
            _readModel = readModel;
        }

        public void Initialize(TceS tce, int slotPosition)
        {
            Rtus = _readModel.Rtus;
            _tce = tce;
            SlotPosition = slotPosition;
            InterfaceCount = tce.Slots.First(s => s.Position == slotPosition).GponInterfaceCount;

            InitializeGpons();
        }

        private void InitializeGpons()
        {
            for (int i = 0; i < InterfaceCount; i++)
            {
                var line = new GponViewModel(_readModel);
                var lineModel = new GponModel()
                {
                    GponInterface = i,
                    SlotPosition = SlotPosition,
                    Tce = _tce
                };

                var relation = _readModel.GponPortRelations.FirstOrDefault(r => r.TceId == _tce.Id
                    && r.SlotPosition == SlotPosition
                    && r.GponInterface == i);
                if (relation != null)
                {
                    lineModel.Rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == relation.RtuId);
                    lineModel.Otau = _readModel.Otaus.FirstOrDefault(o => o.Id.ToString() == relation.OtauPort.OtauId);
                    lineModel.OtauPort = relation.OtauPort.OpticalPort.ToString();
                    lineModel.Trace = _readModel.Traces.FirstOrDefault(t =>
                        t.OtauPort != null
                        && t.OtauPort.OtauId == relation.OtauPort.OtauId
                        && t.OtauPort.OpticalPort == relation.OtauPort.OpticalPort);
                }

                line.Initialize(lineModel);
                Gpons.Add(line);
            }
        }

        public void ChangeInterfaceCount()
        {
            var oldCount = _tce.Slots.First(s => s.Position == SlotPosition).GponInterfaceCount;
            _tce.Slots.First(s => s.Position == SlotPosition).GponInterfaceCount = InterfaceCount;

            if (oldCount < InterfaceCount)
                for (int i = oldCount; i < InterfaceCount; i++)
                {
                    CreateNewGponInterface(i);
                }
            else
            {
                var forRemoval = Gpons.Where(g => g.GponInWork.GponInterface >= InterfaceCount).ToList();
                foreach (var gponViewModel in forRemoval)
                {
                    Gpons.Remove(gponViewModel);
                }
            }
        }

        private void CreateNewGponInterface(int i)
        {
            var line = new GponViewModel(_readModel);
            var lineModel = new GponModel()
            {
                GponInterface = i,
                SlotPosition = SlotPosition,
                Tce = _tce
            };

            line.Initialize(lineModel);
            Gpons.Add(line);
        }

        public IEnumerable<GponPortRelation> GetGponPortsRelations()
        {
            return Gpons.Where(g=>g.GponInWork.Trace != null).Select(gponViewModel => gponViewModel.GetGponPortRelation());
        }
    }
}

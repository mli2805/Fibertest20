using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class TceComponentsViewModel : Screen
    {
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        public Tce Tce { get; set; }

        public string TextBoxSlotCount { get; set; }

        public ObservableCollection<TceSlot> Slots { get; set; } = new ObservableCollection<TceSlot>();

        public TceComponentsViewModel(IWcfServiceDesktopC2D c2DWcfManager)
        {
            _c2DWcfManager = c2DWcfManager;
        }

        public void Initialize(Tce tce)
        {
            if (tce.Slots == null) tce.Slots = new List<TceSlot>();
            Tce = tce;
            Slots.Clear();
            foreach (var tceSlot in tce.Slots)
            {
                Slots.Add(tceSlot);
            }
        }

        public void ApplySlotCount()
        {
            if (!int.TryParse(TextBoxSlotCount, out int newSlotCount)) return;

            while (Slots.Count < newSlotCount)
            {
                Slots.Add(new TceSlot() { Position = Slots.Count });
            }

            while (Slots.Count > newSlotCount)
            {
                var lastSlot = Slots.Last();
                Slots.Remove(lastSlot);
            }

            Tce.SlotCount = newSlotCount;
        }

        public async Task Save()
        {
            var cmd = new AddOrUpdateTce()
            {
                Id = Tce.Id,
                Title = Tce.Title,
                TceType = Tce.TceType,
                Ip = Tce.Ip,
                SlotCount = Tce.SlotCount,
                Slots = Slots.ToList(),
                Comment = Tce.Comment,
            };
            var _ = await _c2DWcfManager.SendCommandAsObj(cmd);
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}

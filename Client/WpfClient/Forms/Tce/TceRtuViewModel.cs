using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TceRtuViewModel : Screen
    {
        private readonly Model _readModel;
        public Tce Tce { get; set; }
        public List<TceSlotRtuViewModel> Slots { get; set; }
        public TceSlotRtuViewModel SelectedSlot { get; set; }

        public TceRtuViewModel(Model readModel)
        {
            _readModel = readModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = $@"{Tce.TceType}  {Tce.Title}";
        }

        public void Initialize(Tce tce, int slotCount)
        {
            Tce = tce;
            Slots = new List<TceSlotRtuViewModel>();
            for (int i = 0; i < slotCount; i++)
            {
                var slot = new TceSlotRtuViewModel(_readModel) { Slot = i + 1 };
                slot.Initialize(tce, i, 16);
                Slots.Add(slot);
            }
        }

        public async Task Apply()
        {
            await Task.Delay(1);
        }

        public void Close()
        {
            TryClose();
        }
    }
}

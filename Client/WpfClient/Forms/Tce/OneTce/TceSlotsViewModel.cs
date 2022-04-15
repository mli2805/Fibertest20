using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TceSlotsViewModel : PropertyChangedBase
    {
        public List<SlotViewModel> Slots { get; set; }

        private SlotViewModel _selectedSlot;
        public SlotViewModel SelectedSlot    
        {
            get => _selectedSlot;
            set
            {
                if (Equals(value, _selectedSlot)) return;
                _selectedSlot = value;
                NotifyOfPropertyChange();
            }
        }

        public void Initialize(Model readModel, TceS tce)
        {
            Slots = new List<SlotViewModel>();
            for (int i = 0; i < tce.SlotCount; i++)
            {
                var slot = new SlotViewModel(readModel) { SlotPosition = tce.TceTypeStruct.SlotPositions[i] };
                slot.Initialize(tce, i, tce.Slots[i].GponInterfaceCount);
                Slots.Add(slot);
            }

            SelectedSlot = Slots.First();
        }

    }
}

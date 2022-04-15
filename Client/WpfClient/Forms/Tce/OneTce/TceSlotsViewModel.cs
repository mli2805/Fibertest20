using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TceSlotsViewModel : PropertyChangedBase
    {
        public List<RelationsOfSlotViewModel> Slots { get; set; }

        private RelationsOfSlotViewModel _selectedSlot;
        public RelationsOfSlotViewModel SelectedSlot    
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
            Slots = new List<RelationsOfSlotViewModel>();
            for (int i = 0; i < tce.SlotCount; i++)
            {
                var slot = new RelationsOfSlotViewModel(readModel) { SlotPosition = tce.TceTypeStruct.SlotPositions[i] };
                slot.Initialize(tce, i, tce.Slots[i].GponInterfaceCount);
                Slots.Add(slot);
            }

            SelectedSlot = Slots.First();
        }

    }
}

using System.Collections.Generic;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TceSlotsViewModel : PropertyChangedBase
    {
        public List<RelationsOfSlotViewModel> Slots { get; set; }
        public RelationsOfSlotViewModel SelectedSlot { get; set; }

        public void Initialize(Model readModel, TceS tce)
        {
            Slots = new List<RelationsOfSlotViewModel>();
            for (int i = 0; i < tce.SlotCount; i++)
            {
                var slot = new RelationsOfSlotViewModel(readModel) { SlotPosition = tce.TceTypeStruct.SlotPositions[i] };
                slot.Initialize(tce, i, tce.Slots[i].GponInterfaceCount);
                Slots.Add(slot);
            }
        }

    }
}

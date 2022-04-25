using System;
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

        public void Initialize(Model readModel, TceS tce, Func<Trace, bool> isTraceLinked)
        {
            Slots = new List<SlotViewModel>();
            foreach (var slotPosition in tce.TceTypeStruct.SlotPositions)
            {
                var slot = new SlotViewModel(readModel);
                slot.Initialize(tce, slotPosition, isTraceLinked);
                Slots.Add(slot);

            }

            SelectedSlot = Slots.First();
        }

    }
}

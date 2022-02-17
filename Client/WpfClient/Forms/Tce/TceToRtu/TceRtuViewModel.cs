using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class TceRtuViewModel : Screen
    {
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly Model _readModel;
        public Tce Tce { get; set; }
        public List<TceSlotRtuViewModel> Slots { get; set; }
        public TceSlotRtuViewModel SelectedSlot { get; set; }

        public TceRtuViewModel(IWcfServiceDesktopC2D c2DWcfManager, Model readModel)
        {
            _c2DWcfManager = c2DWcfManager;
            _readModel = readModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = $@"{Tce.TceType}  {Tce.Title}";
        }

        public void Initialize(Tce tce)
        {
            Tce = tce;
            Slots = new List<TceSlotRtuViewModel>();
            for (int i = 0; i < tce.SlotCount; i++)
            {
                var slot = new TceSlotRtuViewModel(_readModel) { Slot = i };
                slot.Initialize(tce, i, tce.Slots[i].GponInterfaceCount);
                Slots.Add(slot);
            }
        }

        public async Task Save()
        {
            await Task.Delay(1);

            var cmd = new UpdateAllTceGponRelations();
            foreach (var slot in Slots)
            {
                foreach (var gpon in slot.Gpons.Where(g=>g.GponInWork.OtauPort != 0))
                {
                    var relation = new GponPortRelation()
                    {
                        TceId = Tce.Id,
                        TceSlot = slot.Slot,
                        GponInterface = gpon.GponInWork.GponInterface,
                        RtuId = gpon.GponInWork.Rtu.Id,
                        OtauPort = new OtauPortDto()
                        {
                            OtauId = gpon.GponInWork.Otau.Id.ToString(),
                            OpticalPort = gpon.GponInWork.OtauPort,
                        }
                    };
                    cmd.AllTceRelations.Add(relation);
                }
            }

            await _c2DWcfManager.SendCommandAsObj(cmd);
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}

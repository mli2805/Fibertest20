using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class RelationsOfTceViewModel : Screen
    {
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly Model _readModel;
        public Tce Tce { get; set; }
        public List<RelationsOfSlotViewModel> Slots { get; set; }
        public RelationsOfSlotViewModel SelectedSlot { get; set; }

        public RelationsOfTceViewModel(IWcfServiceDesktopC2D c2DWcfManager, Model readModel)
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
            Slots = new List<RelationsOfSlotViewModel>();
            for (int i = 0; i < tce.SlotCount; i++)
            {
                var slot = new RelationsOfSlotViewModel(_readModel) { Slot = i };
                slot.Initialize(tce, i, tce.Slots[i].GponInterfaceCount);
                Slots.Add(slot);
            }
        }

        public async Task Save()
        {
            await Task.Delay(1);

            var cmd = new UpdateAllTceGponRelations() { TceId = Tce.Id };
            foreach (var slot in Slots)
            {
                foreach (var gpon in slot.Gpons.Where(g=>g.RelationOfGponInWork.OtauPort != 0))
                {
                    var relation = new GponPortRelation()
                    {
                        TceId = Tce.Id,
                        TceSlot = slot.Slot,
                        GponInterface = gpon.RelationOfGponInWork.GponInterface,
                        RtuId = gpon.RelationOfGponInWork.Rtu.Id,
                        OtauPort = new OtauPortDto()
                        {
                            OtauId = gpon.RelationOfGponInWork.Otau.Id.ToString(),
                            OpticalPort = gpon.RelationOfGponInWork.OtauPort,
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

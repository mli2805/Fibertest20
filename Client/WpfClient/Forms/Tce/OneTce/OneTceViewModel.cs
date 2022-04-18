using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class OneTceViewModel : Screen
    {
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;
        private bool _isInCreationMode;
        private TceS _tceInWork;
        public TceInfoViewModel TceInfoViewModel { get; set; } = new TceInfoViewModel();
        public TceSlotsViewModel TceSlotsViewModel { get; set; } = new TceSlotsViewModel();

        public OneTceViewModel(IWcfServiceDesktopC2D c2DWcfManager, Model readModel, IWindowManager windowManager)
        {
            _c2DWcfManager = c2DWcfManager;
            _readModel = readModel;
            _windowManager = windowManager;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _isInCreationMode ? Resources.SID_Add_Equipment : Resources.SID_Update_equipment;
        }
        public void Initialize(TceS tce, bool isCreation)
        {
            _isInCreationMode = isCreation;
            _tceInWork = tce;
            TceInfoViewModel.Initialize(tce);
            TceSlotsViewModel.Initialize(_readModel, tce);
        }

        public async void ButtonSave()
        {
            await Save();
        }
        public async void ButtonSaveAndClose()
        {
            if (await Save())
                TryClose(true);
        }

        private async Task<bool> Save()
        {
            var cmd = new AddOrUpdateTceWithRelations()
            {
                Id = _tceInWork.Id,
                Title = TceInfoViewModel.Title,
                TceTypeStruct = _tceInWork.TceTypeStruct,
                Ip = TceInfoViewModel.Ip4InputViewModel.GetString(),
                Slots = _tceInWork.Slots,
                Comment = TceInfoViewModel.Comment,
            };

            foreach (var slot in TceSlotsViewModel.Slots)
            {
                foreach (var gpon in slot.Gpons.Where(g => g.GponInWork.OtauPort != 0))
                {
                    var relation = new GponPortRelation()
                    {
                        TceId = _tceInWork.Id,
                        TceSlot = slot.SlotPosition,
                        GponInterface = gpon.GponInWork.GponInterface,
                        RtuId = gpon.GponInWork.Rtu.Id,
                        OtauPort = new OtauPortDto()
                        {
                            OtauId = gpon.GponInWork.Otau.Id.ToString(),
                            OpticalPort = gpon.GponInWork.OtauPort,
                        }
                    };
                    cmd.AllRelationsOfTce.Add(relation);
                }
            }

            var result = await _c2DWcfManager.SendCommandAsObj(cmd);
            if (result != null)
            {
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, result));
            }
            return string.IsNullOrEmpty(result);
        }

        public void Cancel()
        {
            TryClose(false);
        }

    }
}

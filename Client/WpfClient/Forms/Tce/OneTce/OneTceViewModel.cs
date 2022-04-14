using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class OneTceViewModel : Screen
    {
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly Model _readModel;
        private bool _isInCreationMode;
        private TceS _tceInWork;
        public TceInfoViewModel TceInfoViewModel { get; set; } = new TceInfoViewModel();
        public TceSlotsViewModel TceSlotsViewModel { get; set; } = new TceSlotsViewModel();

        public OneTceViewModel(IWcfServiceDesktopC2D c2DWcfManager, Model readModel)
        {
            _c2DWcfManager = c2DWcfManager;
            _readModel = readModel;
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
            await Save();
            TryClose(true);
        }

        private async Task Save()
        {
            //TODO new command/event

            // var cmd = new AddOrUpdateTce()
            // {
            //     Id = _tceInWork.Id,
            //     Title = TceInfoViewModel.Title,
            //     TceType = _tceInWork.TceTypeStruct,
            //     Ip = TceInfoViewModel.Ip4InputViewModel.GetString(),
            //     SlotCount = _tceInWork.SlotCount,
            //     Slots = _tceInWork.Slots,
            //     Comment = TceInfoViewModel.Comment,
            // };
            // var res = await _c2DWcfManager.SendCommandAsObj(cmd);
            // TryClose(string.IsNullOrEmpty(res));
        }

        public void Cancel()
        {
            TryClose(false);
        }

    }
}

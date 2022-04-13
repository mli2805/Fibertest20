using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class OneTceViewModel : Screen
    {
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private bool _isInCreationMode;
        private TceS _tceInWork;
        public TceInfoViewModel TceInfoViewModel { get; set; }

        public OneTceViewModel(IWcfServiceDesktopC2D c2DWcfManager)
        {
            _c2DWcfManager = c2DWcfManager;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _isInCreationMode ? Resources.SID_Add_Equipment : Resources.SID_Update_equipment;
        }
        public void Initialize(TceS tce, bool isCreation)
        {
            _isInCreationMode = isCreation;
            _tceInWork = tce;
        }

        public async void Save()
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

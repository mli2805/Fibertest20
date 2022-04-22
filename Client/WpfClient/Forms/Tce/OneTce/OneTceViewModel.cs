using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
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
            DisplayName = _isInCreationMode ? Resources.SID_Add_Equipment : Resources.SID_Settings;
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
            var cmd = new AddOrUpdateTceWithRelations
            {
                Id = _tceInWork.Id,
                Title = TceInfoViewModel.Title,
                TceTypeStruct = _tceInWork.TceTypeStruct,
                Ip = TceInfoViewModel.Ip4InputViewModel.GetString(),
                Slots = TceSlotsViewModel.Slots.Select(s=>s.GetTceSlot()).ToList(),
                Comment = TceInfoViewModel.Comment,
                AllRelationsOfTce = TceSlotsViewModel.Slots.SelectMany(s => s.GetGponPortsRelations()).ToList(),
            };

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

using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class TceTypeViewModel :Screen
    {
        private readonly Model _readModel;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly CurrentUser _currentUser;

        public TceTypeSelectionViewModel HuaweiSelectionViewModel { get; set; }
        public TceTypeSelectionViewModel ZteSelectionViewModel { get; set; }
        public Visibility ReSeedVisibility { get; set; }
        public int SelectedTabItem { get; set; }

        public TceTypeViewModel(Model readModel, IWcfServiceDesktopC2D c2DWcfManager, IWindowManager windowManager, CurrentUser currentUser )
        {
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _currentUser = currentUser;
        }

        public void Initialize()
        {
            HuaweiSelectionViewModel = new TceTypeSelectionViewModel();
            HuaweiSelectionViewModel.Initialize(_readModel.TceTypeStructs.Where(s => s.Maker == TceMaker.Huawei && s.IsVisible).ToList());
            ZteSelectionViewModel = new TceTypeSelectionViewModel();
            ZteSelectionViewModel.Initialize(_readModel.TceTypeStructs.Where(s => s.Maker == TceMaker.ZTE && s.IsVisible).ToList());

            ReSeedVisibility = _currentUser.Role == Role.Developer ? Visibility.Visible : Visibility.Collapsed;
        }

        public async void ReSeed()
        {
            var cmd = new ReSeedTceTypeStructList() { TceTypes = TceTypeStructExt.Generate().ToList() };
            var res = await _c2DWcfManager.SendCommandAsObj(cmd);
            if (res != null)
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error,
                    @"Can't send Tce Types List!"));
            TryClose(false);
        }

        public void Apply()
        {
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }
    }
}

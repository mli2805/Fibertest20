using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class TceTypeViewModel :Screen
    {
        private readonly Model _readModel;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly CurrentUser _currentUser;
        public List<TceTypeStruct> HuaweiModels { get; set; }
        public List<TceTypeStruct> ZteModels { get; set; }
        public Visibility ReSeedVisibility { get; set; }

        public TceTypeViewModel(Model readModel, IWcfServiceDesktopC2D c2DWcfManager, IWindowManager windowManager, CurrentUser currentUser )
        {
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _currentUser = currentUser;
        }

        public void Initialize()
        {
            HuaweiModels = _readModel.TceTypeStructs.Where(s => s.Maker == TceMaker.Huawei && s.IsVisible).ToList();
            ZteModels = _readModel.TceTypeStructs.Where(s => s.Maker == TceMaker.ZTE && s.IsVisible).ToList();
            ReSeedVisibility = _currentUser.Role == Role.Developer ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ReSend()
        {

            TryClose(false);
        }
    }
}

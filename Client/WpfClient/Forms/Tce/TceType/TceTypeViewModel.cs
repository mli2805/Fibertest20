using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class TceTypeViewModel : Screen
    {
        private readonly Model _readModel;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly CurrentUser _currentUser;
        private bool _isCreationMode;

        public TceTypeSelectionViewModel HuaweiSelectionViewModel { get; set; }
        public TceTypeSelectionViewModel ZteSelectionViewModel { get; set; }
        public Visibility ReSeedVisibility { get; set; }
        public int SelectedTabItem { get; set; }

        public TceTypeViewModel(Model readModel, IWcfServiceDesktopC2D c2DWcfManager, IWindowManager windowManager, CurrentUser currentUser)
        {
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _currentUser = currentUser;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Telecommunications_equipment_model;
        }

        public void Initialize(TceTypeStruct tceTypeStruct, bool isCreationMode)
        {
            _isCreationMode = isCreationMode;
            HuaweiSelectionViewModel = new TceTypeSelectionViewModel();
            HuaweiSelectionViewModel.Initialize(_readModel.TceTypeStructs.Where(s => s.Maker == TceMaker.Huawei && s.IsVisible).ToList(), tceTypeStruct);
            ZteSelectionViewModel = new TceTypeSelectionViewModel();
            ZteSelectionViewModel.Initialize(_readModel.TceTypeStructs.Where(s => s.Maker == TceMaker.ZTE && s.IsVisible).ToList(), tceTypeStruct);

            ReSeedVisibility = _currentUser.Role <= Role.Root ? Visibility.Visible : Visibility.Collapsed;
        }

        public async Task ReSeed()
        {
            var cmd = new ReSeedTceTypeStructList() { TceTypes = TceTypeStructExt.Generate().ToList() };
            var res = await _c2DWcfManager.SendCommandAsObj(cmd);
            if (res != null)
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error,
                    @"Can't send Tce Types List!"));
            TryClose(false);
        }

        public void Select()
        {
            if (_isCreationMode)
            {
                TryClose(true);
                return;
            }

            var vm = new MyMessageBoxViewModel(MessageType.Confirmation,
                new List<MyMessageBoxLineModel>()
                {
                    new MyMessageBoxLineModel(){ Line = "Изменение типа оборудования может привести к потере связей", FontWeight = FontWeights.Bold },
                    new MyMessageBoxLineModel(){ Line = "порт RTU - интерфейс телекоммуникационного оборудования", FontWeight = FontWeights.Bold },
                    new MyMessageBoxLineModel(){ Line = ""},
                    new MyMessageBoxLineModel(){ Line = ""},
                    new MyMessageBoxLineModel(){ Line = "Изменения будут применены после сохранения в следующем окне."},
                    new MyMessageBoxLineModel(){ Line = ""},
                    new MyMessageBoxLineModel(){ Line = "Продолжить?"},
                });
            _windowManager.ShowDialogWithAssignedOwner(vm);

            TryClose(vm.IsAnswerPositive);
        }

        public void Cancel()
        {
            TryClose(false);
        }
    }
}

using System;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class RtuInitializeViewModel : Screen
    {
        public RtuInitializeModel FullModel { get; set; }

        private readonly ILifetimeScope _globalScope;
        private readonly CurrentUser _currentUser;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceCommonC2D _wcfServiceCommonC2D;
        private readonly IMyLog _logFile;
        private readonly CommonStatusBarViewModel _commonStatusBarViewModel;

        private bool _isIdle;
        public bool IsIdle
        {
            get => _isIdle;
            set
            {
                if (value == _isIdle) return;
                _isIdle = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsInitializationPermitted));
            }
        }

        private bool _isCloseEnabled;
        public bool IsCloseEnabled
        {
            get { return _isCloseEnabled; }
            set
            {
                if (value == _isCloseEnabled) return;
                _isCloseEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsInitializationPermitted => _currentUser.Role <= Role.Operator && IsIdle;

        public RtuInitializeViewModel(ILifetimeScope globalScope, CurrentUser currentUser,
            IWindowManager windowManager, IWcfServiceCommonC2D wcfServiceCommonC2D,
            IMyLog logFile, RtuLeaf rtuLeaf, CommonStatusBarViewModel commonStatusBarViewModel)
        {
            _globalScope = globalScope;
            _currentUser = currentUser;
            IsIdle = true;
            IsCloseEnabled = true;
            _windowManager = windowManager;
            _wcfServiceCommonC2D = wcfServiceCommonC2D;
            _logFile = logFile;
            _commonStatusBarViewModel = commonStatusBarViewModel;

            FullModel = _globalScope.Resolve<RtuInitializeModel>();
            FullModel.StartFromRtu(rtuLeaf.Id);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Network_settings;
        }

        public async Task InitializeRtu()
        {
            if (!FullModel.Validate()) return;

            try
            {
                IsIdle = false;
                IsCloseEnabled = false;
                RtuInitializedDto result;

                using (_globalScope.Resolve<IWaitCursor>())
                {
                    if (!await FullModel.CheckConnectionBeforeInitialization())
                        return;
                    var rtuMaker = FullModel.MainChannelTestViewModel.NetAddressInputViewModel.Port == (int)TcpPorts.RtuListenTo
                        ? RtuMaker.IIT
                        : RtuMaker.VeEX;
                    _commonStatusBarViewModel.StatusBarMessage2 = Resources.SID_RTU_is_being_initialized___;

                    var initializeRtuDto = FullModel.CreateDto(rtuMaker, _currentUser);
                    result = await _wcfServiceCommonC2D.InitializeRtuAsync(initializeRtuDto);
                }
                _commonStatusBarViewModel.StatusBarMessage2 = "";

                if (result.ReturnCode == ReturnCode.RtuUnauthorizedAccess)
                {
                    var vm = new RtuAskSerialViewModel();
                    vm.Initialize(!FullModel.OriginalRtu.IsInitialized,
                        FullModel.MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress().ToStringA(), result.Serial);
                    _windowManager.ShowDialogWithAssignedOwner(vm);
                    if (!vm.IsSavePressed) return;
                    FullModel.OriginalRtu.Serial = vm.Serial.ToUpper();
                    await InitializeRtu();
                }
                else
                    ReactRtuInitialized(result);
            }
            catch (Exception e)
            {
                _logFile.AppendLine($@"InitializeRtu : {e.Message}");
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_RTU_initialization_error_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
            }
            finally
            {
                IsIdle = true;
                IsCloseEnabled = true;
            }
        }

        private void ReactRtuInitialized(RtuInitializedDto dto)
        {
           

            var rtuName = dto.RtuAddresses != null ? $@"RTU {dto.RtuAddresses.Main.Ip4Address}" : @"RTU";
            var message = dto.IsInitialized
                ? $@"{rtuName} initialized successfully."
                : $@"{rtuName} initialization failed. " + Environment.NewLine + dto.ReturnCode.GetLocalizedString();
            if (!string.IsNullOrEmpty(dto.ErrorMessage))
                message += Environment.NewLine + dto.ErrorMessage;
            _logFile.AppendLine(message);

            if (dto.IsInitialized)
                FullModel.UpdateWithDto(dto);

            var resultMessageBox = dto.ShowInitializationResultMessageBox(FullModel.OriginalRtu.Title);
            _windowManager.ShowDialogWithAssignedOwner(resultMessageBox);
        }

        public void Close()
        {
            TryClose();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceCommonC2D _wcfServiceCommonC2D;
        private readonly IWcfServiceDesktopC2D _wcfServiceDesktopC2D;
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

        public RtuInitializeViewModel(ILifetimeScope globalScope, CurrentUser currentUser, Model readModel,
            IWindowManager windowManager, IWcfServiceCommonC2D wcfServiceCommonC2D, IWcfServiceDesktopC2D wcfServiceDesktopC2D,
            IMyLog logFile, RtuLeaf rtuLeaf, CommonStatusBarViewModel commonStatusBarViewModel)
        {
            _globalScope = globalScope;
            _currentUser = currentUser;
            _readModel = readModel;
            IsIdle = true;
            IsCloseEnabled = true;
            _windowManager = windowManager;
            _wcfServiceCommonC2D = wcfServiceCommonC2D;
            _wcfServiceDesktopC2D = wcfServiceDesktopC2D;
            _logFile = logFile;
            _commonStatusBarViewModel = commonStatusBarViewModel;

            FullModel = _globalScope.Resolve<RtuInitializeModel>();
            FullModel.StartFromRtu(rtuLeaf.Id);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Network_settings;
        }

        public async Task InitializeAndSynchronize()
        {
            await Do(true);
        }

        public async Task InitializeRtu()
        {
            await Do(false);
        }

        private async Task Do(bool isSynchronizationRequired)
        {
            if (!FullModel.Validate()) return;

            try
            {
                IsIdle = false;
                IsCloseEnabled = false;
                InitializeRtuDto initializeRtuDto;
                RtuInitializedDto result;

                using (_globalScope.Resolve<IWaitCursor>())
                {
                    if (!await FullModel.CheckConnectionBeforeInitialization())
                        return;

                    _commonStatusBarViewModel.StatusBarMessage2 = Resources.SID_RTU_is_being_initialized___;

                    initializeRtuDto = FullModel.CreateDto(_currentUser);
                    initializeRtuDto.IsSynchronizationRequired = isSynchronizationRequired;
                    result = await _wcfServiceCommonC2D.InitializeRtuAsync(initializeRtuDto);
                }
                _commonStatusBarViewModel.StatusBarMessage2 = "";

                if (result.ReturnCode == ReturnCode.InProgress)
                {
                    result = await PollMakLinuxTillResult(initializeRtuDto.RtuAddresses);
                }
                else if (result.ReturnCode == ReturnCode.RtuUnauthorizedAccess)
                {
                    if (AskVeexSerial(result.Serial))
                        await InitializeRtu();
                    return;
                }
                _logFile.AppendLine(result.CreateLogMessage());

                if (result.IsInitialized)
                    FullModel.UpdateWithDto(result);

                if (result.IsInitialized && isSynchronizationRequired)
                    await SynchronizeBaseRefs();

                _windowManager.ShowDialogWithAssignedOwner(result.CreateMessageBox(FullModel.OriginalRtu.Title));
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
                _commonStatusBarViewModel.StatusBarMessage2 = "";
            }
        }

        private bool AskVeexSerial(string oldSerial)
        {
            var vm = new RtuAskSerialViewModel();
            vm.Initialize(!FullModel.OriginalRtu.IsInitialized,
                FullModel.MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress().ToStringA(), oldSerial);
            _windowManager.ShowDialogWithAssignedOwner(vm);
            if (!vm.IsSavePressed) return false;
            FullModel.OriginalRtu.Serial = vm.Serial.ToUpper();
            return true;
        }

        private async Task<RtuInitializedDto> PollMakLinuxTillResult(DoubleAddress rtuDoubleAddress)
        {
            var count = 18; // 18 * 5 sec = 90 sec limit
            var requestDto = new GetCurrentRtuStateDto() { RtuDoubleAddress = rtuDoubleAddress };
            while (--count >= 0)
            {
                await Task.Delay(5000);
                var state = await _wcfServiceCommonC2D.GetRtuCurrentState(requestDto);
                if (state.LastInitializationResult != null)
                    return state.LastInitializationResult.Result;
            }

            return new RtuInitializedDto(ReturnCode.TimeOutExpired);
        }

        private async Task SynchronizeBaseRefs()
        {
            var commands = new List<object>();
            foreach (var veexTest in _readModel.VeexTests)
            {
                var trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == veexTest.TraceId);
                if (trace != null && trace.RtuId == FullModel.OriginalRtu.Id)
                    commands.Add(new RemoveVeexTest() { TestId = veexTest.TestId });
            }
            await _wcfServiceDesktopC2D.SendCommandsAsObjs(commands);

            using (_globalScope.Resolve<IWaitCursor>())
            {
                var list = _readModel.CreateReSendDtos(FullModel.OriginalRtu, _currentUser.ConnectionId).ToList();
                foreach (var reSendBaseRefsDto in list)
                {
                    _commonStatusBarViewModel.StatusBarMessage2
                        = string.Format(Resources.SID_Sending_base_refs_for_port__0_, reSendBaseRefsDto.OtauPortDto.ToStringB());
                    var resultDto = await _wcfServiceCommonC2D.ReSendBaseRefAsync(reSendBaseRefsDto);
                    _commonStatusBarViewModel.StatusBarMessage2 =
                        // string.Format(Resources.SID_Sending_base_refs_for_port__0_, reSendBaseRefsDto.OtauPortDto.ToStringB()) + @"   " + 
                        resultDto.ReturnCode.GetLocalizedString();
                }
            }
        }

        public void Close()
        {
            TryClose();
        }
    }
}

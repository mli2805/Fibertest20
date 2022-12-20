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
            IWindowManager windowManager, IWcfServiceCommonC2D wcfServiceCommonC2D,
            IMyLog logFile, RtuLeaf rtuLeaf, CommonStatusBarViewModel commonStatusBarViewModel)
        {
            _globalScope = globalScope;
            _currentUser = currentUser;
            _readModel = readModel;
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
                    initializeRtuDto.IsSynchronizationRequired = isSynchronizationRequired;
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
                {
                    ReactRtuInitialized(result);

                    if (result.IsInitialized && isSynchronizationRequired)
                        await SynchronizeBaseRefs();
                }
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

        private async Task SynchronizeBaseRefs()
        {
            var arr = _readModel.VeexTests.ToArray();
            foreach (var veexTest in arr)
            {
                var trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == veexTest.TraceId);
                if (trace != null && trace.RtuId == FullModel.OriginalRtu.Id)
                    _readModel.VeexTests.Remove(veexTest);
            }

            // 
            _logFile.AppendLine(@"Before synchronization in Model there are:");
            foreach (var veexTest in _readModel.VeexTests)
            {
                var trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == veexTest.TraceId);
                if (trace != null && trace.RtuId == FullModel.OriginalRtu.Id)
                    _logFile.AppendLine($@"{veexTest.BasRefType} test for port {trace.OtauPort.ToStringB()} created {veexTest.CreationTimestamp:G}", 3);
            }






            using (_globalScope.Resolve<IWaitCursor>())
            {
                var list = _readModel.CreateReSendDtos(FullModel.OriginalRtu, _currentUser).ToList();
                foreach (var reSendBaseRefsDto in list)
                {
                    _commonStatusBarViewModel.StatusBarMessage2 = $@"Sending base refs {reSendBaseRefsDto.OtauPortDto.ToStringB()}";
                    var resultDto = await _wcfServiceCommonC2D.ReSendBaseRefAsync(reSendBaseRefsDto);
                    _commonStatusBarViewModel.StatusBarMessage2 = $@"Sending base refs {reSendBaseRefsDto.OtauPortDto.ToStringB()} {resultDto.ReturnCode}";
                }
            }
        }

        private void ReactRtuInitialized(RtuInitializedDto result)
        {
            _logFile.AppendLine(result.CreateLogMessage());

            if (result.IsInitialized)
                FullModel.UpdateWithDto(result);

            _windowManager.ShowDialogWithAssignedOwner(result.CreateMessageBox(FullModel.OriginalRtu.Title));
        }

        public void Close()
        {
            TryClose();
        }
    }

    public static class ReSendDtoExt
    {
        public static IEnumerable<ReSendBaseRefsDto> CreateReSendDtos(this Model model, Rtu rtu, CurrentUser currentUser)
        {
            foreach (var trace in model.Traces
                         .Where(t => t.RtuId == rtu.Id && t.IsAttached && t.HasAnyBaseRef))
            {
                var dto = new ReSendBaseRefsDto()
                {
                    ConnectionId = currentUser.ConnectionId,
                    RtuId = rtu.Id,
                    RtuMaker = rtu.RtuMaker,
                    TraceId = trace.TraceId,
                    OtauPortDto = trace.OtauPort,
                    BaseRefDtos = new List<BaseRefDto>(),
                };
                foreach (var baseRef in model.BaseRefs.Where(b => b.TraceId == trace.TraceId))
                {
                    dto.BaseRefDtos.Add(new BaseRefDto()
                    {
                        SorFileId = baseRef.SorFileId,

                        Id = baseRef.TraceId,
                        BaseRefType = baseRef.BaseRefType,
                        Duration = baseRef.Duration,
                        SaveTimestamp = baseRef.SaveTimestamp,
                        UserName = baseRef.UserName,
                    });
                }
                yield return dto;
            }
        }
    }
}

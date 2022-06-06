using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class WcfServiceInClient : IWcfServiceInClient
    {
        private readonly IMyLog _logFile;
        private readonly RtuStateViewsManager _rtuStateViewsManager;
        private readonly Heartbeater _heartbeater;
        private readonly ClientPoller _clientPoller;
        private readonly ClientMeasurementViewModel _clientMeasurementViewModel;
        private readonly AutoBaseViewModel _autoBaseViewModel;
        private readonly RtuAutoBaseViewModel _rtuAutoBaseViewModel;
        private readonly IWcfServiceCommonC2D _commonC2DWcfManager;
        private readonly CurrentUser _currentUser;
        private readonly WaitViewModel _waitViewModel;
        private readonly IWindowManager _windowManager;

        public WcfServiceInClient(IMyLog logFile, RtuStateViewsManager rtuStateViewsManager, 
            Heartbeater heartbeater, ClientPoller clientPoller, CurrentUser currentUser,
            ClientMeasurementViewModel clientMeasurementViewModel, 
            AutoBaseViewModel autoBaseViewModel, RtuAutoBaseViewModel rtuAutoBaseViewModel,
            IWcfServiceCommonC2D commonC2DWcfManager,
            WaitViewModel waitViewModel, IWindowManager windowManager)
        {
            _logFile = logFile;
            _rtuStateViewsManager = rtuStateViewsManager;
            _heartbeater = heartbeater;
            _clientPoller = clientPoller;
            _clientMeasurementViewModel = clientMeasurementViewModel;
            _autoBaseViewModel = autoBaseViewModel;
            _rtuAutoBaseViewModel = rtuAutoBaseViewModel;
            _commonC2DWcfManager = commonC2DWcfManager;
            _currentUser = currentUser;
            _waitViewModel = waitViewModel;
            _windowManager = windowManager;
        }

        public Task<int> NotifyUsersRtuCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            _rtuStateViewsManager.NotifyUserRtuCurrentMonitoringStep(dto);
            return Task.FromResult(0);
        }

        public Task<int> NotifyMeasurementClientDone(ClientMeasurementResultDto dto)
        {
            _logFile.AppendLine($@"RTU returned measurement {dto.Id.First6()} through WCF connection");
            if (_clientMeasurementViewModel.IsOpen)
                _clientMeasurementViewModel.ShowReflectogram(dto.SorBytes);
            if (_autoBaseViewModel.OneMeasurementExecutor.Model.IsOpen)
                _autoBaseViewModel.OneMeasurementExecutor.ProcessMeasurementResult(dto.SorBytes);
            if (_rtuAutoBaseViewModel.OneMeasurementExecutor.Model.IsOpen)
                _rtuAutoBaseViewModel.OneMeasurementExecutor.ProcessMeasurementResult(dto.SorBytes);
            return Task.FromResult(0);
        }

        public async Task<int> SuperClientAsksClientToExit()
        {
            _logFile.AppendLine(@"SuperClient asks to exit.");
            await _commonC2DWcfManager.UnregisterClientAsync(new UnRegisterClientDto(){ConnectionId = _currentUser.ConnectionId});
            await Task.Factory.StartNew(ExitApp);
            return 0;
        }

        private void ExitApp()
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Application.Current.Dispatcher?.InvokeAsync(() => Application.Current.Shutdown());
        }

        public async Task<int> ServerAsksClientToExit(ServerAsksClientToExitDto dto)
        {
            if (dto.ToAll || dto.ConnectionId == _currentUser.ConnectionId)
                await Task.Factory.StartNew(() => LeaveApp(dto.Reason));
            return 0;
        }

        public async Task<int> BlockClientWhileDbOptimization(DbOptimizationProgressDto dto)
        {
            _logFile.AppendLine($@"DbOptimizationProgressDto received, {dto.Stage}");

            if (!_waitViewModel.IsOpen)
                await Task.Factory.StartNew(ShowWaiting);
            else
                _waitViewModel.UpdateOptimizationProgress(dto);
            return 0;
        }

        private void ShowWaiting()
        {
            _heartbeater.CancellationTokenSource.Cancel();
            _clientPoller.CancellationTokenSource.Cancel();
            _waitViewModel.Initialize(LongOperation.DbOptimization);
            Application.Current.Dispatcher?.InvokeAsync(() => _windowManager.ShowDialogWithAssignedOwner(_waitViewModel));
        }

        private async Task<int> LeaveApp(UnRegisterReason reason)
        {
            _heartbeater.CancellationTokenSource.Cancel();
            _clientPoller.CancellationTokenSource.Cancel();
            var vm = new LeaveAppViewModel();
            vm.Initialize(reason, _currentUser.UserName);
            if (Application.Current.Dispatcher != null)
                await Application.Current.Dispatcher.InvokeAsync(() => _windowManager.ShowDialogWithAssignedOwner(vm));
            return 0;
        }
    }
}

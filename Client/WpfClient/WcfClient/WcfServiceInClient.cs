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
        private readonly ClientPoller _clientPoller;
        private readonly ClientMeasurementViewModel _clientMeasurementViewModel;
        private readonly IWcfServiceCommonC2D _commonC2DWcfManager;
        private readonly WaitViewModel _waitViewModel;
        private readonly IWindowManager _windowManager;

        public WcfServiceInClient(IMyLog logFile, RtuStateViewsManager rtuStateViewsManager, ClientPoller clientPoller,
            ClientMeasurementViewModel clientMeasurementViewModel, IWcfServiceCommonC2D commonC2DWcfManager,
            WaitViewModel waitViewModel, IWindowManager windowManager)
        {
            _logFile = logFile;
            _rtuStateViewsManager = rtuStateViewsManager;
            _clientPoller = clientPoller;
            _clientMeasurementViewModel = clientMeasurementViewModel;
            _commonC2DWcfManager = commonC2DWcfManager;
            _waitViewModel = waitViewModel;
            _windowManager = windowManager;
        }

        public Task<int> NotifyUsersRtuCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            _rtuStateViewsManager.NotifyUserRtuCurrentMonitoringStep(dto);
            return Task.FromResult(0);
        }

        public Task<int> NotifyAboutMeasurementClientDone(SorBytesDto dto)
        {
            if (_clientMeasurementViewModel.IsOpen)
                _clientMeasurementViewModel.ShowReflectogram(dto.SorBytes);
            return Task.FromResult(0);
        }

        public async Task<int> AskClientToExit()
        {
            _logFile.AppendLine(@"SuperClient asks to exit.");
            await _commonC2DWcfManager.UnregisterClientAsync(new UnRegisterClientDto());
            await Task.Factory.StartNew(ExitApp);
            return 0;
        }

        private void ExitApp()
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Application.Current.Dispatcher?.InvokeAsync(() => Application.Current.Shutdown());
        }

        public async Task<int> BlockClientWhileDbOptimization(DbOptimizationProgressDto dto)
        {
            if (!_waitViewModel.IsOpen)
                await Task.Factory.StartNew(ShowWaiting);
            else
                _waitViewModel.UpdateOptimizationProgress(dto);
            return 0;
        }

        public async Task<int> UnBlockClientAfterDbOptimization()
        {
            await Task.Factory.StartNew(LeaveApp);
            return 0;
        }

        private void ShowWaiting()
        {
            _logFile.AppendLine(@"DbOptimizationProgressDto received");

            _clientPoller.CancellationTokenSource.Cancel();
            _waitViewModel.Initialize(LongOperation.DbOptimization);
            Application.Current.Dispatcher?.InvokeAsync(() => _windowManager.ShowDialogWithAssignedOwner(_waitViewModel));
        }

        private async Task<int> LeaveApp()
        {
            var vm = new LeaveAppViewModel();
            if (Application.Current.Dispatcher != null)
                await Application.Current.Dispatcher.InvokeAsync(() => _windowManager.ShowDialogWithAssignedOwner(vm));
            return 0;
        }
    }
}

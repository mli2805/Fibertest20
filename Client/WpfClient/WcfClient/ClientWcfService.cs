using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.ClientWcfServiceInterface;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ClientWcfService : IClientWcfService
    {
        private readonly IMyLog _logFile;
        private readonly RtuStateViewsManager _rtuStateViewsManager;
        private readonly ClientPoller _clientPoller;
        private readonly ClientMeasurementViewModel _clientMeasurementViewModel;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly WaitViewModel _waitViewModel;
        private readonly IWindowManager _windowManager;

        public ClientWcfService(IMyLog logFile, RtuStateViewsManager rtuStateViewsManager, ClientPoller clientPoller,
            ClientMeasurementViewModel clientMeasurementViewModel, IWcfServiceForClient c2DWcfManager,
            WaitViewModel waitViewModel, IWindowManager windowManager)
        {
            _logFile = logFile;
            _rtuStateViewsManager = rtuStateViewsManager;
            _clientPoller = clientPoller;
            _clientMeasurementViewModel = clientMeasurementViewModel;
            _c2DWcfManager = c2DWcfManager;
            _waitViewModel = waitViewModel;
            _windowManager = windowManager;
        }

        public Task<int> NotifyUsersRtuCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            _rtuStateViewsManager.NotifyUserRtuCurrentMonitoringStep(dto);
            return Task.FromResult(0);
        }

        public Task<int> NotifyAboutMeasurementClientDone(ClientMeasurementDoneDto dto)
        {
            if (_clientMeasurementViewModel.IsOpen)
                _clientMeasurementViewModel.ShowReflectogram(dto.SorBytes);
            return Task.FromResult(0);
        }

        public async Task<int> AskClientToExit()
        {
            _logFile.AppendLine(@"SuperClient asks to exit.");
            await _c2DWcfManager.UnregisterClientAsync(new UnRegisterClientDto());
            await Task.Factory.StartNew(ExitApp);
            return 0;
        }

        private void ExitApp()
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Application.Current.Dispatcher.InvokeAsync(() => Application.Current.Shutdown());
        }

        public async Task<int> BlockClientWhileDbOptimization()
        {
            await Task.Factory.StartNew(ShowWaiting);
            _logFile.AppendLine(@"BlockClientWhileDbOptimization");
            return 0;
        }

        public async Task<int> UnBlockClientAfterDbOptimization()
        {
            await Task.Factory.StartNew(LeaveApp);
            return 0;
        }

        private void ShowWaiting()
        {
            _clientPoller.CancellationTokenSource.Cancel();
            _waitViewModel.Initialize(false);
            Application.Current.Dispatcher.InvokeAsync(() => _windowManager.ShowDialogWithAssignedOwner(_waitViewModel));
            _logFile.AppendLine(@"ShowWaiting");
        }

        private async Task<int> LeaveApp()
        {
            if (_waitViewModel.IsActive)
                _waitViewModel.TryClose();
            var vm = new LeaveAppViewModel();
            await Application.Current.Dispatcher.InvokeAsync(() => _windowManager.ShowDialogWithAssignedOwner(vm));
            return 0;
        }
    }
}

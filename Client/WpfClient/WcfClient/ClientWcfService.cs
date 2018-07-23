using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Iit.Fibertest.ClientWcfServiceInterface;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ClientWcfService : IClientWcfService
    {
        private readonly IMyLog _logFile;
        private readonly RtuStateViewsManager _rtuStateViewsManager;
        private readonly ClientMeasurementViewModel _clientMeasurementViewModel;
        private readonly IWcfServiceForClient _c2DWcfManager;

        public ClientWcfService(IMyLog logFile, RtuStateViewsManager rtuStateViewsManager,
            ClientMeasurementViewModel clientMeasurementViewModel, IWcfServiceForClient c2DWcfManager)
        {
            _logFile = logFile;
            _rtuStateViewsManager = rtuStateViewsManager;
            _clientMeasurementViewModel = clientMeasurementViewModel;
            _c2DWcfManager = c2DWcfManager;
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
    }
}

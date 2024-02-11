using System;
using System.ServiceModel;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{


    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class WcfServiceForRtu : IWcfServiceForRtu
    {
        private readonly IMyLog _logFile;
        private readonly ClientsCollection _clientsCollection;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly D2CWcfManager _d2CWcfManager;
        private readonly GlobalState _globalState;
        private readonly IFtSignalRClient _ftSignalRClient;
        private readonly ClientMeasurementSender _clientMeasurementSender;


        public WcfServiceForRtu(IMyLog logFile, ClientsCollection clientsCollection,
            RtuStationsRepository rtuStationsRepository, D2CWcfManager d2CWcfManager, GlobalState globalState,
            IFtSignalRClient ftSignalRClient, ClientMeasurementSender clientMeasurementSender)
        {
            _logFile = logFile;
            _clientsCollection = clientsCollection;
            _rtuStationsRepository = rtuStationsRepository;
            _d2CWcfManager = d2CWcfManager;
            _globalState = globalState;
            _ftSignalRClient = ftSignalRClient;
            _clientMeasurementSender = clientMeasurementSender;
        }

        public async void RegisterRtuHeartbeat(RtuChecksChannelDto dto)
        {
            try
            {
                await _rtuStationsRepository.RegisterRtuHeartbeatAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine("WcfServiceForRtu.RegisterRtuHeartbeat: " + e.Message);
            }
        }

        public void NotifyUserCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            try
            {
                //                _logFile.AppendLine("WcfServiceForRtu.NotifyUserCurrentMonitoringStep: " + dto.CurrentStepDto);
                if (_globalState.IsDatacenterInDbOptimizationMode)
                    return;

                if (_clientsCollection.HasAnyWebClients())
                {
                    _ftSignalRClient.NotifyAll("NotifyMonitoringStep", dto.ToCamelCaseJson()).Wait();
                }

                var addresses = _clientsCollection.GetAllDesktopClientsAddresses();
                if (addresses == null)
                    return;

                _d2CWcfManager.SetClientsAddresses(addresses);
                _d2CWcfManager.NotifyUsersRtuCurrentMonitoringStep(dto).Wait();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("WcfServiceForRtu.NotifyUserCurrentMonitoringStep: " + e.Message);
            }
        }

        public void TransmitClientMeasurementResult(ClientMeasurementResultDto result)
        {
            if (_globalState.IsDatacenterInDbOptimizationMode)
                return;

            _clientMeasurementSender.ToClient(result).Wait();
        }
    }
}

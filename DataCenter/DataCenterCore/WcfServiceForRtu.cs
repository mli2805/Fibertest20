using System;
using System.ServiceModel;
using System.Threading;
using Iit.Fibertest.DatabaseLibrary;
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
        private readonly MeasurementsForWebNotifier _measurementsForWebNotifier;
        private readonly FtSignalRClient _ftSignalRClient;


        public WcfServiceForRtu(IniFile iniFile, IMyLog logFile, ClientsCollection clientsCollection,
            RtuStationsRepository rtuStationsRepository, D2CWcfManager d2CWcfManager, GlobalState globalState,
            MeasurementsForWebNotifier measurementsForWebNotifier, FtSignalRClient ftSignalRClient)
        {
            _logFile = logFile;
            _clientsCollection = clientsCollection;
            _rtuStationsRepository = rtuStationsRepository;
            _d2CWcfManager = d2CWcfManager;
            _globalState = globalState;
            _measurementsForWebNotifier = measurementsForWebNotifier;
            _ftSignalRClient = ftSignalRClient;

            var tid = Thread.CurrentThread.ManagedThreadId;
            _logFile.AppendLine($"RTU listener: works in thread {tid}");
        }

        public void RegisterRtuHeartbeat(RtuChecksChannelDto dto)
        {
            try
            {
                _rtuStationsRepository.RegisterRtuHeartbeatAsync(dto).Wait();
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
                _logFile.AppendLine("WcfServiceForRtu.NotifyUserCurrentMonitoringStep: " + dto.Step);
                if (_globalState.IsDatacenterInDbOptimizationMode)
                    return;

                if (_clientsCollection.HasAnyWebClients())
                {
                    _ftSignalRClient.NotifyAll("NotifyMonitoringStep", dto.ToCamelCaseJson()).Wait();
                }

                var addresses = _clientsCollection.GetDesktopClientsAddresses();
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

        public void TransmitClientMeasurementResult(SorBytesDto result)
        {
            if (_globalState.IsDatacenterInDbOptimizationMode)
                return;
            if (result.SorBytes == null || result.SorBytes.Length == 0)
            {
                _logFile.AppendLine("Bad measurement client result.");
                return;
            }
            _logFile.AppendLine($"Measurement Client result received ({result.SorBytes.Length} bytes, for clientIp {result.ClientIp})");

            try
            {
                var clientStation = _clientsCollection.GetClientStation(result.ClientIp);
                if (clientStation == null)
                {
                    _logFile.AppendLine($"There is no registered client station with IP {result.ClientIp}");
                    return;
                }
                if (clientStation.IsWebClient)
                {
                    _measurementsForWebNotifier.Push(result);
                    _logFile.AppendLine("measurement placed into queue");
                }
                else
                {
                    var addresses = _clientsCollection.GetDesktopClientsAddresses(result.ClientIp);
                    if (addresses != null)
                    {
                        _d2CWcfManager.SetClientsAddresses(addresses);
                        _d2CWcfManager.NotifyMeasurementClientDone(result).Wait();
                    }
                }

            }
            catch (Exception e)
            {
                _logFile.AppendLine("WcfServiceForRtu.TransmitClientMeasurementResult: " + e.Message);
            }
            _logFile.AppendLine("Client measurement ended");
        }
    }
}

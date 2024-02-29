using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    public class RtuLinuxPollsterThread
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly ClientsCollection _clientsCollection;
        private readonly GlobalState _globalState;
        private readonly Model _writeModel;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly ClientToLinuxRtuHttpTransmitter _clientToLinuxRtuHttpTransmitter;
        private readonly ClientMeasurementSender _clientMeasurementSender;
        private readonly MsmqMessagesProcessor _msmqMessagesProcessor;
        private readonly D2CWcfManager _d2CWcfManager;
        private readonly IFtSignalRClient _ftSignalRClient;

        private TimeSpan _gap;
        private readonly Dictionary<Guid, bool> _makLinuxRtuAccess = new Dictionary<Guid, bool>();

        public RtuLinuxPollsterThread(IniFile iniFile, IMyLog logFile, ClientsCollection clientsCollection,
            GlobalState globalState, Model writeModel, RtuStationsRepository rtuStationsRepository,
            ClientToLinuxRtuHttpTransmitter clientToLinuxRtuHttpTransmitter, ClientMeasurementSender clientMeasurementSender,
            MsmqMessagesProcessor msmqMessagesProcessor, D2CWcfManager d2CWcfManager, IFtSignalRClient ftSignalRClient)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _clientsCollection = clientsCollection;
            _globalState = globalState;
            _writeModel = writeModel;
            _rtuStationsRepository = rtuStationsRepository;
            _clientToLinuxRtuHttpTransmitter = clientToLinuxRtuHttpTransmitter;
            _clientMeasurementSender = clientMeasurementSender;
            _msmqMessagesProcessor = msmqMessagesProcessor;
            _d2CWcfManager = d2CWcfManager;
            _ftSignalRClient = ftSignalRClient;
        }

        public void Start()
        {
            var thread = new Thread(Init) { IsBackground = true };
            thread.Start();
        }

        // ReSharper disable once FunctionNeverReturns
        private async void Init()
        {
            _gap = TimeSpan.FromSeconds(_iniFile.Read(IniSection.General, IniKey.MakLinuxPollsterGap, 5));
            _logFile.AppendLine("MAK with Linux pollster started in thread");

            while (true)
            {
                if (!_globalState.IsDatacenterInDbOptimizationMode)
                {
                    await Tick();
                }
                Thread.Sleep(_gap);
            }
        }

        private async Task Tick()
        {
            var makLinuxRtus = _writeModel.Rtus.Where(r => r.MainChannel.Port == (int)TcpPorts.RtuListenToHttp && r.IsInitialized).ToList();
            var stations = await _rtuStationsRepository.GetAllRtuStations();
            foreach (var makLinuxRtu in makLinuxRtus)
            {
                try
                {
                    var station = stations.FirstOrDefault(s => s.RtuGuid == makLinuxRtu.Id);
                    if (station == null) continue;

                    var rtuDoubleAddress = station.GetRtuDoubleAddress();
                    var requestDto = new GetCurrentRtuStateDto()
                    {
                        RtuId = station.RtuGuid,
                        RtuDoubleAddress = rtuDoubleAddress,
                        LastMeasurementTimestamp = station.LastMeasurementTimestamp
                    };
                    var state = await _clientToLinuxRtuHttpTransmitter.GetRtuCurrentState(requestDto);

                    // временно логируем каждое обращение
                    // if (state == null || state.ReturnCode != ReturnCode.Ok)
                    // {
                    //     _logFile.AppendLine($"Failed to get current state of RTU {station.MainAddress}");
                    //     continue;
                    // }
                    //
                    // var word = $"{state.MonitoringResultDtos.Count}/{state.ClientMeasurementResultDtos.Count}/{state.CurrentStepDto.Step.ToString()}";
                    // _logFile.AppendLine($"RTU {station.MainAddress} returns current state {word}");
                    // потом только изменение
                    if (!SaveResultInOrderToLogOnlyChanges(state, makLinuxRtu)) continue;
                    //

                    if (state.LastInitializationResult?.Result == null)
                        continue;

                    var lastMeasurementTimestamp = state.MonitoringResultDtos.Any()
                            ? state.MonitoringResultDtos
                                        .OrderBy(r => r.TimeStamp).Last().TimeStamp
                            : DateTime.MinValue;
                    if (state.MonitoringResultDtos.Count > 0)
                    {
                        _logFile.AppendLine($"{state.MonitoringResultDtos.Count} monitoring results up to {lastMeasurementTimestamp} received ");
                        // process monitoring results in another thread
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Factory.StartNew(() => TransmitMoniResults(state.MonitoringResultDtos));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }

                    if (state.ClientMeasurementResultDtos != null)
                        foreach (var dto in state.ClientMeasurementResultDtos)
                        {
                            await _clientMeasurementSender.ToClient(dto);
                        }

                    var heartbeatDto = new RtuChecksChannelDto()
                    {
                        RtuId = makLinuxRtu.Id,
                        Version = state.LastInitializationResult.Result.Version,
                        IsMainChannel = true,
                        LastMeasurementTimestamp = lastMeasurementTimestamp, //DateTime.MinValue means no results received 
                    };

                    await _rtuStationsRepository.RegisterRtuHeartbeatAsync(heartbeatDto);

                    await NotifyUserCurrentMonitoringStep(state.CurrentStepDto);
                }
                catch (Exception e)
                {
                    _logFile.AppendLine($"Failed to get RTU {makLinuxRtu.MainChannel.ToStringA()} current state");
                    _logFile.AppendLine(e.Message);
                }
            }
        }

        private async Task TransmitMoniResults(List<MonitoringResultDto> dtos)
        {
            foreach (var dto in dtos)
            {
                _logFile.AppendLine($"Transmit moniresult with state {dto.TraceState} for trace {dto.PortWithTrace.TraceId} to _msmqMessagesProcessor");
                await _msmqMessagesProcessor.ProcessMonitoringResult(dto);
            }

        }

        private async Task NotifyUserCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            try
            {
                //                _logFile.AppendLine("WcfServiceForRtu.NotifyUserCurrentMonitoringStep: " + dto.CurrentStepDto);
                if (_globalState.IsDatacenterInDbOptimizationMode)
                    return;

                if (_clientsCollection.HasAnyWebClients())
                {
                    await _ftSignalRClient.NotifyAll("NotifyMonitoringStep", dto.ToCamelCaseJson());
                }

                var addresses = _clientsCollection.GetAllDesktopClientsAddresses();
                if (addresses == null)
                    return;

                _d2CWcfManager.SetClientsAddresses(addresses);
                await _d2CWcfManager.NotifyUsersRtuCurrentMonitoringStep(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine("RtuLinuxPollsterThread.NotifyUserCurrentMonitoringStep: " + e.Message);
            }
        }

        private bool SaveResultInOrderToLogOnlyChanges(RtuCurrentStateDto state, Rtu makLinuxRtu)
        {
            var success = state.ReturnCode != ReturnCode.D2RHttpError;
            if (!_makLinuxRtuAccess.ContainsKey(makLinuxRtu.Id))
            {
                _makLinuxRtuAccess.Add(makLinuxRtu.Id, success);
            }
            else if (_makLinuxRtuAccess[makLinuxRtu.Id] != success)
            {
                var w = success ? "Successfully" : "Failed to";
                _logFile.AppendLine($"{w} GetRtuCurrentState {makLinuxRtu.MainChannel.ToStringA()}");
                _makLinuxRtuAccess[makLinuxRtu.Id] = success;
            }

            return success;
        }

    }
}

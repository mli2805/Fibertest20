using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class RtuLinuxPollsterThread
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly GlobalState _globalState;
        private readonly Model _writeModel;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly ClientToLinuxRtuHttpTransmitter _clientToLinuxRtuHttpTransmitter;

        private TimeSpan _gap;
        private Dictionary<Guid, bool> MakLinuxRtuAccess = new Dictionary<Guid, bool>();

        public RtuLinuxPollsterThread(IniFile iniFile, IMyLog logFile,
            GlobalState globalState, Model writeModel, RtuStationsRepository rtuStationsRepository,
            ClientToLinuxRtuHttpTransmitter clientToLinuxRtuHttpTransmitter)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _globalState = globalState;
            _writeModel = writeModel;
            _rtuStationsRepository = rtuStationsRepository;
            _clientToLinuxRtuHttpTransmitter = clientToLinuxRtuHttpTransmitter;
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
                    var requestDto = new GetCurrentRtuStateDto() { RtuDoubleAddress = rtuDoubleAddress };
                    var state = await _clientToLinuxRtuHttpTransmitter.GetRtuCurrentState(requestDto);

                    if (!SaveResultInOrderToLogOnlyChanges(state, makLinuxRtu)) continue;

                    if (state.LastInitializationResult?.Result == null)
                        continue;

                    var heartbeatDto = new RtuChecksChannelDto()
                    {
                        RtuId = makLinuxRtu.Id, 
                        Version = state.LastInitializationResult.Result.Version,
                        IsMainChannel = true
                    };

                    await _rtuStationsRepository.RegisterRtuHeartbeatAsync(heartbeatDto);

                    //TODO Current Monitoring step


                }
                catch (Exception)
                {
                    _logFile.AppendLine($"Failed to get RTU {makLinuxRtu.MainChannel.ToStringA()} current state");
                }
            }
        }

        private bool SaveResultInOrderToLogOnlyChanges(RtuCurrentStateDto state, Rtu makLinuxRtu)
        {
            var success = state.ReturnCode != ReturnCode.D2RHttpError;
            if (!MakLinuxRtuAccess.ContainsKey(makLinuxRtu.Id))
            {
                MakLinuxRtuAccess.Add(makLinuxRtu.Id, success);
            }
            else if (MakLinuxRtuAccess[makLinuxRtu.Id] != success)
            {
                var w = success ? "Successfully" : "Failed to";
                _logFile.AppendLine($"{w} GetRtuCurrentState {makLinuxRtu.MainChannel.ToStringA()}");
                MakLinuxRtuAccess[makLinuxRtu.Id] = success;
            }

            return success;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.D2RtuVeexLibrary;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class VeexCompletedTestsFetcher
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly GlobalState _globalState;
        private readonly Model _writeModel;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly D2RtuVeexLayer3 _d2RtuVeexLayer3;
        private readonly VeexCompletedTestProcessor _veexCompletedTestProcessor;

        private List<Rtu> _veexRtus;
        private TimeSpan _gap;

        public VeexCompletedTestsFetcher(IniFile iniFile, IMyLog logFile, GlobalState globalState, Model writeModel,
            RtuStationsRepository rtuStationsRepository, D2RtuVeexLayer3 d2RtuVeexLayer3,
            VeexCompletedTestProcessor veexCompletedTestProcessor)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _globalState = globalState;
            _writeModel = writeModel;
            _rtuStationsRepository = rtuStationsRepository;
            _d2RtuVeexLayer3 = d2RtuVeexLayer3;
            _veexCompletedTestProcessor = veexCompletedTestProcessor;
        }

        public void Start()
        {
            var thread = new Thread(Init) { IsBackground = true };
            thread.Start();
        }

        // ReSharper disable once FunctionNeverReturns
        private void Init()
        {
            _gap = TimeSpan.FromSeconds(_iniFile.Read(IniSection.General, IniKey.AskVeexRtuEvery, 8));

            while (true)
            {
                if (!_globalState.IsDatacenterInDbOptimizationMode)
                    Tick().Wait();
                Thread.Sleep(_gap);
            }
        }

        public async Task<int> Tick()
        {
            _veexRtus = _writeModel.Rtus.Where(r => r.RtuMaker == RtuMaker.VeEX && r.IsInitialized).ToList();
            var stations = await _rtuStationsRepository.GetAllRtuStations();

            var changedStations = new List<RtuStation>();
            foreach (var rtu in _veexRtus)
            {
                var station = stations.FirstOrDefault(s => s.RtuGuid == rtu.Id);
                if (station == null) continue;

                var rtuDoubleAddress = station.GetRtuDoubleAddress();
                var utc = station.LastMeasurementTimestamp.ToUniversalTime();
                var startingFrom = utc.AddSeconds(1).ToString("O");

                // rtu can't return more than 1024 completed tests at a time, but can less, parameter limit is optional
                // limit = 1024 sometimes causes exception "A task was canceled"
                var getPortionResult = await _d2RtuVeexLayer3.GetCompletedTestsAfterTimestampAsync(rtuDoubleAddress, startingFrom, 512);
                await ProcessRequestResult(getPortionResult, station, rtu, rtuDoubleAddress);
            }

            if (changedStations.Any())
                return await _rtuStationsRepository.SaveAvailabilityChanges(changedStations);

            return 0;
        }

        private async Task ProcessRequestResult(HttpRequestResult getPortionResult, RtuStation station, Rtu rtu, DoubleAddress rtuDoubleAddress)
        {
            if (getPortionResult.IsSuccessful)
            {
                if (getPortionResult.ResponseObject is CompletedTestPortion portion)
                {
                    if (portion.items.Count > 0)
                        _logFile.AppendLine($"RTU {station.MainAddress} returned " +
                              $"portion of {portion.items.Count} from {portion.total} completed tests", 0, 3);
                    foreach (var completedTest in portion.items)
                        await _veexCompletedTestProcessor.ProcessOneCompletedTest(completedTest, rtu, rtuDoubleAddress);

                    if (portion.items.Any())
                        station.LastMeasurementTimestamp = portion.items.Last().started.ToLocalTime();
                    station.LastConnectionByMainAddressTimestamp = DateTime.Now;

                    await _rtuStationsRepository.SaveAvailabilityChanges(new List<RtuStation>() { station });
                }
            }
            else
                _logFile.AppendLine($"RTU {station.MainAddress} returned {getPortionResult.ErrorMessage}");
        }
    }
}
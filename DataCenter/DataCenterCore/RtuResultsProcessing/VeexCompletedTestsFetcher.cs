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
        private readonly VeexCompletedTestsProcessorThread _veexCompletedTestsProcessorThread;

        private List<Rtu> _veexRtus;
        private TimeSpan _gap;

        public VeexCompletedTestsFetcher(IniFile iniFile, IMyLog logFile, GlobalState globalState, Model writeModel,
            RtuStationsRepository rtuStationsRepository, D2RtuVeexLayer3 d2RtuVeexLayer3,
            VeexCompletedTestsProcessorThread veexCompletedTestsProcessorThread)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _globalState = globalState;
            _writeModel = writeModel;
            _rtuStationsRepository = rtuStationsRepository;
            _d2RtuVeexLayer3 = d2RtuVeexLayer3;
            _veexCompletedTestsProcessorThread = veexCompletedTestsProcessorThread;
        }

        public void Start()
        {
            var thread = new Thread(Init) { IsBackground = true };
            thread.Start();
        }

        // ReSharper disable once FunctionNeverReturns
        private async void Init()
        {
            _gap = TimeSpan.FromSeconds(_iniFile.Read(IniSection.General, IniKey.AskVeexRtuEvery, 8));

            while (true)
            {
                if (!_globalState.IsDatacenterInDbOptimizationMode)
                {
                    await Tick();
                }
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

                // var getPortionResult = await _d2RtuVeexLayer3.GetCompletedTestsAfterTimestampAsync(rtuDoubleAddress, startingFrom, 512);
                // await ProcessRequestResult(getPortionResult, station, rtu, rtuDoubleAddress);

                await Task.Factory.StartNew(() => FetchOneRtu(rtu, station, rtuDoubleAddress, startingFrom));
            }

            if (changedStations.Any())
                return await _rtuStationsRepository.SaveAvailabilityChanges(changedStations);

            return 0;
        }

        private async void FetchOneRtu(Rtu rtu, RtuStation station, DoubleAddress rtuDoubleAddress, string startingFrom)
        {
            // rtu can't return more than 1024 completed tests at a time, but can less, parameter limit is optional
            var getPortionResult = await _d2RtuVeexLayer3.GetCompletedTestsAfterTimestampAsync(rtuDoubleAddress, startingFrom, 1024);
            if (!getPortionResult.IsSuccessful)
            {
                _logFile.AppendLine($"Failed to connect RTU4100 {rtuDoubleAddress.Main.ToStringA()} ({getPortionResult.HttpStatusCode}; {getPortionResult.ErrorMessage})", 0, 3);
                return;
            }

            if (!(getPortionResult.ResponseObject is CompletedTestPortion portion))
            {
                _logFile.AppendLine($"Invalid CompletedTestPortion");
                return;
            }

            if (portion.total > 0)
                _logFile.AppendLine($"got portion of {portion.total} measurements from RTU {rtu.Title}");
            foreach (var completedTest in portion.items)
            {
                _veexCompletedTestsProcessorThread.CompletedTests.Enqueue(new Tuple<CompletedTest, Rtu>(completedTest, rtu));
            }

            if (portion.items.Any())
                station.LastMeasurementTimestamp = portion.items.Last().started.ToLocalTime();
            station.LastConnectionByMainAddressTimestamp = DateTime.Now;

            await _rtuStationsRepository.SaveAvailabilityChanges(new List<RtuStation>() { station });
        }
    }
}
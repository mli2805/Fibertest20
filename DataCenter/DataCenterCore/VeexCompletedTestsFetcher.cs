﻿using System;
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
        private readonly Model _writeModel;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly D2RtuVeexLayer3 _d2RtuVeexLayer3;
        private readonly VeexCompletedTestProcessor _veexCompletedTestProcessor;

        private List<Rtu> _veexRtus;
        private TimeSpan _gap;

        public VeexCompletedTestsFetcher(IniFile iniFile, IMyLog logFile, Model writeModel,
            RtuStationsRepository rtuStationsRepository, D2RtuVeexLayer3 d2RtuVeexLayer3,
            VeexCompletedTestProcessor veexCompletedTestProcessor)
        {
            _iniFile = iniFile;
            _logFile = logFile;
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
                Tick().Wait();
                Thread.Sleep(_gap);
            }
        }

        private async Task<int> Tick()
        {
            _veexRtus = _writeModel.Rtus.Where(r => r.RtuMaker == RtuMaker.VeEX && r.IsInitialized).ToList();
            var stations = await _rtuStationsRepository.GetAllRtuStations();

            var changedStations = new List<RtuStation>();
            foreach (var rtu in _veexRtus)
            {
                var station = stations.FirstOrDefault(s => s.RtuGuid == rtu.Id);
                if (station == null) continue;

                var rtuDoubleAddress = station.GetRtuDoubleAddress();
                var utc = station.LastConnectionByMainAddressTimestamp.ToUniversalTime();
                var startingFrom = utc.AddSeconds(1).ToString("O");

                // rtu can't return more than 1024 completed tests at a time, but can less, parameter limit is optional
                var getPortionResult = await _d2RtuVeexLayer3.GetCompletedTestsAfterTimestamp(rtuDoubleAddress, startingFrom, 2048);
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
                    _logFile.AppendLine($"RTU {station.MainAddress} returned " +
                              $"portion of {portion.items.Count} from {portion.total} completed tests", 0, 3);
                    foreach (var completedTest in portion.items)
                        await _veexCompletedTestProcessor.ProcessOneCompletedTest(completedTest, rtu, rtuDoubleAddress);

                    if (portion.items.Any())
                        station.LastConnectionByMainAddressTimestamp = portion.items.Last().started.ToLocalTime();

                    if (await _rtuStationsRepository.RefreshStationLastConnectionTime(station) > 0)
                        _logFile.AppendLine("last fetched measurement time refreshed successfully", 0, 3);
                }
            }
            else
                _logFile.AppendLine($"RTU {station.MainAddress} returned {getPortionResult.ErrorMessage}");
        }
    }
}
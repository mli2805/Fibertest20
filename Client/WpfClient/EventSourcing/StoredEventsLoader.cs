﻿using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Newtonsoft.Json;
using NEventStore;

namespace Iit.Fibertest.Client
{
    public class StoredEventsLoader
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All};

        private readonly IMyLog _logFile;
        private readonly ILocalDbManager _localDbManager;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly Model _readModel;
        private readonly SnapshotsLoader _snapshotsLoader;
        private readonly EventsOnTreeExecutor _eventsOnTreeExecutor;
        private readonly OpticalEventsExecutor _opticalEventsExecutor;
        private readonly NetworkEventsDoubleViewModel _networkEventsDoubleViewModel;
        private readonly BopNetworkEventsDoubleViewModel _bopNetworkEventsDoubleViewModel;
        private readonly RenderingManager _renderingManager;

        public StoredEventsLoader(IMyLog logFile, ILocalDbManager localDbManager,
            IWcfServiceDesktopC2D c2DWcfManager,
            Model readModel, SnapshotsLoader snapshotsLoader,
            EventsOnTreeExecutor eventsOnTreeExecutor,
            OpticalEventsExecutor opticalEventsExecutor,
            NetworkEventsDoubleViewModel networkEventsDoubleViewModel,
            BopNetworkEventsDoubleViewModel bopNetworkEventsDoubleViewModel,
            RenderingManager renderingManager)
        {
            _logFile = logFile;
            _localDbManager = localDbManager;
            _c2DWcfManager = c2DWcfManager;
            _readModel = readModel;
            _snapshotsLoader = snapshotsLoader;
            _eventsOnTreeExecutor = eventsOnTreeExecutor;
            _opticalEventsExecutor = opticalEventsExecutor;
            _networkEventsDoubleViewModel = networkEventsDoubleViewModel;
            _bopNetworkEventsDoubleViewModel = bopNetworkEventsDoubleViewModel;
            _renderingManager = renderingManager;
        }

        // TwoComponentLoading
        public async Task<int> TwoComponentLoading()
        {
            var lastEventFromSnapshot = await _snapshotsLoader.LoadAndApplySnapshot();

            var lastLoadedEvent = lastEventFromSnapshot + await LoadAndApplyEvents(lastEventFromSnapshot);
            var currentEventNumber = await DownloadAndApplyEvents(lastLoadedEvent);

            _renderingManager.Initialize();
            await _renderingManager.RenderCurrentZoneOnApplicationStart();
            return currentEventNumber;
        }

      
        private async Task<int> LoadAndApplyEvents(int lastLoadedEvent)
        {
            var jsonsInCache = await _localDbManager.LoadEvents(lastLoadedEvent);
            _logFile.AppendLine($@"{jsonsInCache.Length} events in cache should be applying...");
            return ApplyBatch(jsonsInCache);
        }

     
        private async Task<int> DownloadAndApplyEvents(int currentEventNumber)
        {
            _logFile.AppendLine($@"Downloading events from {currentEventNumber}...");
            string[] events;
            do
            {
                events = await _c2DWcfManager.GetEvents(new GetEventsDto() {Revision = currentEventNumber});
                await _localDbManager.SaveEvents(events, currentEventNumber + 1);
                currentEventNumber = currentEventNumber + ApplyBatch(events);
            } while (events.Length != 0);

            _logFile.AppendLine($@"{currentEventNumber} is last event number found in Cache + Db");
            return currentEventNumber;
        }

        private int ApplyBatch(string[] events)
        {
            for (var i = 0; i < events.Length; i++)
            {
                var json = events[i];
                var msg = (EventMessage) JsonConvert.DeserializeObject(json, JsonSerializerSettings);
                var evnt = msg.Body;

                try
                {
                    _readModel.Apply(evnt);
                    _eventsOnTreeExecutor.Apply(evnt);
                    _opticalEventsExecutor.Apply(evnt);
                    _networkEventsDoubleViewModel.Apply(evnt);
                    _bopNetworkEventsDoubleViewModel.Apply(evnt);
                }
                catch (Exception e)
                {
                    _logFile.AppendLine(e.Message);
                    var header = @"Timestamp";
                    _logFile.AppendLine(
                        $@"Exception thrown while processing event with timestamp {msg.Headers[header]} \n {
                                evnt.GetType().FullName
                            }");
                }
            }

            return events.Length;
        }
    }
}
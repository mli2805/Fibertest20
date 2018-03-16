using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Newtonsoft.Json;
using NEventStore;
using PrivateReflectionUsingDynamic;

namespace Iit.Fibertest.Client
{
    public class ReadyEventsLoader
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        private readonly IMyLog _logFile;
        private readonly ILocalDbManager _localDbManager;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly ReadModel _readModel;
        private readonly GraphReadModel _graphReadModel;
        private readonly EventsOnModelExecutor _eventsOnModelExecutor;
        private readonly TreeOfRtuModel _treeOfRtuModel;
        private readonly OpticalEventsExecutor _opticalEventsExecutor;
        private readonly NetworkEventsDoubleViewModel _networkEventsDoubleViewModel;
        private readonly BopNetworkEventsDoubleViewModel _bopNetworkEventsDoubleViewModel;

        public ReadyEventsLoader(IMyLog logFile, ILocalDbManager localDbManager, IWcfServiceForClient c2DWcfManager, 
            ReadModel readModel, GraphReadModel graphReadModel,
            EventsOnModelExecutor eventsOnModelExecutor, TreeOfRtuModel treeOfRtuModel,
            OpticalEventsExecutor opticalEventsExecutor, 
            NetworkEventsDoubleViewModel networkEventsDoubleViewModel,  BopNetworkEventsDoubleViewModel bopNetworkEventsDoubleViewModel)
        {
            _logFile = logFile;
            _localDbManager = localDbManager;
            _c2DWcfManager = c2DWcfManager;
            _readModel = readModel;
            _graphReadModel = graphReadModel;
            _eventsOnModelExecutor = eventsOnModelExecutor;
            _treeOfRtuModel = treeOfRtuModel;
            _opticalEventsExecutor = opticalEventsExecutor;
            _networkEventsDoubleViewModel = networkEventsDoubleViewModel;
            _bopNetworkEventsDoubleViewModel = bopNetworkEventsDoubleViewModel;
        }

        public async Task<int> Load()
        {
            var currentEventNumber = await LoadFromCache();
            currentEventNumber = await LoadFromDb(currentEventNumber);
            RenderAllGraph();
            return currentEventNumber;
        }

        private async Task<int> LoadFromCache()
        {
            var jsonsInCache = await _localDbManager.LoadEvents();
            _logFile.AppendLine($@"There are {jsonsInCache.Length} events in cache. Applying...");
            return ApplyBatch(jsonsInCache);
        }

        private async Task<int> LoadFromDb(int currentEventNumber)
        {
            string[] events;
            do
            {
                events = await _c2DWcfManager.GetEvents(currentEventNumber);
                await _localDbManager.SaveEvents(events);
                currentEventNumber = currentEventNumber + ApplyBatch(events);
            }
            while (events.Length != 0);
            _logFile.AppendLine($@"{currentEventNumber} events found in Cache + Db");
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
                    _eventsOnModelExecutor.Apply(evnt);
                    _treeOfRtuModel.AsDynamic().Apply(evnt);
                    _opticalEventsExecutor.Apply(evnt);
                    if (evnt is NetworkEventAdded ee) _networkEventsDoubleViewModel.Apply(ee);
                    if (evnt is BopNetworkEventAdded bee) _bopNetworkEventsDoubleViewModel.Apply(bee);
                }
                catch (Exception e)
                {
                    _logFile.AppendLine(e.Message);
                    var header = @"Timestamp";
                    _logFile.AppendLine(
                        $@"Exception thrown while processing event with timestamp {msg.Headers[header]}");
                }
            }

            return events.Length;
        }

        // some sort of parsing snapshot

        private void RenderAllGraph()
        {
            List<NodeVm> nodeVms = new List<NodeVm>();

            foreach (var node in _readModel.Nodes)
            {
                nodeVms.Add(new NodeVm()
                {
                    Id = node.Id,
                    Title = node.Title,
                    Position = node.Position,
                    Type = node.TypeOfLastAddedEquipment,
                });
            }

            List<FiberVm> fiberVms = new List<FiberVm>();

            foreach (var fiber in _readModel.Fibers)
            {
                var fiberVm = new FiberVm()
                {
                    Id = fiber.Id,
                    Node1 = nodeVms.First(n=>n.Id == fiber.Node1),
                    Node2 = nodeVms.First(n=>n.Id == fiber.Node2),
                    States = new Dictionary<Guid, FiberState>(),
                    TracesWithExceededLossCoeff = fiber.TracesWithExceededLossCoeff,
                };
                foreach (var pair in fiber.States)
                    fiberVm.States.Add(pair.Key, pair.Value);

                fiberVms.Add(fiberVm);
            }

            foreach (var trace in _readModel.Traces)
            {
                var fiberIds = _readModel.GetTraceFibers(trace).Select(f=>f.Id).ToList();
                foreach (var fiberVm in fiberVms)
                {
                    if (fiberIds.Contains(fiberVm.Id))
                        fiberVm.SetState(trace.Id, trace.State);
                }
            }

            foreach (var nodeVm in nodeVms)
                _graphReadModel.Data.Nodes.Add(nodeVm);
            foreach (var fiberVm in fiberVms)
                _graphReadModel.Data.Fibers.Add(fiberVm);
        }

    }
}
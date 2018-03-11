using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GMap.NET;
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
        private readonly CurrentUser _currentUser;
        private readonly EventsOnModelExecutor _eventsOnModelExecutor;
        private readonly TreeOfRtuModel _treeOfRtuModel;
        private readonly EventsOnGraphExecutor _eventsOnGraphExecutor;

        public ReadyEventsLoader(IMyLog logFile, ILocalDbManager localDbManager, IWcfServiceForClient c2DWcfManager, 
            ReadModel readModel, CurrentUser currentUser,
            EventsOnModelExecutor eventsOnModelExecutor, TreeOfRtuModel treeOfRtuModel, EventsOnGraphExecutor eventsOnGraphExecutor)
        {
            _logFile = logFile;
            _localDbManager = localDbManager;
            _c2DWcfManager = c2DWcfManager;
            _readModel = readModel;
            _currentUser = currentUser;
            _eventsOnModelExecutor = eventsOnModelExecutor;
            _treeOfRtuModel = treeOfRtuModel;
            _eventsOnGraphExecutor = eventsOnGraphExecutor;
        }

        public async Task<int> Load()
        {
            var currentEventNumber = await LoadFromCache();
            currentEventNumber = await LoadFromDb(currentEventNumber);
            DrawGraphFromReadModel();
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
            foreach (var json in events)
            {
                var msg = (EventMessage)JsonConvert.DeserializeObject(json, JsonSerializerSettings);
                var evnt = msg.Body;

                try
                {
                    _eventsOnModelExecutor.Apply(evnt);
                    _treeOfRtuModel.AsDynamic().Apply(evnt);
                    //temporary
                    _eventsOnGraphExecutor.Apply(evnt);
                }
                catch (Exception e)
                {
                    _logFile.AppendLine(e.Message);
                    var header = @"Timestamp";
                    _logFile.AppendLine($@"Exception thrown while processing event with timestamp {msg.Headers[header]}");
                }
            }

            return events.Length;
        }

        // some sort of parsing snapshot
        private void DrawGraphFromReadModel()
        {
            foreach (var rtu in _readModel.Rtus)
            {
                if ((_currentUser.ZoneId == Guid.Empty || _currentUser.ZoneId == rtu.ZoneId) && !rtu.ShouldBeHidden)
                    DrawRtuFromReadModel(rtu.Id);
            }
        }

        private void DrawRtuFromReadModel(Guid rtuId)
        {

        }

        private void RenderAllGraph()
        {
            List<NodeVm> nodeVms = new List<NodeVm>();

            foreach (var node in _readModel.Nodes)
            {
                nodeVms.Add(new NodeVm()
                {
                    Id = node.Id,
                    Title = node.Title,
                    Position = new PointLatLng(node.Latitude, node.Longitude),
                    Type = node.TypeOfLastAddedEquipment,
                });
            }

            List<FiberVm> fiberVms = new List<FiberVm>();

            foreach (var fiber in _readModel.Fibers)
            {
                fiberVms.Add(new FiberVm()
                {
                    Id = fiber.Id,
                    Node1 = nodeVms.First(n=>n.Id == fiber.Node1),
                    Node2 = nodeVms.First(n=>n.Id == fiber.Node2),
                });
            }
        }

    }
}
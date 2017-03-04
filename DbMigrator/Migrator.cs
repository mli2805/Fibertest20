using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Iit.Fibertest.Graph;

namespace Convertor
{
    public class Migrator
    {
        private readonly Db _db;
        private Dictionary<int, Guid> _nodesDictionary = new Dictionary<int, Guid>();
        private Dictionary<int, Guid> _equipmentsDictionary = new Dictionary<int, Guid>();
        private Dictionary<int, Guid> _tracesDictionary = new Dictionary<int, Guid>();

        private List<TraceAdded> _traceAddedEvents = new List<TraceAdded>();

        public Migrator(Db db)
        {
            _db = db;
        }

        public void Go()
        {
            Encoding win1251 = Encoding.GetEncoding("Windows-1251");
            string[] lines = File.ReadAllLines(@"export.txt", win1251);

            foreach (var line in lines)
            {
                var parts = line.Split('|')[1].Trim().Split(';');
                switch (parts[0])
                {
                    case "NODES::":
                        ParseNode(parts);
                        break;
                    case "FIBERS::":
                        ParseFibers(parts);
                        break;
                    case "EQUIPMENTS::":
                        ParseEquipments(parts);
                        break;
                    case "TRACES::":
                        ParseTrace(parts);
                        break;
                    case "TRACES::N":
                        ParseTraceNodes(parts);
                        break;
                    case "TRACES::E":
                        ParseTraceEquipments(parts);
                        break;
                }
            }
        }

        private void ParseNode(string[] parts)
        {
            var nodeId = Int32.Parse(parts[1]);
            var nodeGuid = Guid.NewGuid();
            _nodesDictionary.Add(nodeId, nodeGuid);
            var type = (EquipmentType)Int32.Parse(parts[2]);

            if (type == EquipmentType.Rtu)
            {
                var rtuGuid = Guid.NewGuid();
                _db.Add(new RtuAtGpsLocationAdded() { Id = rtuGuid, NodeId = nodeGuid, Latitude = 3, Longitude = 4, });
                _db.Add(new RtuUpdated()
                {
                    Id = rtuGuid,
                    Title = parts[5],
                    Comment = parts[6]
                });
            }
            else
                _db.Add(new NodeAdded() { Id = nodeGuid, Latitude = 3, Longitude = 4 });
            _db.Add(new NodeUpdated() { Id = nodeGuid, Title = parts[5], Comment = parts[6] });
        }

        private void ParseFibers(string[] parts)
        {
            var nodeId1 = Int32.Parse(parts[1]);
            var nodeId2 = Int32.Parse(parts[2]);

            var evnt = new FiberAdded()
            {
                Id = Guid.NewGuid(),
                Node1 = _nodesDictionary[nodeId1],
                Node2 = _nodesDictionary[nodeId2]
            };
            _db.Add(evnt);
        }

        private void ParseEquipments(string[] parts)
        {
            var equipmentId = Int32.Parse(parts[1]);
            var equipmentGuid = Guid.NewGuid();
            _equipmentsDictionary.Add(equipmentId, equipmentGuid);

            var nodeId = Int32.Parse(parts[2]);
            var type = (EquipmentType)Int32.Parse(parts[3]);

            var evnt = new EquipmentIntoNodeAdded()
            {
                Id = equipmentGuid, 
                NodeId = _nodesDictionary[nodeId],
                Type = type,
                Title = parts[4],
                Comment = parts[5]
            };
            _db.Add(evnt);
        }

        private void ParseTrace(string[] parts)
        {
            var traceId = Int32.Parse(parts[1]);
            var traceGuid = Guid.NewGuid();
            _tracesDictionary.Add(traceId, traceGuid);

            var evnt = new TraceAdded()
            {
                Id = traceGuid,
                Title = parts[2],
                Comment = parts[3],
            };

            _traceAddedEvents.Add(evnt);
        }

        private void ParseTraceNodes(string[] parts)
        {
            var traceId = Int32.Parse(parts[1]);
            var evnt = _traceAddedEvents.First(e => e.Id == _tracesDictionary[traceId]);
        }

        private void ParseTraceEquipments(string[] parts)
        {
            var traceId = Int32.Parse(parts[1]);
            var evnt = _traceAddedEvents.First(e => e.Id == _tracesDictionary[traceId]);
        }


    }
}

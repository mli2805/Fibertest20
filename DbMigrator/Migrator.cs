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
        private readonly Dictionary<int, Guid> _nodesDictionary = new Dictionary<int, Guid>();
        private readonly Dictionary<Guid, Guid> _nodeToRtuDictionary = new Dictionary<Guid, Guid>();
        private readonly Dictionary<int, Guid> _equipmentsDictionary = new Dictionary<int, Guid>();
        private readonly Dictionary<int, Guid> _tracesDictionary = new Dictionary<int, Guid>();

        private readonly List<object> _traceEventsUnderConstruction = new List<object>();

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
                var logLineParts = line.Split('|');
                if (logLineParts.Length == 1)
                    continue;
                var parts = logLineParts[1].Trim().Split(';');
                switch (parts[0])
                {
                    case "NODES::":
                        ParseNode(parts);
                        break;
                    case "RTU::":
                        ParseRtu(parts);
                        break;
                    case "FIBERS::":
                        ParseFibers(parts);
                        break;
                    case "EQUIPMENTS::":
                        ParseEquipments(parts);
                        break;
                }
            }

            // second pass - all nodes and equipment loaded, now we can process traces
            foreach (var line in lines)
            {
                var logLineParts = line.Split('|');
                if (logLineParts.Length == 1)
                    continue;
                var parts = logLineParts[1].Trim().Split(';');
                switch (parts[0])
                {
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

            _traceEventsUnderConstruction.ForEach(e => _db.Add(e));
        }

        private void ParseRtu(string[] parts)
        {
            var nodeId = Int32.Parse(parts[1]);
            var nodeGuid = _nodesDictionary[nodeId];
            var rtuGuid = _nodeToRtuDictionary[nodeGuid];

            _db.Add(new RtuInitialized()
            {
                Id = rtuGuid,
                OwnPortCount = Int32.Parse(parts[2]),
                FullPortCount = Int32.Parse(parts[2]),
                Serial = Int32.Parse(parts[4]).ToString(),
                MainChannel = new NetAddress() { Ip4Address = parts[5], Port = Int32.Parse(parts[6]) },
                OtdrNetAddress = new NetAddress() { Ip4Address = parts[7], Port = Int32.Parse(parts[8]) },
                ReserveChannel = new NetAddress() { Ip4Address = parts[9], Port = Int32.Parse(parts[10]) },
            });
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
                _db.Add(new RtuAtGpsLocationAdded()
                {
                    Id = rtuGuid,
                    NodeId = nodeGuid,
                    Latitude = double.Parse(parts[3]),
                    Longitude = double.Parse(parts[4]),
                });
                _db.Add(new RtuUpdated()
                {
                    Id = rtuGuid,
                    Title = parts[5],
                    Comment = parts[6]
                });
                _nodeToRtuDictionary.Add(nodeGuid, rtuGuid);
            }
            else
            {
                _db.Add(new NodeAdded()
                {
                    Id = nodeGuid,
                    Latitude = double.Parse(parts[3]),
                    Longitude = double.Parse(parts[4]),
                });
                _db.Add(new NodeUpdated()
                {
                    Id = nodeGuid, Title = parts[5],
                    Comment = parts[6]
                });
            }

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

            _traceEventsUnderConstruction.Add(
                new TraceAdded()
                {
                    Id = traceGuid,
                    Title = parts[3],
                    Comment = parts[4],
                });


            var port = Int32.Parse(parts[2]);
            if (port != -1)
                _traceEventsUnderConstruction.Add(
                    new TraceAttached()
                    {
                        TraceId = traceGuid,
                        Port = port
                    });
        }

        private void ParseTraceNodes(string[] parts)
        {
            var traceId = Int32.Parse(parts[1]);
            var evnt = (TraceAdded)_traceEventsUnderConstruction.First(e => e is TraceAdded && ((TraceAdded)e).Id == _tracesDictionary[traceId]);
            for (int i = 2; i < parts.Length; i++)
            {
                if (parts[i] == "")
                    continue;
                evnt.Nodes.Add(_nodesDictionary[Int32.Parse(parts[i])]);
            }
        }

        private void ParseTraceEquipments(string[] parts)
        {
            var traceId = Int32.Parse(parts[1]);
            var evnt = (TraceAdded)_traceEventsUnderConstruction.First(e => e is TraceAdded && ((TraceAdded)e).Id == _tracesDictionary[traceId]);
            var rtuGuid = _nodeToRtuDictionary[_nodesDictionary[Int32.Parse(parts[2])]];
            evnt.RtuId = rtuGuid;
            evnt.Equipments.Add(rtuGuid);
            for (int i = 3; i < parts.Length; i++)
            {
                if (parts[i] == "")
                    continue;
                var equipmentId = Int32.Parse(parts[i]);
                evnt.Equipments.Add(equipmentId == -1 ? Guid.Empty : _equipmentsDictionary[equipmentId]);
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using Iit.Fibertest.Graph;

namespace Convertor
{
    public class Migrator
    {
        private readonly Db _db;
        private Dictionary<int, Guid> _nodesDictionary = new Dictionary<int, Guid>();

        public Migrator(Db db)
        {
            _db = db;
        }

        public void Go()
        {
            string[] lines = File.ReadAllLines(@"export.txt");

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
                _db.Add(new RtuAtGpsLocationAdded() {Id = rtuGuid, NodeId = nodeGuid, Latitude = 3, Longitude = 4,});
                _db.Add(new RtuUpdated() {Id = rtuGuid, Title = parts[5], Comment = parts[6]});
            }    
            else 
                _db.Add(new NodeAdded() {Id = nodeGuid, Latitude = 3, Longitude = 4});
                _db.Add(new NodeUpdated() {Id = nodeGuid, Title = parts[5], Comment = parts[6]});
        }

        private void ParseFibers(string[] parts)
        {
            
        }

        private void ParseEquipments(string[] parts)
        {
            
        }

        private void ParseTrace(string[] parts)
        {
            
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace KadastrLoader
{
    public class ConpointParser
    {
        private readonly IMyLog _logFile;
        private readonly KadastrDbProvider _kadastrDbProvider;
        private readonly C2DWcfManager _c2DWcfManager;
        private readonly LoadedAlready _loadedAlready;

        public ConpointParser(IMyLog logFile, KadastrDbProvider kadastrDbProvider,
            C2DWcfManager c2DWcfManager, LoadedAlready loadedAlready)
        {
            _logFile = logFile;
            _kadastrDbProvider = kadastrDbProvider;
            _c2DWcfManager = c2DWcfManager;
            _loadedAlready = loadedAlready;
        }

        public async Task<int> ParseConpoints(string folder)
        {
            var count = 0;
            var filename = folder + @"\conpoints.csv";

            var lines = File.ReadAllLines(filename);
            _logFile.AppendLine($"{lines.Length} lines found in conpoints.csv");
            foreach (var line in lines)
            {
                if (await ProcessOneLine(line) == null) count++;
            }
            return count;
        }

        private async Task<string> ProcessOneLine(string line)
        {
            var fields = line.Split(';');
            if (fields.Length < 4) return "invalid line";

            if (!int.TryParse(fields[3], out int conpointInKadastrId)) return "invalid line";
            if (!int.TryParse(fields[0], out int wellInKadastrId)) return "invalid line";
            if (_loadedAlready.Conpoints.FirstOrDefault(c => c.InKadastrId == conpointInKadastrId) != null)
                return "conpoint exists already";

            var conpoint = new Conpoint()
            {
                InKadastrId = conpointInKadastrId,
            };
            _loadedAlready.Conpoints.Add(conpoint);
            await _kadastrDbProvider.AddConpoint(conpoint);

            if (!int.TryParse(fields[2], out int conType) || conType == 0) return "invalid line";
            
            // conType == 1 - fork
            // conType == 2 - joint closure

            var well = _loadedAlready.Wells.FirstOrDefault(w => w.InKadastrId == wellInKadastrId);
            if (well == null) return "no such well";
            var cmd = new AddEquipmentIntoNode();
            cmd.NodeId = well.InFibertestId;
            cmd.EquipmentId = Guid.NewGuid();
            cmd.Type = EquipmentType.Closure;
            return await _c2DWcfManager.SendCommandAsObj(cmd);
        }

    }
}
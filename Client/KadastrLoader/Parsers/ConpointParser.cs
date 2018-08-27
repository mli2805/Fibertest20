using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;

namespace KadastrLoader
{
    public class ConpointParser
    {
        private readonly KadastrDbProvider _kadastrDbProvider;
        private readonly C2DWcfManager _c2DWcfManager;
        private readonly LoadedAlready _loadedAlready;

        public ConpointParser(KadastrDbProvider kadastrDbProvider,
            C2DWcfManager c2DWcfManager, LoadedAlready loadedAlready)
        {
            _kadastrDbProvider = kadastrDbProvider;
            _c2DWcfManager = c2DWcfManager;
            _loadedAlready = loadedAlready;
        }

        public void ParseConpoints(string folder, BackgroundWorker worker)
        {
            var count = 0;
            var filename = folder + @"\conpoints.csv";

            var lines = File.ReadAllLines(filename);
            var str = string.Format(Resources.SID__0__lines_found_in_conpoints_csv, lines.Length);
            worker.ReportProgress(0, str);
            foreach (var line in lines)
            {
                if (ProcessOneLine(line) == null) count++;
            }
            worker.ReportProgress(0, string.Format(Resources.SID__0__conpoints_applied, count));
        }

        private string ProcessOneLine(string line)
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
            _kadastrDbProvider.AddConpoint(conpoint).Wait();

            if (!int.TryParse(fields[2], out int conType) || conType == 0) return "invalid line";
            
            // conType == 1 - fork
            // conType == 2 - joint closure

            var well = _loadedAlready.Wells.FirstOrDefault(w => w.InKadastrId == wellInKadastrId);
            if (well == null) return "no such well";
            var cmd = new AddEquipmentIntoNode();
            cmd.NodeId = well.InFibertestId;
            cmd.EquipmentId = Guid.NewGuid();
            cmd.Type = EquipmentType.Closure;
            return _c2DWcfManager.SendCommandAsObj(cmd).Result;
        }

    }
}
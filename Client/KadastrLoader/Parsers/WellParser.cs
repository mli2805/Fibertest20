using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace KadastrLoader
{
    public class WellParser
    {
        private readonly IMyLog _logFile;
        private readonly KadastrDbProvider _kadastrDbProvider;
        private readonly C2DWcfManager _c2DWcfManager;
        private readonly LoadedAlready _loadedAlready;

        public WellParser(IMyLog logFile, KadastrDbProvider kadastrDbProvider, 
            C2DWcfManager c2DWcfManager, LoadedAlready loadedAlready)
        {
            _logFile = logFile;
            _kadastrDbProvider = kadastrDbProvider;
            _c2DWcfManager = c2DWcfManager;
            _loadedAlready = loadedAlready;
        }

        public async Task<int> ParseWells(string folder)
        {
            var count = 0;
            var filename = folder + @"\wells.csv";

            var lines = File.ReadAllLines(filename);
            _logFile.AppendLine($"{lines.Length} lines found in wells.csv");
            foreach (var line in lines)
            {
                if (await ProcessOneLine(line) == null) count++;
            }

            return count;
        }

        private async Task<string> ProcessOneLine(string line)
        {
            var fields = line.Split(';');
            if (fields.Length < 5) return "invalid line";

            if (!int.TryParse(fields[0], out int inKadastrId)) return "invalid line";

            if (_loadedAlready.Wells.FirstOrDefault(w => w.InKadastrId == inKadastrId) != null) return "well exists already";

            var well = new Well()
            {
                InKadastrId = inKadastrId,
                InFibertestId = Guid.NewGuid(),
            };
            _loadedAlready.Wells.Add(well);
            await _kadastrDbProvider.AddWell(well);

            var cmd = CreateNodeCmd(fields, well.InFibertestId);
            return await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        private AddEquipmentAtGpsLocationWithNodeTitle CreateNodeCmd(string[] parts, Guid inFibertestId)
        {
            var cmd = new AddEquipmentAtGpsLocationWithNodeTitle();
            cmd.NodeId = inFibertestId;
            cmd.Title = parts[1];
            cmd.EmptyNodeEquipmentId = Guid.NewGuid();
            cmd.Type = EquipmentType.EmptyNode;

            var ds = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            double.TryParse(parts[2].Replace('.', ds[0]).Replace(',', ds[0]), out double xKadastr);
            cmd.Latitude = Latitude0 + xKadastr * Latitude1M;
            double.TryParse(parts[3].Replace('.', ds[0]).Replace(',', ds[0]), out double yKadastr);
            cmd.Longitude = Longitude0 + yKadastr * Longitude1M;

            return cmd;
        }

        // точка с кооординатами в кадастре 0,0 имеет такие координаты GPS
        private const double Latitude0 = 53.903103;
        private const double Longitude0 = 27.555008;

        // в одном метре столько градусов в районе Минска
        private const double Longitude1M = 15.228e-6;
        private const double Latitude1M = 8.981e-6;
    }
}
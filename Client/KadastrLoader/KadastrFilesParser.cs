using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace KadastrLoader
{
    public class KadastrFilesParser
    {
        private readonly IMyLog _logFile;
        private readonly KadastrDbProvider _kadastrDbProvider;
        private readonly C2DWcfManager _c2DWcfManager;

        public KadastrFilesParser(IMyLog logFile, KadastrDbProvider kadastrDbProvider, C2DWcfManager c2DWcfManager)
        {
            _logFile = logFile;
            _kadastrDbProvider = kadastrDbProvider;
            _c2DWcfManager = c2DWcfManager;
        }

        public async Task<int>  Go(string folder)
        {
            return await ParseWells(folder);
        }

        private async Task<int> ParseWells(string folder)
        {
            var filename = folder + @"\wells.csv";
            var lines = File.ReadAllLines(filename);
            _logFile.AppendLine($"{lines.Length} lines found in wells.csv");
            foreach (var line in lines)
            {
                await ProcessOneLine(line);
            }

            return 1;
        }

        private async Task<int> ProcessOneLine(string line)
        {
            var fields = line.Split(';');
            if (fields.Length < 5) return 0;

            if (!int.TryParse(fields[0], out int inKadastrId)) return 0;

            if (_kadastrDbProvider.GetWellByKadastrId(inKadastrId) != null) return 0;

            var well = new Well()
            {
                InKadastrId = inKadastrId,
                InFibertestId = Guid.NewGuid(),
            };
            await _kadastrDbProvider.AddWell(well);

            var cmd = CreateNodeCmd(fields, well.InFibertestId);
            await _c2DWcfManager.SendCommandAsObj(cmd);
            return 1;
        }

        private AddEquipmentAtGpsLocationWithNodeTitle CreateNodeCmd(string[] parts, Guid inFibertestId)
        {
            var cmd = new AddEquipmentAtGpsLocationWithNodeTitle();
            cmd.NodeId = inFibertestId;
            cmd.Title = parts[1];
            cmd.EmptyNodeEquipmentId = Guid.NewGuid();
            cmd.Type = EquipmentType.EmptyNode;

            var ds = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            double.TryParse(parts[2].Replace('.', ds[0]).Replace(',',ds[0]), out double xKadastr);
            cmd.Latitude = Latitude0 + xKadastr * Latitude1M;
            double.TryParse(parts[3].Replace('.', ds[0]).Replace(',',ds[0]), out double yKadastr);
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
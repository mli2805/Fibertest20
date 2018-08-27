using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace KadastrLoader
{
    public class ChannelParser
    {
        private readonly IMyLog _logFile;
        private readonly C2DWcfManager _c2DWcfManager;
        private readonly LoadedAlready _loadedAlready;

        public ChannelParser(IMyLog logFile,
            C2DWcfManager c2DWcfManager, LoadedAlready loadedAlready)
        {
            _logFile = logFile;
            _c2DWcfManager = c2DWcfManager;
            _loadedAlready = loadedAlready;
        }

        public async Task<int> ParseChannels(string folder)
        {
            var count = 0;
            var filename = folder + @"\channels.csv";
            var lines = File.ReadAllLines(filename);
            _logFile.AppendLine($"{lines.Length} lines found in channels.csv");
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

            var cmd = CreateFiberCmd(fields);
            return cmd == null ? "invalid line" : await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        private AddFiber CreateFiberCmd(string[] parts)
        {
            var cmd = new AddFiber();

            cmd.FiberId = Guid.NewGuid();
            if (!int.TryParse(parts[0], out int inKadastrIdL)) return null;
            var wellL = _loadedAlready.Wells.FirstOrDefault(w => w.InKadastrId == inKadastrIdL);
            if (wellL == null)
                return null;
            cmd.NodeId1 = wellL.InFibertestId;

            if (!int.TryParse(parts[1], out int inKadastrIdR)) return null;
            var wellR = _loadedAlready.Wells.FirstOrDefault(w => w.InKadastrId == inKadastrIdR);
            if (wellR == null)
                return null;
            cmd.NodeId2 = wellR.InFibertestId;

            return cmd;
        }
    }
}
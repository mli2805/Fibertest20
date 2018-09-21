using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;

namespace KadastrLoader
{
    public class ChannelParser
    {
        private readonly C2DWcfManager _c2DWcfManager;
        private readonly LoadedAlready _loadedAlready;

        public ChannelParser(C2DWcfManager c2DWcfManager, LoadedAlready loadedAlready)
        {
            _c2DWcfManager = c2DWcfManager;
            _loadedAlready = loadedAlready;
        }

        public void ParseChannels(string folder, BackgroundWorker worker)
        {
            var count = 0;
            var filename = folder + @"\channels.csv";
            var lines = File.ReadAllLines(filename);
            worker.ReportProgress(0, string.Format(Resources.SID__0__lines_found_in_channels_csv, lines.Length));
            foreach (var line in lines)
            {
                if (ProcessOneLine(line) == null) count++;
            }

            worker.ReportProgress(0, string.Format(Resources.SID__0__channels_applied, count));
        }

        private string ProcessOneLine(string line)
        {
            var fields = line.Split(';');
            if (fields.Length < 4) return "invalid line";

            var cmd = CreateFiberCmd(fields);
            return cmd == null ? "invalid line" : _c2DWcfManager.SendCommandAsObj(cmd).Result;
        }

        private AddFiber CreateFiberCmd(string[] parts)
        {
            var cmd = new AddFiber {FiberId = Guid.NewGuid()};

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
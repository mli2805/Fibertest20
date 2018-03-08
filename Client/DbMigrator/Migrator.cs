using System.IO;
using System.Text;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DbMigrator
{
    public class Migrator
    {
        private readonly IniFile _iniFile;
        private readonly LogFile _logFile;
        private readonly Graph _graph;
        private readonly FileStringParser _fileStringParser;
        private readonly FileStringTraceParser _fileStringTraceParser;



        public Migrator(IniFile iniFile, LogFile logFile, Graph graph, 
            FileStringParser fileStringParser, FileStringTraceParser fileStringTraceParser)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _graph = graph;
            _fileStringParser = fileStringParser;
            _fileStringTraceParser = fileStringTraceParser;

        }

        public void Go()
        {
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            var memory = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            Encoding win1251 = Encoding.GetEncoding("Windows-1251");
            string[] lines = File.ReadAllLines(@"..\db\export.txt", win1251);
            _logFile.AppendLine($"Export.txt contains {lines.Length} lines");

           FirstPass(lines);

            // second pass - all nodes and equipment loaded, now we can process traces
           SecondPass(lines);

            _graph.TraceEventsUnderConstruction.ForEach(e => _graph.Db.Add(e));

            System.Threading.Thread.CurrentThread.CurrentCulture = memory;


            SendCommands();
        }

        private void SendCommands()
        {
            _logFile.EmptyLine();
            _logFile.AppendLine($"{_graph.Db.Count} commands prepared. Sending...");

            var c2DWcfManager = new C2DWcfManager(_iniFile, _logFile);
            DoubleAddress serverAddress = _iniFile.ReadDoubleAddress((int) TcpPorts.ServerListenToClient);
            NetAddress clientAddress = _iniFile.Read(IniSection.ClientLocalAddress, (int) TcpPorts.ClientListenTo);
            c2DWcfManager.SetServerAddresses(serverAddress, @"migrator", clientAddress.Ip4Address);
            for (var i = 0; i < _graph.Db.Count; i++)
            {
                var command = _graph.Db[i];
                var result = c2DWcfManager.SendCommandAsObj(command).Result;
                if (!string.IsNullOrEmpty(result))
                    _logFile.AppendLine(result);


                if (i % 100 == 0)
                    _logFile.AppendLine($"{i} commands sent");
            }
        }

        private void FirstPass(string[] lines)
        {
            _logFile.EmptyLine();
            _logFile.AppendLine("First pass");

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var logLineParts = line.Split('|');
                if (logLineParts.Length == 1)
                    continue;
                var parts = logLineParts[1].Trim().Split(';');
                switch (parts[0])
                {
                    case "NODES::":
                        _fileStringParser.ParseNode(parts);
                        break;
                    case "RTU::":
                        _fileStringParser.ParseRtu(parts);
                        break;
                    case "FIBERS::":
                        _fileStringParser.ParseFiber(parts);
                        break;
                    case "EQUIPMENTS::":
                        _fileStringParser.ParseEquipments(parts);
                        break;
                    case "CHARON::":
                        _fileStringParser.ParseCharon(parts);
                        break;
                }

                if (i % 2000 == 0)
                    _logFile.AppendLine($"{i} lines processed");
            }
        }

        private void SecondPass(string[] lines)
        {
            _logFile.EmptyLine();
            _logFile.AppendLine("Second pass");

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var logLineParts = line.Split('|');
                if (logLineParts.Length == 1)
                    continue;
                var parts = logLineParts[1].Trim().Split(';');
                switch (parts[0])
                {
                    case "TRACES::":
                        _fileStringTraceParser.ParseTrace(parts);
                        break;
                    case "TRACES::N":
                        _fileStringTraceParser.ParseTraceNodes(parts);
                        break;
                    case "TRACES::E":
                        _fileStringTraceParser.ParseTraceEquipments(parts);
                        break;
                }

                if (i % 2000 == 0)
                    _logFile.AppendLine($"{i} lines processed");
            }
        }
      
    }
}

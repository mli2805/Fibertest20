using System;
using System.IO;
using System.Text;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DbMigrator
{
    public class GraphFetcher
    {
        private readonly LogFile _logFile;
        private readonly GraphModel _graphModel;
        private readonly FileStringParser _fileStringParser;
        private readonly FileStringTraceParser _fileStringTraceParser;

        public GraphFetcher(LogFile logFile, GraphModel graphModel)
        {
            _logFile = logFile;
            _graphModel = graphModel;
            _fileStringParser = new FileStringParser(graphModel);
            _fileStringTraceParser = new FileStringTraceParser(graphModel);
        }

        public void Fetch()
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

            foreach (var o in _graphModel.TraceEventsUnderConstruction)
            {
                switch (o)
                {
                    case AddTrace cmd:
                        _graphModel.Commands.Add(cmd);
                        _graphModel.AddTraceCommands.Add(cmd); // copy for easy access
                        break;
                    case AttachTrace cmd: _graphModel.AttachTraceCommands.Add(cmd); break;
                }
            }

            System.Threading.Thread.CurrentThread.CurrentCulture = memory;

            _logFile.AppendLine($"{_graphModel.Commands.Count} commands prepared");
        }

        private void FirstPass(string[] lines)
        {
            Console.WriteLine();
            Console.WriteLine($"{DateTime.Now}   First pass");

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
                    Console.WriteLine($"{DateTime.Now}   {i} lines processed");
            }
        }

        private void SecondPass(string[] lines)
        {
            Console.WriteLine();
            Console.WriteLine($"{DateTime.Now}   Second pass");

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
                    Console.WriteLine($"{DateTime.Now}   {i} lines processed");
            }
        }
    }
}
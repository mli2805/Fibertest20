using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace DbMigrationWpf
{
    public class GraphFetcher
    {
        private readonly IMyLog _logFile;
        private readonly GraphModel _graphModel;
        private readonly ObservableCollection<string> _progressLines;
        private readonly FileStringParser _fileStringParser;
        private readonly FileStringTraceParser _fileStringTraceParser;

        public GraphFetcher(IMyLog logFile, GraphModel graphModel, ObservableCollection<string> progressLines)
        {
            _logFile = logFile;
            _graphModel = graphModel;
            _progressLines = progressLines;
            _fileStringParser = new FileStringParser(graphModel);
            _fileStringTraceParser = new FileStringTraceParser(graphModel);
        }

        public void Fetch(string exportFileName)
        {
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            var memory = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            Encoding win1251 = Encoding.GetEncoding("Windows-1251");
            string[] lines = File.ReadAllLines(exportFileName, win1251);
            _logFile.AppendLine($"Export.txt contains {lines.Length} lines");
           _progressLines.Add($"Export.txt contains {lines.Length} lines");

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
            _progressLines.Add($"{_graphModel.Commands.Count} commands prepared");
        }

        private void FirstPass(string[] lines)
        {
            _logFile.EmptyLine();
            _logFile.AppendLine(@"First pass");
            _progressLines.Add(@"First pass");

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var parts = line.Trim().Split(';');
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
                {
                    _logFile.AppendLine($"{DateTime.Now}   {i} lines processed");
                    _progressLines.Add($"{DateTime.Now}   {i} lines processed");
                }
            }
        }

        private void SecondPass(string[] lines)
        {
            _logFile.EmptyLine();
            _logFile.AppendLine("Second pass");
            _progressLines.Add(@"Second pass");

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var parts = line.Trim().Split(';');
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
                {
                    _logFile.AppendLine($"{i} lines processed");
                    _progressLines.Add($"{i} lines processed");
                }
            }
        }
    }
}
using System.IO;
using System.Text;

namespace Iit.Fibertest.DbMigrator
{
    public class Migrator
    {
        private readonly Graph _graph;
        private readonly FileStringParser _fileStringParser;
        private readonly FileStringTraceParser _fileStringTraceParser;

        public Migrator(Graph graph, FileStringParser fileStringParser, FileStringTraceParser fileStringTraceParser)
        {
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

           FirstPass(lines);

            // second pass - all nodes and equipment loaded, now we can process traces
           SecondPass(lines);

            _graph.TraceEventsUnderConstruction.ForEach(e => _graph.Db.Add(e));

            System.Threading.Thread.CurrentThread.CurrentCulture = memory;
        }

        private void FirstPass(string[] lines)
        {
            foreach (var line in lines)
            {
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
            }
        }

        private void SecondPass(string[] lines)
        {
            foreach (var line in lines)
            {
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
            }
        }
      
    }
}

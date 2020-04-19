using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DbMigrationWpf.BaseRefMigration;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace DbMigrationWpf
{
    public class MigrationManager
    {
        private readonly IMyLog _logFile;
        private readonly GraphModel _graphModel;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly IWcfServiceCommonC2D _c2RWcfManager;
        private readonly ObservableCollection<string> _lines;

        public MigrationManager(IMyLog logFile, GraphModel graphModel, 
            IWcfServiceDesktopC2D c2DWcfManager, IWcfServiceCommonC2D c2RWcfManager, ObservableCollection<string> lines)
        {
            _logFile = logFile;
            _graphModel = graphModel;
            _c2DWcfManager = c2DWcfManager;
            _c2RWcfManager = c2RWcfManager;
            _lines = lines;
        }

        public async Task<int> Migrate(string exportFileName, string ft15Address, 
            int oldMySqlPort, string ft20Address, int newMySqlPort, bool hasKadastr)
        {
            new GraphFetcher(_logFile, _graphModel, _lines).Fetch(exportFileName);
            _logFile.AppendLine("Graph is fetched");
            _lines.Add("Graph is fetched");

            await SendCommandsExcludingAttachTrace();
            _logFile.AppendLine("Graph is sent");
            _lines.Add("Graph is sent");

            await TransferBaseRefs(ft15Address, oldMySqlPort);
            _logFile.AppendLine("Base refs are sent");
            _lines.Add("Base refs are sent");

            await SendCommandsAttachTrace();

            if (hasKadastr)
                await MigrateKadastr(ft15Address, oldMySqlPort, ft20Address, newMySqlPort);

            _logFile.AppendLine("Migration is terminated");
            _lines.Add("Migration is terminated");
            return 0;
        }

        private async Task MigrateKadastr(string ft15Address, int oldMySqlPort, string ft20Address, int newMySqlPort)
        {
            var km = new Kadastr15Fetcher(ft15Address, oldMySqlPort, _graphModel, _lines);
            var model = km.Fetch();
            if (model == null) return;
            var kp = new Kadastr20Provider(ft20Address, newMySqlPort, _lines);
            kp.Init();
            await kp.Save(model);
        }

        private async Task TransferBaseRefs(string ft15Address, int oldMySqlPort)
        {
            var i = 0;
            var totalTraces = _graphModel.TracesDictionary.Count;
            foreach (var pair in _graphModel.TracesDictionary)
            {
                var addTraceCommand = _graphModel.AddTraceCommands.First(c => c.TraceId == pair.Value);
                var rtuGuid = addTraceCommand.RtuId;
                var assignBaseRefCommand = new TraceBaseFetcher(ft15Address, oldMySqlPort).GetAssignBaseRefsDto(pair.Key, pair.Value, rtuGuid);
                var result = await _c2RWcfManager.AssignBaseRefAsyncFromMigrator(assignBaseRefCommand);
                if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                {
                    _lines.Add($"Error!!! {addTraceCommand.Title}");
                    _lines.Add(result.ErrorMessage);
                }

                i++;
                _logFile.AppendLine($"{i}/{totalTraces} assign base ref commands sent");
                _lines.Add($"{DateTime.Now}  {i}/{totalTraces} assign base ref commands sent");
            }
        }

        private async Task SendCommandsExcludingAttachTrace()
        {
            _logFile.EmptyLine();
            var totalCmds = _graphModel.Commands.Count;
            _logFile.AppendLine($"{totalCmds} commands prepared. Sending...");
            _lines.Add($"{DateTime.Now}   {totalCmds} commands prepared. Sending...");

            var list = new List<object>();
            var portion = 50; // no more than 100 please, max size of wcf operation could be exceeded, anyway check the log if are some errors
            for (var i = 0; i < totalCmds; i++)
            {
                list.Add(_graphModel.Commands[i]);
                if (list.Count == portion)
                {
                    var result = await _c2DWcfManager.SendCommandsAsObjs(list);
                    if (result != portion)
                    {
                        _logFile.AppendLine($"i = {i};   commands accepted = {result}");
                        _lines.Add($"i = {i};   commands accepted = {result}");
                    }
                    list = new List<object>();
                    _logFile.AppendLine($"{i + 1}/{totalCmds} commands sent");
                    _lines.Add($"{DateTime.Now}   {i + 1}/{totalCmds} commands sent");
                }
            }
            if (list.Count > 0)
                await _c2DWcfManager.SendCommandsAsObjs(list);
            _logFile.AppendLine($"{totalCmds} commands sent");
            _lines.Add($"{DateTime.Now}   {totalCmds} commands sent");
        }

        private async Task SendCommandsAttachTrace()
        {
            _logFile.EmptyLine();
            _logFile.AppendLine($"{_graphModel.AttachTraceCommands.Count} attach trace commands prepared. Sending...");
            _lines.Add($"{DateTime.Now}   {_graphModel.AttachTraceCommands.Count} attach trace commands prepared. Sending...");

            var list = new List<object>();
            for (var i = 0; i < _graphModel.AttachTraceCommands.Count; i++)
            {
                list.Add(_graphModel.AttachTraceCommands[i]);
                if (list.Count == 100) // no more please, max size of wcf operation could be exceeded, anyway check the log if are some errors
                {
                    await _c2DWcfManager.SendCommandsAsObjs(list);
                    list = new List<object>();
                    _logFile.AppendLine($"{i + 1} attach trace commands sent");
                    _lines.Add($"{DateTime.Now}   {i + 1} attach trace commands sent");
                }
            }

            if (list.Count > 0)
                await _c2DWcfManager.SendCommandsAsObjs(list);
            _logFile.AppendLine($"{_graphModel.AttachTraceCommands.Count} commands sent");
            _lines.Add($"{DateTime.Now}   {_graphModel.AttachTraceCommands.Count} commands sent");
        }
    }
}

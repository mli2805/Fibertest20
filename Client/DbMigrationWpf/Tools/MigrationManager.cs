﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DbMigrationWpf.Measurements;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace DbMigrationWpf
{
    public class MigrationManager
    {
        private readonly IMyLog _logFile;
        private readonly GraphModel _graphModel;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly ObservableCollection<string> _lines;

        public MigrationManager(IMyLog logFile, GraphModel graphModel, IWcfServiceForClient c2DWcfManager, ObservableCollection<string> lines)
        {
            _logFile = logFile;
            _graphModel = graphModel;
            _c2DWcfManager = c2DWcfManager;
            _lines = lines;
        }

        public async Task Migrate(string exportFileName, bool shouldTransferMeasurements, string ft15Address)
        {
            new GraphFetcher(_logFile, _graphModel, _lines).Fetch(exportFileName);
            _logFile.AppendLine("Graph is fetched");
            _lines.Add("Graph is fetched");

            await SendCommandsExcludingAttachTrace();
            _logFile.AppendLine("Graph is sent");
            _lines.Add("Graph is sent");

            await TransferBaseRefs(ft15Address);
            _logFile.AppendLine("Base refs are sent");
            _lines.Add("Base refs are sent");

            if (shouldTransferMeasurements)
                await new MeasurementsFetcher(ft15Address, _logFile).TransferMeasurements(_graphModel, _c2DWcfManager);

            await SendCommandsAttachTrace();
            _logFile.AppendLine("Migration is terminated");
            _lines.Add("Migration is terminated");
        }

        private async Task TransferBaseRefs(string ft15Address)
        {
            var i = 0;
            var totalTraces = _graphModel.TracesDictionary.Count;
            foreach (var pair in _graphModel.TracesDictionary)
            {
                var addTraceCommand = _graphModel.AddTraceCommands.First(c => c.TraceId == pair.Value);
                var rtuGuid = addTraceCommand.RtuId;
                var assignBaseRefCommand = new TraceBaseFetcher(ft15Address).GetAssignBaseRefsDto(pair.Key, pair.Value, rtuGuid);
                var result = await _c2DWcfManager.AssignBaseRefAsyncFromMigrator(assignBaseRefCommand);
                if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                {
                    _lines.Add($"Error!!! {addTraceCommand.Title}");
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
            var portion = 100; // no more than 100 please, max size of wcf operation could be exceeded, anyway check the log if are some errors
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

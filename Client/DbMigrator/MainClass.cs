using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DbMigrator
{
    public class MainClass
    {
        private readonly LogFile _logFile;
        private readonly GraphModel _graphModel;
        private readonly GraphFetcher _graphFetcher;
        private readonly SorFetcher _sorFetcher;

        private readonly C2DWcfManager _c2DWcfManager;
        public MainClass(IniFile iniFile, LogFile logFile, GraphModel graphModel,
            GraphFetcher graphFetcher, SorFetcher sorFetcher)
        {
            _logFile = logFile;
            _graphModel = graphModel;
            _graphFetcher = graphFetcher;
            _sorFetcher = sorFetcher;

            _c2DWcfManager = new C2DWcfManager(iniFile, _logFile);
            DoubleAddress serverAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToClient);
            NetAddress clientAddress = iniFile.Read(IniSection.ClientLocalAddress, (int)TcpPorts.ClientListenTo);
            _c2DWcfManager.SetServerAddresses(serverAddress, @"migrator", clientAddress.Ip4Address);
        }

        public void Go()
        {
            _graphFetcher.Fetch();
            SendCommandsExcludingAttachTrace();

            var i = 0;
            foreach (var pair in _graphModel.TracesDictionary)
            {
                var rtuGuid = _graphModel.AddTraceCommands.First(c => c.Id == pair.Value).RtuId;
                var assignBaseRefCommand = _sorFetcher.GetAssignBaseRefsDto(pair.Key, pair.Value, rtuGuid);
                _c2DWcfManager.AssignBaseRefAsync(assignBaseRefCommand).Wait();

                Console.WriteLine($"{DateTime.Now}  {++i} assign base ref commands sent");
            }

            SendCommandsAttachTrace();
            _logFile.AppendLine($"Commands are sent");
        }

        private void SendCommandsExcludingAttachTrace()
        {
            Console.WriteLine();
            Console.WriteLine($"{DateTime.Now}   {_graphModel.Commands.Count} commands prepared. Sending...");

            var list = new List<object>();
            for (var i = 0; i < _graphModel.Commands.Count; i++)
            {
                list.Add(_graphModel.Commands[i]);
                if (list.Count == 100) // no more please, max size of wcf operation could be exceeded, anyway check the log if are some errors
                {
                    _c2DWcfManager.SendCommandsAsObjs(list).Wait();
                    list = new List<object>();
                    Console.WriteLine($"{DateTime.Now}   {i + 1} commands sent");
                }
            }
            if (list.Count > 0)
                _c2DWcfManager.SendCommandsAsObjs(list).Wait();
            Console.WriteLine($"{DateTime.Now}   {_graphModel.Commands.Count} commands sent");
        }

        private void SendCommandsAttachTrace()
        {
            Console.WriteLine();
            Console.WriteLine($"{DateTime.Now}   {_graphModel.AttachTraceCommands.Count} attach trace commands prepared. Sending...");

            var list = new List<object>();
            for (var i = 0; i < _graphModel.AttachTraceCommands.Count; i++)
            {
                list.Add(_graphModel.AttachTraceCommands[i]);
                if (list.Count == 100) // no more please, max size of wcf operation could be exceeded, anyway check the log if are some errors
                {
                    _c2DWcfManager.SendCommandsAsObjs(list).Wait();
                    list = new List<object>();
                    Console.WriteLine($"{DateTime.Now}   {i + 1} attach trace commands sent");
                }
            }
            if (list.Count > 0)
                _c2DWcfManager.SendCommandsAsObjs(list).Wait();
            Console.WriteLine($"{DateTime.Now}   {_graphModel.AttachTraceCommands.Count} commands sent");

        }

    }
}

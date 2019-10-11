using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Newtonsoft.Json;

namespace WebProxeInterfaceProbe
{
    class Program
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        static async Task<int> Main()
        {
            var iniFile = new IniFile();
            iniFile.AssignFile("webproxy.ini");

            var logFile = new LogFile(iniFile);
            logFile.AssignFile("webproxy.log");

            var webProxy2DWcfManager = new WebProxy2DWcfManager(iniFile, logFile);
            var doubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToWebProxy);
            webProxy2DWcfManager.SetServerAddresses(doubleAddress, "webProxy", "localhost");

            var eventList = await webProxy2DWcfManager.GetOpticalEventList();
            logFile.AppendLine(eventList == null
                ? "Failed to get list"
                : $"list contains {eventList.Count} items");


            var rtuList = await webProxy2DWcfManager.GetRtuList();
            logFile.AppendLine(rtuList == null
                ? "Failed to get list"
                : $"list contains {rtuList.Count} items");


            eventList = await webProxy2DWcfManager.GetOpticalEventList();
            logFile.AppendLine(eventList == null
                ? "Failed to get list"
                : $"list contains {eventList.Count} items");

            var tree = await webProxy2DWcfManager.GetTreeInJson();
            logFile.AppendLine(tree == null
                ? "Failed to get tree"
                : $"tree contains {tree.Length} symbols");
            var treeL = (List<RtuDto>)JsonConvert.DeserializeObject(tree, JsonSerializerSettings);
            logFile.AppendLine(treeL == null
                ? "Failed to get list"
                : $"list contains {treeL.Count} items");


            Console.ReadKey();
            return 0;
        }


    }
}

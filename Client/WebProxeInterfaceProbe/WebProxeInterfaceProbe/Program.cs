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
            
            var tree = await webProxy2DWcfManager.GetTreeInJson("root");
            Console.WriteLine(tree == null
                ? "Failed to get tree"
                : $"tree contains {tree.Length} symbols");
            var treeL = (List<RtuDto>)JsonConvert.DeserializeObject(tree, JsonSerializerSettings);
            Console.WriteLine(treeL == null
                ? "Failed to get list"
                : $"list contains {treeL.Count} items");

            var guid = Guid.Parse("0f3becab-c235-47f5-b041-58057d56979a");
            var traceInfo = await webProxy2DWcfManager.GetTraceInformation("root", guid);
            Console.WriteLine(traceInfo == null
                ? "Failed to get info"
                : $"trace has {traceInfo.Equipment.Count} eq");

            Console.ReadKey();
            return 0;
        }


    }
}

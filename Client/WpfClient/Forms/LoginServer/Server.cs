using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Newtonsoft.Json;

namespace Iit.Fibertest.Client
{
    public class Server
    {
        public string Title { get; set; }
        public DoubleAddress ServerAddress { get; set; }
        public string ClientIpAddress { get; set; }
        public bool IsLastSelected { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }

    public static class ServerList
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        private static readonly string ServerListFile = Utils.FileNameForSure(@"..\ini\", @"servers.list", false);

        public static List<Server> Load(IMyLog logFile)
        {
            try
            {
                var contents = File.ReadAllLines(ServerListFile);
                var servers = contents.Select(s => (Server)JsonConvert.DeserializeObject(s, JsonSerializerSettings)).ToList();
                logFile.AppendLine($@"{servers.Count} servers in my list.");
                return servers;
            }
            catch (Exception e)
            {
                logFile.AppendLine($@"Servers loading: {e.Message}");
                return new List<Server>();
            }
        }

        public static void Save(List<Server> servers, IMyLog logFile)
        {
            try
            {
                var list = servers.Select(p => JsonConvert.SerializeObject(p, JsonSerializerSettings));
                File.WriteAllLines(ServerListFile, list);
            }
            catch (Exception e)
            {
                logFile.AppendLine($@"Servers saving: {e.Message}");
            }
        }
    }
}
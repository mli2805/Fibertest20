using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Iit.Fibertest.UtilsLib;
using Newtonsoft.Json;

namespace Iit.Fibertest.SuperClient
{
    public class FtServer
    {
        public int Id { get; set; }
        public string ServerTitle { get; set; }
        public string ServerIp { get; set; }
        public int ServerTcpPort { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class FtServerList
    {
        private readonly IMyLog _logFile;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        private string _filename;

        public FtServerList(IMyLog logFile)
        {
            _logFile = logFile;
            var path = AppDomain.CurrentDomain.BaseDirectory;
            _filename = FileOperations.GetParentFolder(path) + @"\ini\servers.list";
        }

        public ObservableCollection<FtServer> Read()
        {
            var servers = new ObservableCollection<FtServer>();

            try
            {
                if (!File.Exists(_filename))
                {
                    File.Create(_filename);
                }
                else
                {
                    var contents = File.ReadAllLines(_filename);
                    foreach (var server in contents.Select(s => (FtServer)JsonConvert.DeserializeObject(s, JsonSerializerSettings)))
                    {
                        servers.Add(server);
                    }
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine($@"Servers loading: {e.Message}");
            }
            _logFile.AppendLine($@"{servers.Count} server(s) in my list.");
            return servers;
        }

        private void Write(List<FtServer> servers)
        {
            try
            {
                var list = servers.Select(p => JsonConvert.SerializeObject(p, JsonSerializerSettings));
                File.WriteAllLines(_filename, list);
            }
            catch (Exception e)
            {
                _logFile.AppendLine($@"Servers saving: {e.Message}");
            }
        }
    }
}
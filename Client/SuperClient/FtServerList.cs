using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Iit.Fibertest.UtilsLib;
using Newtonsoft.Json;

namespace Iit.Fibertest.SuperClient
{
    public class FtServerList
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        private readonly IMyLog _logFile;

        private string _filename;

        public ObservableCollection<FtServer> Servers { get; set; }

        public FtServerList(IMyLog logFile)
        {
            _logFile = logFile;
            var path = AppDomain.CurrentDomain.BaseDirectory;
            _filename = FileOperations.GetParentFolder(path) + @"\ini\servers.list";
        }

        public void Read()
        {
            Servers = new ObservableCollection<FtServer>();

            try
            {
                if (!File.Exists(_filename))
                {
                    File.Create(_filename);
                }
                else
                {
                    var contents = File.ReadAllLines(_filename);
                    foreach (var ftServerEntity in contents.Select(s => (FtServerEntity)JsonConvert.DeserializeObject(s, JsonSerializerSettings)))
                    {
                        Servers.Add(new FtServer(){Entity = ftServerEntity});
                    }
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine($@"Servers loading: {e.Message}");
            }
            _logFile.AppendLine($@"{Servers.Count} server(s) in my list.");
        }

        public void Add(FtServer ftServer)
        {
            Servers.Add(ftServer);
            Write();
        }

        public void Remove(FtServer ftServer)
        {
            Servers.Remove(ftServer);
            Write();
        }

        private void Write()
        {
            try
            {
                var json = Servers.Select(s=>s.Entity).Select(p => JsonConvert.SerializeObject(p, JsonSerializerSettings));
                File.WriteAllLines(_filename, json);
            }
            catch (Exception e)
            {
                _logFile.AppendLine($@"Servers saving: {e.Message}");
            }
        }
    }
}
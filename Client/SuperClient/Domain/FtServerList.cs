using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using Iit.Fibertest.UtilsLib;
using Newtonsoft.Json;

namespace Iit.Fibertest.SuperClient
{
    public class FtServerList
    {
        private byte[] _key = { 0xc5, 0x51, 0xf6, 0x4e, 0x97, 0xdc, 0xa0, 0x54, 0x89, 0x1d, 0xe6, 0x62, 0x3f, 0x27, 0x00, 0xca };
        private byte[] _initVector = { 0xf3, 0x5e, 0x7a, 0x81, 0xae, 0x8c, 0xb4, 0x92, 0xd0, 0xf2, 0xe7, 0xc1, 0x8d, 0x54, 0x00, 0xd8 };

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        private readonly IMyLog _logFile;

        private readonly string _filename;

        public ObservableCollection<FtServer> Servers { get; set; }

        public FtServerList(IMyLog logFile)
        {
            _logFile = logFile;
            var path = AppDomain.CurrentDomain.BaseDirectory;
            _filename = FileOperations.GetParentFolder(path) + @"\ini\servers.list";
        }

        private void EncryptAndSerialize()
        {
            try
            {
                var json = Servers.Select(s => s.Entity).Select(p => JsonConvert.SerializeObject(p, JsonSerializerSettings)).ToList();
                using (Stream fStream = new FileStream(_filename, FileMode.Create, FileAccess.Write))
                {
                    var rmCrypto = new RijndaelManaged();

                    using (var cryptoStream = new CryptoStream(fStream, rmCrypto.CreateEncryptor(_key, _initVector), CryptoStreamMode.Write))
                    {
                        var binaryFormatter = new BinaryFormatter();
                        binaryFormatter.Serialize(cryptoStream, json);
                    }
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine(@"EncryptAndSerialize: " + e.Message);
            }
        }

        public void DeserializeAndDecrypt()
        {
            Servers = new ObservableCollection<FtServer>();
            try
            {
                List<string> content;
                using (Stream fStream = new FileStream(_filename, FileMode.Open, FileAccess.Read))
                {
                    var rmCrypto = new RijndaelManaged();

                    using (var cryptoStream = new CryptoStream(fStream, rmCrypto.CreateDecryptor(_key, _initVector), CryptoStreamMode.Read))
                    {
                        var binaryFormatter = new BinaryFormatter();
                        content = (List<string>)binaryFormatter.Deserialize(cryptoStream);
                    }
                }

                foreach (var line in content)
                {
                    var ftServerEntity = (FtServerEntity)JsonConvert.DeserializeObject(line, JsonSerializerSettings);
                    Servers.Add(new FtServer() { Entity = ftServerEntity });
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine(@"DeserializeAndDecrypt: " + e.Message);
            }
        }

        //        public void Read()
        //        {
        //            Servers = new ObservableCollection<FtServer>();
        //
        //            try
        //            {
        //                if (!File.Exists(_filename)) File.Create(_filename);
        //                else
        //                {
        //                    var contents = File.ReadAllLines(_filename);
        //                    foreach (var ftServerEntity in contents.Select(s => (FtServerEntity)JsonConvert.DeserializeObject(s, JsonSerializerSettings)))
        //                    {
        //                        Servers.Add(new FtServer(){Entity = ftServerEntity});
        //                    }
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                _logFile.AppendLine($@"Servers loading: {e.Message}");
        //            }
        //            _logFile.AppendLine($@"{Servers.Count} server(s) in my list.");
        //        }

        public void Add(FtServer ftServer)
        {
            Servers.Add(ftServer);
            EncryptAndSerialize();
        }

        public void Remove(FtServer ftServer)
        {
            Servers.Remove(ftServer);
            EncryptAndSerialize();
        }

        //        private void Write()
        //        {
        //            try
        //            {
        //                var json = Servers.Select(s=>s.Entity).Select(p => JsonConvert.SerializeObject(p, JsonSerializerSettings));
        //                File.WriteAllLines(_filename, json);
        //            }
        //            catch (Exception e)
        //            {
        //                _logFile.AppendLine($@"Servers saving: {e.Message}");
        //            }
        //        }
    }
}
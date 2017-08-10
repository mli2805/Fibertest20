using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.ServiceModel;
using System.Threading;
using Iit.Fibertest.Utils35;
using WcfServiceForClientLibrary;
using WcfServiceForRtuLibrary;

namespace DataCenterCore
{
    public partial class DcManager
    {
        private readonly Logger35 _dcLog;
        private readonly IniFile _coreIni;

        private readonly object _rtuStationsLockObj = new object();
        private List<RtuStation> _rtuStations;

        private readonly object _clientStationsLockObj = new object();
        private List<ClientStation> _clientStations;


        public DcManager()
        {
            _coreIni = new IniFile();
            _coreIni.AssignFile("DcCore.ini");
            var cultureString = _coreIni.Read(IniSection.General, IniKey.Culture, "ru-RU");

            _dcLog = new Logger35();
            _dcLog.AssignFile("DcCore.log", cultureString);
            _dcLog.EmptyLine();
            _dcLog.EmptyLine('-');

            Thread.Sleep(10000);

            lock (_rtuStationsLockObj)
            {
                _rtuStations = InitializeRtuStationListFromDb();
            }

            lock (_clientStationsLockObj)
            {
                _clientStations = new List<ClientStation>();
            }

            StartWcfListenerToClient();
            StartWcfListenerToRtu();
        }

        internal static ServiceHost ServiceForRtuHost;
        internal static ServiceHost ServiceForClientHost;

        private void StartWcfListenerToRtu()
        {
            ServiceForRtuHost?.Close();

            WcfServiceForRtu.ServiceLog = _dcLog;
            WcfServiceForRtu.MessageReceived += WcfServiceForRtu_MessageReceived;
            ServiceForRtuHost = new ServiceHost(typeof(WcfServiceForRtu));
            try
            {
                ServiceForRtuHost.Open();
                _dcLog.AppendLine("RTUs listener started successfully");
            }
            catch (Exception e)
            {
                _dcLog.AppendLine(e.Message);
                throw;
            }
        }


        private void StartWcfListenerToClient()
        {
            ServiceForClientHost?.Close();

            WcfServiceForClient.ServiceLog = _dcLog;
            WcfServiceForClient.MessageReceived += WcfServiceForClient_MessageReceived;
            ServiceForClientHost = new ServiceHost(typeof(WcfServiceForClient));
            try
            {
                ServiceForClientHost.Open();
                _dcLog.AppendLine("Clients listener started successfully");
            }
            catch (Exception e)
            {
                _dcLog.AppendLine(e.Message);
                throw;
            }
        }

        private List<RtuStation> InitializeRtuStationListFromDb()
        {
            return ReadDbTempTxt();
        }

        private List<RtuStation> ReadDbTempTxt()
        {
            var list = new List<RtuStation>();

            var app = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(app);
            if (path == null)
                return null;
            var filename = Path.Combine(path, @"..\Ini\DbTemp.txt");
            if (File.Exists(filename))
            {
                var content = File.ReadAllLines(filename);
                foreach (var line in content)
                {
                    var parts = line.Split(' ');
                    list.Add(new RtuStation()
                    {
                        Id = Guid.Parse(parts[0]),
                        Ip = parts[1],
                        LastConnection = DateTime.Now,
                    });
                }
            }
            _dcLog.AppendLine($"{list.Count} RTU found");
            return list;
        }

    }
}

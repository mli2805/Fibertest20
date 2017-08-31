using System;
using System.Collections.Concurrent;
using System.Threading;
using Dto;
using Iit.Fibertest.UtilsLib;
using WcfConnections;

namespace RtuManagement
{
    public class Dove
    {
        private readonly Guid _rtuId;
        private DoubleAddressWithConnectionStats _serverAddresses;
        private readonly IniFile _serviceIni;
        private readonly LogFile _serviceLog;
        public ConcurrentQueue<MoniResultOnDisk> QueueOfMoniResultsOnDisk { get; set; }

//        private int _forLogCounter;

        public Dove(Guid rtuId, DoubleAddressWithConnectionStats serverAddresses, IniFile serviceIni, LogFile serviceLog)
        {
            _rtuId = rtuId;
            _serverAddresses = serverAddresses;
            _serviceIni = serviceIni;
            _serviceLog = serviceLog;
        }

        public void Start()
        {
            var checkChannelsTimeout =
                TimeSpan.FromSeconds(_serviceIni.Read(IniSection.General, IniKey.CheckChannelsTimeout, 1));
            while (true)
            {
                _serverAddresses = new R2DWcfManager(_serverAddresses, _serviceIni, _serviceLog).SendImAliveByBothChannels(_rtuId);

                if (_serverAddresses.IsLastConnectionOnMainSuccessfull == true || _serverAddresses.IsLastConnectionOnReserveSuccessfull == true)
                    FullSendMoniResult();

                Thread.Sleep(checkChannelsTimeout);
            }
        }

        private void FullSendMoniResult()
        {
            try
            {
                MoniResultOnDisk moniResultOnDisk;
                while (QueueOfMoniResultsOnDisk.TryPeek(out moniResultOnDisk))
                {
                    if (SendMoniResult(moniResultOnDisk))
                    {
                        QueueOfMoniResultsOnDisk.TryDequeue(out moniResultOnDisk);
                        moniResultOnDisk.Delete();
                    }
                }

            }
            catch (Exception e)
            {
                _serviceLog.AppendLine(e.Message);
            }
        }

        private bool SendMoniResult(MoniResultOnDisk moniResultOnDisk)
        {
            try
            {
                moniResultOnDisk.Load();
                return new R2DWcfManager(_serverAddresses, _serviceIni, _serviceLog).SendMonitoringResult(moniResultOnDisk.Dto);
            }
            catch (Exception e)
            {
                _serviceLog.AppendLine(e.Message);
                return false;
            }
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Threading;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.RtuManagement
{
    public class Dove
    {
        private readonly Guid _id;
        private readonly DoubleAddressWithConnectionStats _serverAddresses;
        private readonly IniFile _serviceIni;
        private readonly IMyLog _serviceLog;

        private DateTime _lastLogRecord;

        private readonly object _isCancelledLocker = new object();
        private bool _isCancelled;
        public bool IsCancelled
        {
            get
            {
                lock (_isCancelledLocker)
                {
                    return _isCancelled;
                }
            }
            set
            {
                lock (_isCancelledLocker)
                {
                    _isCancelled = value;
                }
            }
        }

        public ConcurrentQueue<MoniResultOnDisk> QueueOfMoniResultsOnDisk { get; set; }

        public Dove(DoubleAddressWithConnectionStats serverAddresses, IniFile serviceIni, IMyLog serviceLog)
        {
            _id = Guid.NewGuid();
            _serverAddresses = serverAddresses;
            _serviceIni = serviceIni;
            _serviceLog = serviceLog;
        }

//        public void Start()
//        {
//            var checkNewMoniresultsTimeout = TimeSpan.FromSeconds(1);
//            _lastLogRecord = DateTime.Now;
//            _serviceLog.AppendLine($"Dove {_id.First6()} is started");
//            while (!IsCancelled)
//            {
//                SendAllMoniResultsInQueue();
//                Thread.Sleep(checkNewMoniresultsTimeout);
//            }
//            _serviceLog.AppendLine($"Dove {_id.First6()} is finished");
//        }

        private void SendAllMoniResultsInQueue()
        {
            try
            {
                MoniResultOnDisk moniResultOnDisk;

                var betweenLogRecordsTimeout = TimeSpan.FromSeconds(120);
                if (QueueOfMoniResultsOnDisk.Count > 0 || DateTime.Now - _lastLogRecord > betweenLogRecordsTimeout)
                {
                    _lastLogRecord = DateTime.Now;
                    _serviceLog.AppendLine($"There are {QueueOfMoniResultsOnDisk.Count} moniresults in the queue");
                }

                var isTheSameMoniresultPeeked = false;
                while (QueueOfMoniResultsOnDisk.TryPeek(out moniResultOnDisk))
                {
                    if (!isTheSameMoniresultPeeked)
                        _serviceLog.AppendLine("Moniresult peeked from the queue");
                    if (SendMoniResult(moniResultOnDisk))
                    {
                        QueueOfMoniResultsOnDisk.TryDequeue(out moniResultOnDisk);
                        try
                        {
                            moniResultOnDisk.Delete();
                        }
                        catch (Exception e)
                        {
                            _serviceLog.AppendLine($"Error while moniresult deleting from disk {e.Message}");
                        }
                    }
                    else
                    {
                        if (!isTheSameMoniresultPeeked)
                            _serviceLog.AppendLine("Can't send moniresult");
                        isTheSameMoniresultPeeked = true;
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
                var a = _serviceIni.ReadDoubleAddress((int)TcpPorts.ServerListenToRtu);
                _serverAddresses.DoubleAddress = (DoubleAddress)a.Clone();
                if (moniResultOnDisk.Load())
                    return new R2DWcfManager(_serverAddresses, _serviceIni, _serviceLog).SendMonitoringResult(moniResultOnDisk.Dto);

                _serviceLog.AppendLine("something wrong with this moniresult, it will be deleted from queue");
                return true;
            }
            catch (Exception e)
            {
                _serviceLog.AppendLine(e.Message);
                return false;
            }
        }
    }
}

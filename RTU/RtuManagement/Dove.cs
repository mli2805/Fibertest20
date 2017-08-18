﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using Dto;
using Iit.Fibertest.UtilsLib;
using WcfConnections;

namespace RtuManagement
{
    public class Dove
    {
        private readonly DoubleAddressWithLastConnectionCheck _serverAddresses;
        private readonly IniFile _serviceIni;
        private readonly LogFile _serviceLog;
        public ConcurrentQueue<MoniResultOnDisk> QueueOfMoniResultsOnDisk { get; set; }

        public Dove(DoubleAddressWithLastConnectionCheck serverAddresses, IniFile serviceIni, LogFile serviceLog)
        {
            _serverAddresses = serverAddresses;
            _serviceIni = serviceIni;
            _serviceLog = serviceLog;
        }

        public void Fly()
        {
            while (true)
            {
                MoniResultOnDisk moniResultOnDisk;
                if (QueueOfMoniResultsOnDisk.TryPeek(out moniResultOnDisk))
                {
                    if (SendMoniResult(moniResultOnDisk))
                    {
                        QueueOfMoniResultsOnDisk.TryDequeue(out moniResultOnDisk);
                        moniResultOnDisk.Delete();
                    }
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }
        }

        private bool SendMoniResult(MoniResultOnDisk moniResultOnDisk)
        {
            moniResultOnDisk.Load();
            return new R2DWcfManager(_serverAddresses, _serviceIni, _serviceLog).SendMonitoringResult(moniResultOnDisk.Dto);
        }
    }
}

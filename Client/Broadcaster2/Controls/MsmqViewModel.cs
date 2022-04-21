using System;
using System.IO;
using System.Messaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Broadcaster2
{
    public class MsmqViewModel : PropertyChangedBase
    {
        private readonly IniFile _iniFile;

        public string ServerIp { get; set; } = "192.168.96.21";
        public string SorFileName { get; set; } = @"..\file.sor";
        public string SorBreakFileName { get; set; } = @"..\fileBreak.sor";
        public Guid RtuId;
        public Guid TraceId;
        private int _sentCount;
        public int MsmqCount { get; set; } = 1;
        public int MsmqPauseMs { get; set; } = 2000;

        public int SentCount
        {
            get => _sentCount;
            set
            {
                if (value == _sentCount) return;
                _sentCount = value;
                NotifyOfPropertyChange();
            }
        }

        public MsmqViewModel(IniFile iniFile)
        {
            _iniFile = iniFile;
        }

        public void StartSending()
        {
            var s = _iniFile.Read(IniSection.Broadcast, IniKey.MsmqTestRtuId, "00bf8f28-345f-44dc-af18-1ba6d9e4c563");
            RtuId = Guid.Parse(s);
            s = _iniFile.Read(IniSection.Broadcast, IniKey.MsmqTestTraceId, "d7f1f9ab-23bc-418d-82e6-ff755fc2b469");
            TraceId = Guid.Parse(s);
            Task.Factory.StartNew(VeryLongOperation);
        }

        private void VeryLongOperation()
        {
            SentCount = 0;
            var dto = CreateDto(false);
            var dtoBroken = CreateDto(true);

            var connectionString = $@"FormatName:DIRECT=TCP:{ServerIp}\private$\Fibertest20";
            var queue = new MessageQueue(connectionString);

            System.Messaging.Message message = new System.Messaging.Message(dto, new BinaryMessageFormatter());
            System.Messaging.Message messageBroken = new System.Messaging.Message(dtoBroken, new BinaryMessageFormatter());
            IncreaseSentCounter del = F;
            for (int i = 0; i < MsmqCount; i++)
            {
                queue.Send(i % 4 == 0 ? messageBroken : message, MessageQueueTransactionType.Single);
                Application.Current.Dispatcher?.Invoke(del);
                Thread.Sleep(MsmqPauseMs);
            }
        }

        private delegate void IncreaseSentCounter();
        private void F()
        {
            SentCount++;
        }

        private MonitoringResultDto CreateDto(bool isBroken)
        {
            var bytes = File.ReadAllBytes(isBroken ? SorBreakFileName : SorFileName);
            var dto = new MonitoringResultDto()
            {
                RtuId = RtuId,
                TimeStamp = DateTime.Now,
                PortWithTrace = new PortWithTraceDto()
                {
                    TraceId = TraceId,
                    OtauPort = new OtauPortDto() { OpticalPort = 2, Serial = "68613", IsPortOnMainCharon = true, },
                },
                BaseRefType = BaseRefType.Precise,
                TraceState = isBroken ? FiberState.FiberBreak : FiberState.Ok,
                SorBytes = bytes,
            };
            return dto;
        }
    }
}

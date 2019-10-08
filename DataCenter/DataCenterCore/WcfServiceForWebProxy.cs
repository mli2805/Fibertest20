using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForWebProxyInterface;

namespace Iit.Fibertest.DataCenterCore
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class WcfServiceForWebProxy : IWcfServiceForWebProxy
    {
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;

        public WcfServiceForWebProxy(IMyLog logFile, Model writeModel)
        {
            _logFile = logFile;
            _writeModel = writeModel;
        }

        public async Task<List<RtuDto>>GetRtuList()
        {
            await Task.Delay(1);
            _logFile.AppendLine("We are in WcfServiceForWebProxy");
            return _writeModel.Rtus.Select(r => new RtuDto()
            {
                RtuId = r.Id, 
                RtuMaker = r.RtuMaker,
                Title = r.Title, 

                MainChannel = (NetAddress)r.MainChannel.Clone(),
                MainChannelState = r.MainChannelState,
                ReserveChannel = (NetAddress)r.ReserveChannel.Clone(),
                ReserveChannelState = r.ReserveChannelState,
                IsReserveChannelSet = r.IsReserveChannelSet,
                OtdrNetAddress = (NetAddress)r.OtdrNetAddress.Clone(),
                BopState = r.BopState,

                MonitoringMode = r.MonitoringState, 
                Version = r.Version, 
                Version2 = r.Version2,
            }).ToList();
        }

        public async Task<List<OpticalEventDto>> GetOpticalEventList()
        {
            await Task.Delay(1);
            _logFile.AppendLine("We are in WcfServiceForWebProxy");
            return _writeModel.Measurements
                .Where(m => m.EventStatus > EventStatus.JustMeasurementNotAnEvent)
                .Select(m => new OpticalEventDto()
            {
                EventId = m.SorFileId, 
                RtuTitle = _writeModel.Rtus.FirstOrDefault(r=>r.Id == m.RtuId)?.Title, 
                TraceTitle = _writeModel.Traces.FirstOrDefault(t=>t.TraceId == m.TraceId)?.Title,
                TraceState = m.TraceState,
                BaseRefType = m.BaseRefType,
                EventRegistrationTimestamp = m.EventRegistrationTimestamp,
                MeasurementTimestamp = m.MeasurementTimestamp,

                EventStatus = m.EventStatus,
                StatusChangedTimestamp = m.StatusChangedTimestamp,
                StatusChangedByUser = m.StatusChangedByUser,

                Comment = m.Comment,
            }).ToList();
        }
    }
}
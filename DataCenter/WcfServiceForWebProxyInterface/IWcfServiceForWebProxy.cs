using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.WcfServiceForWebProxyInterface
{
    [ServiceContract]
    public interface IWcfServiceForWebProxy
    {
        [OperationContract]
        Task<string> GetTreeInJson();

        [OperationContract]
        Task<List<RtuDto>> GetRtuList();
        
        [OperationContract]
        Task<List<TraceDto>> GetTraceList();

        [OperationContract]
        Task<TraceStatisticsDto> GetTraceStatistics(Guid traceId);

        [OperationContract]
        Task<List<OpticalEventDto>> GetOpticalEventList();
    }
}

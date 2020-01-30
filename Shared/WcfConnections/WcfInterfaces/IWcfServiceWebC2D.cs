using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.WcfConnections
{
    [ServiceContract]
    public interface IWcfServiceWebC2D
    {
        [OperationContract]
        Task<string> GetTreeInJson(string username);

        #region RTU
        [OperationContract]
        Task<RtuInformationDto> GetRtuInformation(string username, Guid rtuId);

        [OperationContract]
        Task<RtuNetworkSettingsDto> GetRtuNetworkSettings(string username, Guid rtuId);

        [OperationContract]
        Task<RtuStateDto> GetRtuState(string username, Guid rtuId);

        [OperationContract]
        Task<RtuMonitoringSettingsDto> GetRtuMonitoringSettings(string username, Guid rtuId);

        #endregion

        #region Trace
        [OperationContract]
        Task<TraceInformationDto> GetTraceInformation(string username, Guid traceId);

        [OperationContract]
        Task<TraceStatisticsDto> GetTraceStatistics(string username, Guid traceId);
        #endregion

        [OperationContract]
        Task<List<OpticalEventDto>> GetOpticalEventList(string username, bool isCurrentEvents,
            string filterRtu, string filterTrace, string sortOrder, int pageNumber, int pageSize);
    }
}

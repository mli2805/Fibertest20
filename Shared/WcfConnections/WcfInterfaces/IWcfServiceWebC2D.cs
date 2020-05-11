using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.WcfConnections
{
    [ServiceContract]
    public interface IWcfServiceWebC2D
    {
        [OperationContract]
        Task<string> GetAboutInJson(string username);

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
        Task<TreeOfAcceptableMeasParams> GetRtuAcceptableMeasParams(string username, Guid rtuId);

        [OperationContract]
        Task<RtuMonitoringSettingsDto> GetRtuMonitoringSettings(string username, Guid rtuId);

        [OperationContract]
        Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto);
        #endregion

        #region Trace
        [OperationContract]
        Task<TraceInformationDto> GetTraceInformation(string username, Guid traceId);

        [OperationContract]
        Task<TraceStatisticsDto> GetTraceStatistics(string username, Guid traceId, int pageNumber, int pageSize);

         [OperationContract]
        Task<TraceStateDto> GetTraceState(string username, string requestBody);

        [OperationContract]
        Task<AssignBaseParamsDto> GetAssignBaseParams(string username, Guid traceId);

        #endregion

       [OperationContract]
        Task<OpticalEventsRequestedDto> GetOpticalEventPortion(string username, bool isCurrentEvents,
            string filterRtu, string filterTrace, string sortOrder, int pageNumber, int pageSize);
    }
}

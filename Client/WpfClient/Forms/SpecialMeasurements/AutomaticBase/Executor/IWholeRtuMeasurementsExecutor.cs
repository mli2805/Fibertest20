using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public interface IWholeRtuMeasurementsExecutor
    {
        MeasurementModel Model { get; set; }
        bool Initialize(Rtu rtu);
        Task StartOneMeasurement(RtuAutoBaseProgress item, bool keepOtdrConnection = false);
        void ProcessMeasurementResult(ClientMeasurementResultDto dto);
        Task SetAsBaseRef(byte[] sorBytes, Trace trace);

        event WholeRtuMeasurementsExecutor.MeasurementHandler MeasurementCompleted;
        event WholeRtuMeasurementsExecutor.BaseRefHandler BaseRefAssigned;
    }
}
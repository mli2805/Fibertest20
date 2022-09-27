using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public interface IOneMeasurementExecutor
    {
        MeasurementModel Model { get; set; }
        bool Initialize(Rtu rtu, bool isForRtu);
        Task Start(TraceLeaf traceLeaf, bool keepOtdrConnection = false);
        void ProcessMeasurementResult(ClientMeasurementResultDto dto);

        event OneIitMeasurementExecutor.MeasurementHandler MeasurementCompleted;
    }
}
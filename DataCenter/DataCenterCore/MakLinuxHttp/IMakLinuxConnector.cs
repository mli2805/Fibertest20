using Iit.Fibertest.Dto;
using System.Threading.Tasks;

namespace Iit.Fibertest.DataCenterCore
{
    public interface IMakLinuxConnector
    {
        Task<RtuConnectionCheckedDto> CheckRtuConnection(CheckRtuConnectionDto dto);
        Task<RtuCurrentStateDto> GetRtuCurrentState(GetCurrentRtuStateDto dto);

        Task<TResult> SendCommand<T, TResult>(T dto, DoubleAddress rtuDoubleAddress)
            where TResult : RequestAnswer, new();

    }
}

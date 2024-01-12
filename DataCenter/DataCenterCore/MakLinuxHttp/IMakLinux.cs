using Iit.Fibertest.Dto;
using System.Threading.Tasks;

namespace Iit.Fibertest.DataCenterCore
{
    interface IMakLinux
    {
        Task<RtuConnectionCheckedDto> CheckRtuConnection(CheckRtuConnectionDto dto);

    }

    public class MakLinuxHttpConnector : IMakLinux
    {
        public Task<RtuConnectionCheckedDto> CheckRtuConnection(CheckRtuConnectionDto dto)
        {
            throw new System.NotImplementedException();

        }

    }
}

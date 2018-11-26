using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Graph
{
    public static class OtauPortDtoExt
    {
        public static string OpticalPortToString(this OtauPortDto dto)
        {
//            return dto.IsPortOnMainCharon
//                ? dto.OpticalPort.ToString()
//                : string.Format(Resources.SID__0__on_BOP__1___2_, dto.OpticalPort, dto.OtauIp, dto.OtauTcpPort);

            return $"{dto.OpticalPort} on BOP {dto.Serial}";
        }
    }
}
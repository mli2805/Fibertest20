using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public static class OtauPortDtoProvider
    {
        // AutoBase Whole RTU
        public static List<List<OtauPortDto>> PrepareBanchOfOtauPortDto(this RtuLeaf rtuLeaf, Model readModel)
        {
            var rtu = readModel.Rtus.First(r => r.Id == rtuLeaf.Id);
            var traceLeaves = rtuLeaf.GetAttachedTraces();

            var portList = new List<List<OtauPortDto>>();
            foreach (var traceLeaf in traceLeaves)
            {
                var portOwner = (IPortOwner)traceLeaf.Parent;
                var otau = readModel.Otaus.FirstOrDefault(o => o.Serial == portOwner.Serial);
                portList.Add(PrepareOtauPortDto(rtu, otau, portOwner, traceLeaf.PortNumber));
            }

            return portList;
        }

        public static List<OtauPortDto> PreparePairOfOtauPortDto(this Leaf parent, int portNumber, Model readModel)
        {
            var rtuId = (parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent).Id;
            var portOwner = (IPortOwner)parent;
            var rtu = readModel.Rtus.First(r => r.Id == rtuId);
            var otau = readModel.Otaus.FirstOrDefault(o => o.Serial == portOwner.Serial);

            return PrepareOtauPortDto(rtu, otau, portOwner, portNumber);
        }

        private static List<OtauPortDto> PrepareOtauPortDto(Rtu rtu, Otau otau, IPortOwner otauLeaf, int portNumber)
        {
            var otauId = otau == null
                ? rtu.MainVeexOtau.id
                : otau.Id.ToString();

            var otauPortDto = new OtauPortDto()
            {
                OtauId = otauId,
                OpticalPort = portNumber,
                Serial = otauLeaf.Serial,
                IsPortOnMainCharon = otauLeaf is RtuLeaf,
                MainCharonPort = otau?.MasterPort ?? 1
            };

            var result = new List<OtauPortDto>() { otauPortDto };

            if (!otauPortDto.IsPortOnMainCharon && rtu.RtuMaker == RtuMaker.VeEX)
            {
                result.Add(new OtauPortDto() // Veex requires Main OTAU also
                {
                    IsPortOnMainCharon = true,
                    OtauId = rtu.MainVeexOtau.id,
                    OpticalPort = otauPortDto.MainCharonPort,
                });
            }

            return result;
        }
    }
}
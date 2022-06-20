using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public static class ClientMeasurementDtoFactory
    {
        public static DoClientMeasurementDto CreateDoClientMeasurementDto(this RtuLeaf rtuLeaf, Model readModel, CurrentUser currentUser)
        {
            var rtu = readModel.Rtus.First(r => r.Id == rtuLeaf.Id);

            return new DoClientMeasurementDto()
            {
                ConnectionId = currentUser.ConnectionId,
                RtuId = rtu.Id,
                OtdrId = rtu.OtdrId,

                OtauPortDtoList = rtuLeaf.PrepareBanchOfOtauPortDto(readModel),
            };
        }

        public static DoClientMeasurementDto CreateDoClientMeasurementDto(this Leaf parent, int portNumber, Model readModel, CurrentUser currentUser)
        {
            var rtuId = (parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent).Id;
            var rtu = readModel.Rtus.First(r => r.Id == rtuId);

            var listOfOtauPortDto = parent.PreparePairOfOtauPortDto(portNumber, readModel);

            return new DoClientMeasurementDto()
            {
                ConnectionId = currentUser.ConnectionId,
                RtuId = rtu.Id,
                OtdrId = rtu.OtdrId,

                OtauPortDtoList = new List<List<OtauPortDto>>() { listOfOtauPortDto },
            };
        }

        public static DoClientMeasurementDto SetParams(this DoClientMeasurementDto dto, bool isForAutoBase,
            bool isAutoLmax, List<MeasParamByPosition> iitMeasParams, VeexMeasOtdrParameters veexMeasParams)
        {
            dto.SelectedMeasParams = iitMeasParams;
            dto.VeexMeasOtdrParameters = veexMeasParams;

            dto.AnalysisParameters = new AnalysisParameters()
            {
                lasersParameters = new List<LasersParameter>()
                {
                    new LasersParameter()
                        { eventLossThreshold = 0.2, eventReflectanceThreshold = -40, endOfFiberThreshold = 6 }
                }
            };

            dto.IsForAutoBase = isForAutoBase;
            dto.IsAutoLmax = isAutoLmax;
            return dto;
        }
    }
}
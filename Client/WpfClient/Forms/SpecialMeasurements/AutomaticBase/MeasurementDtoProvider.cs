using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class MeasurementDtoProvider
    {
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;

        private IPortOwner _portOwner;
        private Rtu _rtu;
        private OtauPortDto _otauPortDto;

        private bool _isForAutoBase;

        public MeasurementDtoProvider(CurrentUser currentUser, Model readModel)
        {
            _currentUser = currentUser;
            _readModel = readModel;
        }

        public MeasurementDtoProvider Initialize(TraceLeaf traceLeaf, bool isForAutoBase)
        {
            var parent = traceLeaf.Parent;
            var rtuId = (parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent).Id;
            _portOwner = (IPortOwner)parent;
            _rtu = _readModel.Rtus.First(r => r.Id == rtuId);

            var otau = _readModel.Otaus.FirstOrDefault(o => o.Serial == _portOwner.Serial);
            _otauPortDto = PrepareOtauPortDto(_rtu, otau, _portOwner, traceLeaf.PortNumber);

            _isForAutoBase = isForAutoBase;

            return this;
        }

        public MeasurementDtoProvider Initialize(Leaf parent, int portNumber, bool isForAutoBase)
        {
            var rtuId = (parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent).Id;
            _portOwner = (IPortOwner)parent;
            _rtu = _readModel.Rtus.First(r => r.Id == rtuId);

            var otau = _readModel.Otaus.FirstOrDefault(o => o.Serial == _portOwner.Serial);
            _otauPortDto = PrepareOtauPortDto(_rtu, otau, _portOwner, portNumber);

            _isForAutoBase = isForAutoBase;

            return this;
        }

        private OtauPortDto PrepareOtauPortDto(Rtu rtu, Otau otau, IPortOwner otauLeaf, int portNumber)
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
            return otauPortDto;
        }

        public DoClientMeasurementDto PrepareDto(bool isAutoLmax, List<MeasParamByPosition> iitMeasParams, VeexMeasOtdrParameters veexMeasParams)
        {
            var dto = new DoClientMeasurementDto()
            {
                ConnectionId = _currentUser.ConnectionId,
                RtuId = _rtu.Id,
                OtdrId = _rtu.OtdrId,
                OtauIp = _portOwner.OtauNetAddress.Ip4Address,
                OtauTcpPort = _portOwner.OtauNetAddress.Port,

                SelectedMeasParams = iitMeasParams,
                VeexMeasOtdrParameters = veexMeasParams,

                AnalysisParameters = new AnalysisParameters()
                {
                    lasersParameters = new List<LasersParameter>()
                    {
                        new LasersParameter(){ eventLossThreshold = 0.2, eventReflectanceThreshold = -40, endOfFiberThreshold = 6 }
                    }
                },

                IsForAutoBase = _isForAutoBase,
                IsAutoLmax = isAutoLmax,
            };
            dto.OtauPortDtoList.Add(_otauPortDto);

            if (!_otauPortDto.IsPortOnMainCharon && _rtu.RtuMaker == RtuMaker.VeEX)
            {
                dto.MainOtauPortDto = new OtauPortDto()
                {
                    IsPortOnMainCharon = true,
                    OtauId = _rtu.MainVeexOtau.id,
                    OpticalPort = _otauPortDto.MainCharonPort,
                };
            }

            return dto;
        }
    }
}
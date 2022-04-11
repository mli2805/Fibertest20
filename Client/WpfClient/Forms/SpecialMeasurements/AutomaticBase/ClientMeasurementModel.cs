using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class ClientMeasurementModel
    {
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;

        private RtuLeaf _rtuLeaf;
        private IPortOwner _portOwner;
        public Rtu Rtu;
        private OtauPortDto _otauPortDto;

        private bool _isForAutoBase;

        public ClientMeasurementModel(CurrentUser currentUser, Model readModel)
        {
            _currentUser = currentUser;
            _readModel = readModel;
        }

        public void Initialize(TraceLeaf traceLeaf, bool isForAutoBase)
        {
            var parent = traceLeaf.Parent;
            _rtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
            _portOwner = (IPortOwner)parent;
            Rtu = _readModel.Rtus.First(r => r.Id == _rtuLeaf.Id);

            var otau = _readModel.Otaus.FirstOrDefault(o => o.Serial == _portOwner.Serial);
            _otauPortDto = PrepareOtauPortDto(Rtu, otau, _portOwner, traceLeaf.PortNumber);

            _isForAutoBase = isForAutoBase;
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

        public DoClientMeasurementDto PrepareDto(List<MeasParam> iitMeasParams, VeexMeasOtdrParameters veexMeasParams)
        {
            var dto = new DoClientMeasurementDto()
            {
                ConnectionId = _currentUser.ConnectionId,
                RtuId = Rtu.Id,
                OtdrId = Rtu.OtdrId,
                OtauIp = _portOwner.OtauNetAddress.Ip4Address,
                OtauTcpPort = _portOwner.OtauNetAddress.Port,

                SelectedMeasParams = iitMeasParams,
                VeexMeasOtdrParameters = veexMeasParams,

                IsForAutoBase = _isForAutoBase,
            };
            dto.OtauPortDtoList.Add(_otauPortDto);

            if (!_otauPortDto.IsPortOnMainCharon && Rtu.RtuMaker == RtuMaker.VeEX)
            {
                dto.MainOtauPortDto = new OtauPortDto()
                {
                    IsPortOnMainCharon = true,
                    OtauId = Rtu.MainVeexOtau.id,
                    OpticalPort = _otauPortDto.MainCharonPort,
                };
            }

            return dto;
        }
    }
}
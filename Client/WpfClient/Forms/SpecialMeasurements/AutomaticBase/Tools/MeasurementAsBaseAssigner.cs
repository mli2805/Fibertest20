using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    public class MeasurementAsBaseAssigner
    {
        private readonly IWcfServiceCommonC2D _c2DWcfCommonManager;
        private readonly BaseRefMessages _baseRefMessages;

        private CurrentUser _currentUser;
        private Rtu _rtu;

        public MeasurementAsBaseAssigner(CurrentUser currentUser, IWcfServiceCommonC2D c2DWcfCommonManager, 
            BaseRefMessages baseRefMessages)
        {
            _currentUser = currentUser;
            _c2DWcfCommonManager = c2DWcfCommonManager;
            _baseRefMessages = baseRefMessages;
        }

        public void Initialize(Rtu rtu)
        {
            _rtu = rtu;
        }

        public async Task<BaseRefAssignedDto> Assign(OtdrDataKnownBlocks sorData, Trace trace,
            MeasurementProgressViewModel measurementProgressViewModel)
        {
            var dto = PrepareDto(sorData.ToBytes(), trace);
            var result = await _c2DWcfCommonManager.AssignBaseRefAsync(dto); // send to Db and RTU

            measurementProgressViewModel.ControlVisibility = Visibility.Collapsed;
            if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                _baseRefMessages.Display(result, trace);

            return result;
        }

        private AssignBaseRefsDto PrepareDto(byte[] sorBytes, Trace trace)
        {
            var dto = new AssignBaseRefsDto()
            {
                RtuId = trace.RtuId,
                RtuMaker = _rtu.RtuMaker,
                OtdrId = _rtu.OtdrId,
                TraceId = trace.TraceId,
                OtauPortDto = trace.OtauPort,
                BaseRefs = new List<BaseRefDto>(),
                DeleteOldSorFileIds = new List<int>()
            };

            if (trace.OtauPort != null && !trace.OtauPort.IsPortOnMainCharon && _rtu.RtuMaker == RtuMaker.VeEX)
            {
                dto.MainOtauPortDto = new OtauPortDto()
                {
                    IsPortOnMainCharon = true,
                    OtauId = _rtu.MainVeexOtau.id,
                    OpticalPort = trace.OtauPort.MainCharonPort,
                };
            }

            dto.BaseRefs = new List<BaseRefDto>()
            {
                BaseRefDtoFactory.CreateFromBytes(BaseRefType.Precise, sorBytes, _currentUser.UserName),
                BaseRefDtoFactory.CreateFromBytes(BaseRefType.Fast, sorBytes, _currentUser.UserName)
            };
            return dto;
        }
    }
}

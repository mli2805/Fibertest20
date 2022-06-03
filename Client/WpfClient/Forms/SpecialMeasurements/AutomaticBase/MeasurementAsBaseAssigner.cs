using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    public class MeasurementAsBaseAssigner
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWcfServiceCommonC2D _c2DWcfCommonManager;
        private readonly LandmarksTool _landmarksTool;
        private readonly BaseRefMessages _baseRefMessages;

        private CurrentUser _currentUser;
        private Trace _trace;
        private Rtu _rtu;

        public MeasurementAsBaseAssigner(ILifetimeScope globalScope, 
            IWcfServiceCommonC2D c2DWcfCommonManager, LandmarksTool landmarksTool, BaseRefMessages baseRefMessages)
        {
            _globalScope = globalScope;
            _c2DWcfCommonManager = c2DWcfCommonManager;
            _landmarksTool = landmarksTool;
            _baseRefMessages = baseRefMessages;
        }

        public void Initialize(CurrentUser currentUser, Rtu rtu, Trace trace)
        {
            _rtu = rtu;
            _trace = trace;
            _currentUser = currentUser;
        }

        public async Task<bool> ProcessMeasurementResult(OtdrDataKnownBlocks sorData, 
            MeasurementProgressViewModel measurementProgressViewModel)
        {
            _landmarksTool.ApplyTraceToAutoBaseRef(sorData, _trace);

            BaseRefAssignedDto result;
            using (_globalScope.Resolve<IWaitCursor>())
            {
                var dto = PrepareDto(sorData.ToBytes());
                result = await _c2DWcfCommonManager.AssignBaseRefAsync(dto); // send to Db and RTU
            }

            measurementProgressViewModel.ControlVisibility = Visibility.Collapsed;
            if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                _baseRefMessages.Display(result, _trace);

            return result.ReturnCode == ReturnCode.BaseRefAssignedSuccessfully;
        }

        private AssignBaseRefsDto PrepareDto(byte[] sorBytes)
        {
            var dto = new AssignBaseRefsDto()
            {
                RtuId = _trace.RtuId,
                RtuMaker = _rtu.RtuMaker,
                OtdrId = _rtu.OtdrId,
                TraceId = _trace.TraceId,
                OtauPortDto = _trace.OtauPort,
                BaseRefs = new List<BaseRefDto>(),
                DeleteOldSorFileIds = new List<int>()
            };

            if (_trace.OtauPort != null && !_trace.OtauPort.IsPortOnMainCharon && _rtu.RtuMaker == RtuMaker.VeEX)
            {
                dto.MainOtauPortDto = new OtauPortDto()
                {
                    IsPortOnMainCharon = true,
                    OtauId = _rtu.MainVeexOtau.id,
                    OpticalPort = _trace.OtauPort.MainCharonPort,
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

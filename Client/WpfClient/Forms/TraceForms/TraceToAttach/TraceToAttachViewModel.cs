using System.Collections.Generic;
using System.Linq;
using Autofac;
using AutoMapper;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class TraceToAttachViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly IWcfServiceCommonC2D _c2DCommonWcfManager;
        private readonly IWcfServiceDesktopC2D _c2DDesktopWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly CurrentUser _currentUser;
        private OtauPortDto _otauPortDto;
        private Rtu _rtu;
        private Trace _selectedTrace;

        public List<Trace> Choices { get; set; }

        public Trace SelectedTrace
        {
            get { return _selectedTrace; }
            set
            {
                if (Equals(value, _selectedTrace)) return;
                _selectedTrace = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isButtonsEnabled = true;
        public bool IsButtonsEnabled
        {
            get { return _isButtonsEnabled; }
            set
            {
                if (value == _isButtonsEnabled) return;
                _isButtonsEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public TraceToAttachViewModel(ILifetimeScope globalScope, Model readModel, CurrentUser currentUser,
            IWcfServiceCommonC2D c2DCommonWcfManager, IWcfServiceDesktopC2D c2DDesktopWcfManager, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _c2DCommonWcfManager = c2DCommonWcfManager;
            _c2DDesktopWcfManager = c2DDesktopWcfManager;
            _windowManager = windowManager;
            _currentUser = currentUser;
        }

        public void Initialize(Rtu rtu, OtauPortDto otauPortDto)
        {
            _rtu = rtu;
            _otauPortDto = otauPortDto;
            Choices = _readModel.Traces.Where(t => t.RtuId == rtu.Id && t.Port < 1 && t.ZoneIds.Contains(_currentUser.ZoneId)).ToList();
            SelectedTrace = Choices.FirstOrDefault();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Not_attached_traces_list;
        }

        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<VeexTestMappingProfile>()).CreateMapper();
        public async void FullAttach()
        {
            IsButtonsEnabled = false;
            var dto = new AttachTraceDto()
            {
                TraceId = SelectedTrace.TraceId,
                OtauPortDto = _otauPortDto,
                MainOtauPortDto = new OtauPortDto()
                {
                    IsPortOnMainCharon = true,
                    OtauId = _rtu.OtauId,
                    OpticalPort = _otauPortDto.MainCharonPort
                },
            };

            var result = await _c2DCommonWcfManager.AttachTraceAndSendBaseRefs(dto);
            switch (result.ReturnCode)
            {
                case ReturnCode.Ok: break;
                case ReturnCode.BaseRefAssignedSuccessfully:
                {
                    if (_rtu.RtuMaker == RtuMaker.VeEX && result.VeexTests != null)
                    {
                        using (_globalScope.Resolve<IWaitCursor>())
                        {
                            var commands = result.VeexTests
                                .Select(l => (object) (Mapper.Map<AddVeexTest>(l))).ToList();

                            if (commands.Any())
                                await _c2DDesktopWcfManager.SendCommandsAsObjs(commands);
                        }
                    }

                    break;
                }
                default:
                {
                    var errs = new List<string>
                    {
                        result.ReturnCode == ReturnCode.D2RWcfConnectionError
                            ? Resources.SID_Cannot_send_base_refs_to_RTU
                            : Resources.SID_Base_reference_assignment_failed
                    };

                    if (!string.IsNullOrEmpty(result.ErrorMessage))
                        errs.Add(result.ErrorMessage);

                    var vm = new MyMessageBoxViewModel(MessageType.Error, errs);
                    _windowManager.ShowDialog(vm);
                    break;
                }
            }

            IsButtonsEnabled = true;
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}
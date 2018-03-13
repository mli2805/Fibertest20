using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Algorithms;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class TraceToAttachViewModel : Screen
    {
        private readonly ReadModel _readModel;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly AccidentsExtractorFromSor _accidentsExtractorFromSor;
        private OtauPortDto _otauPortDto;
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

        public TraceToAttachViewModel(ReadModel readModel, IWcfServiceForClient c2DWcfManager,
            IWindowManager windowManager, AccidentsExtractorFromSor accidentsExtractorFromSor)
        {
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _accidentsExtractorFromSor = accidentsExtractorFromSor;
        }

        public void Initialize(Guid rtuId, OtauPortDto otauPortDto)
        {
            _otauPortDto = otauPortDto;
            Choices = _readModel.Traces.Where(t => t.RtuId == rtuId && t.Port < 1).ToList();
            SelectedTrace = Choices.FirstOrDefault();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Not_attached_traces_list;
        }

        public async void Attach()
        {
            var cmd = new ReSendBaseRefsDto()
            {
                TraceId = _selectedTrace.Id,
                RtuId = _selectedTrace.RtuId,
                OtauPortDto = _otauPortDto,
            };
            var result = await _c2DWcfManager.ReSendBaseRefAsync(cmd);
            if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
            {
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Cannot_send_base_refs_to_RTU));
                return;
            }

            var command = new AttachTrace()
            {
                TraceId = SelectedTrace.Id,
                OtauPortDto = _otauPortDto,
            };

            MeasurementWithSor measurementWithSor = await _c2DWcfManager.GetLastMeasurementForTrace(_selectedTrace.Id);
            if (measurementWithSor != null && measurementWithSor.Measurement != null)
            {
                command.PreviousTraceState = measurementWithSor.Measurement.TraceState;
                if (measurementWithSor.Measurement.TraceState != FiberState.Ok &&
                    measurementWithSor.Measurement.TraceState != FiberState.NoFiber)
                    command.AccidentsInLastMeasurement = _accidentsExtractorFromSor.GetAccidents(SorData.FromBytes(measurementWithSor.SorBytes));
            }
            else
            { // trace has no measurements so far 
                command.PreviousTraceState = FiberState.Unknown;
            }

            await _c2DWcfManager.SendCommandAsObj(command);
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}
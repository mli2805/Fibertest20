using System;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class RtuAccidentViewModel : Screen
    {
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly SoundManager _soundManager;
        private bool _isSoundForThisVmInstanceOn;
        public bool IsOpen { get; private set; }

        public RtuAccidentModel AccidentModel { get; set; }
        public string ServerTitle { get; set; }

        public RtuAccidentViewModel(CurrentDatacenterParameters currentDatacenterParameters, SoundManager soundManager)
        {
            _currentDatacenterParameters = currentDatacenterParameters;
            _soundManager = soundManager;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_RTU_status_event;
            IsOpen = true;

            if (AccidentModel.Accident.ReturnCode == ReturnCode.MeasurementEndedNormally)
            {
                _soundManager.PlayOk();
            }
            else
            {
                _isSoundForThisVmInstanceOn = true;
                _soundManager.StartAlert();
                AccidentModel.IsSoundButtonEnabled = true;
            }
        }

        public void Initialize(RtuAccidentModel rtuAccidentModel)
        {
            AccidentModel = rtuAccidentModel;
            ServerTitle = _currentDatacenterParameters.ServerTitle;
        }

        public void TurnSoundOff()
        {
            if (_isSoundForThisVmInstanceOn)
                _soundManager.StopAlert();
            _isSoundForThisVmInstanceOn = false;
            AccidentModel.IsSoundButtonEnabled = false;
        }

        public override void CanClose(Action<bool> callback)
        {
            if (_isSoundForThisVmInstanceOn)
                _soundManager.StopAlert();
            IsOpen = false;
            callback(true);
        }

        public void Close() { TryClose(); }
    }

    
}

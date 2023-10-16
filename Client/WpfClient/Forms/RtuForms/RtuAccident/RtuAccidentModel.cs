using System.Globalization;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class RtuAccidentModel : PropertyChangedBase
    {
        public RtuAccident Accident { get; set; }

        public string RtuTitle { get; set; }
        public string TraceTitle { get; set; }

        public string State { get; set; }
        public string Explanation { get; set; }
        public Brush StateBackground { get; set; }

        public string Port { get; set; }
        public string StateAt => string.Format(Resources.SID_State_at_,
            Accident.EventRegistrationTimestamp.ToString(CultureInfo.CurrentCulture), Accident.Id);
        public Brush StateForeground { get; set; }

        private bool _isSoundButtonEnabled;
        public bool IsSoundButtonEnabled
        {
            get => _isSoundButtonEnabled;
            set
            {
                if (value == _isSoundButtonEnabled) return;
                _isSoundButtonEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public RtuAccidentModel(RtuAccident accident)
        {
            Accident = accident;
        }

        public RtuAccidentModel Build(Model readModel)
        {
            RtuTitle = readModel.Rtus.FirstOrDefault(r => r.Id == Accident.RtuId)?.Title ?? string.Empty;
            var trace = readModel.Traces.FirstOrDefault(t => t.TraceId == Accident.TraceId);
            TraceTitle = trace?.Title ?? string.Empty;
            Port = trace?.OtauPort.ToStringB() ?? string.Empty;

            SetStateAndBrush();
            return this;
        }

        private void SetStateAndBrush()
        {
            switch (Accident.ReturnCode)
            {
                case ReturnCode.MeasurementEndedNormally:
                case ReturnCode.MeasurementErrorCleared:
                case ReturnCode.MeasurementErrorClearedByInit:
                    State = Accident.ReturnCode.RtuStatusEventToLocalizedString();
                    Explanation = Accident.ReturnCode.GetLocalizedString();
                    StateBackground = Brushes.Transparent;
                    StateForeground = Brushes.Black;
                    break;
                case ReturnCode.MeasurementBaseRefNotFound:
                    State = Resources.SID_Measurement__Failed_;
                    Explanation = string.Format(Accident.ReturnCode.GetLocalizedString(), Accident.BaseRefType.GetLocalizedFemaleString());
                    StateBackground = FiberState.Critical.GetBrush(false);
                    StateForeground = FiberState.Critical.GetBrush(true);
                    break;
                case ReturnCode.MeasurementFailedToSetParametersFromBase:
                case ReturnCode.MeasurementAnalysisFailed:
                case ReturnCode.MeasurementComparisonFailed:
                    State = Resources.SID_Measurement__Failed_;
                    Explanation = string.Format(Accident.ReturnCode.GetLocalizedString(), Accident.BaseRefType.GetLocalizedGenitiveString());
                    StateBackground = FiberState.Critical.GetBrush(false);
                    StateForeground = FiberState.Critical.GetBrush(true);
                    break;

                case ReturnCode.RtuManagerServiceWorking:
                    State = Accident.ReturnCode.RtuStatusEventToLocalizedString();
                    Explanation = Accident.ReturnCode.GetLocalizedString();
                    StateBackground = Brushes.Transparent;
                    StateForeground = Brushes.White;
                    break;
                case ReturnCode.RtuFrequentServiceRestarts:
                    State = Accident.ReturnCode.RtuStatusEventToLocalizedString();
                    Explanation = Accident.ReturnCode.GetLocalizedString();
                    StateBackground = FiberState.Minor.GetBrush(false);
                    StateForeground = FiberState.Minor.GetBrush(true);
                    break;

                default:
                    State = Accident.ReturnCode.RtuStatusEventToLocalizedString();
                    Explanation = Accident.ReturnCode.GetLocalizedString();
                    StateBackground = Brushes.Red;
                    StateForeground = Brushes.Black;
                    break;
            }

        }
    }
}

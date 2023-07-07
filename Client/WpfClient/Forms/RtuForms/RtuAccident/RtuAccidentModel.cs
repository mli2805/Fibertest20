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

        #region RtuAccidentViewModel

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
        #endregion

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
                    State = Resources.SID_Measurement__OK;
                    Explanation = Accident.ReturnCode.GetLocalizedString();
                    StateBackground = Brushes.Transparent;
                    StateForeground = Brushes.White;
                    break;
                case ReturnCode.MeasurementBaseRefNotFound:
                    State = Resources.SID_Measurement__Failed_;
                    Explanation = $@"{Accident.BaseRefType.GetLocalizedFemaleString()} {Accident.ReturnCode.GetLocalizedString()}";
                    StateBackground = FiberState.Critical.GetBrush(false);
                    StateForeground = FiberState.Critical.GetBrush(true);
                    break;
                case ReturnCode.MeasurementFailedToSetParametersFromBase:
                    State = Resources.SID_Measurement__Failed_;
                    Explanation = string.Format(Accident.ReturnCode.GetLocalizedString(), Accident.BaseRefType.GetLocalizedGenitiveString());
                    StateBackground = FiberState.Critical.GetBrush(false);
                    StateForeground = FiberState.Critical.GetBrush(true);
                    break;

                case ReturnCode.RtuRestored:
                    State = Resources.SID_RTU__OK;
                    Explanation = string.Format(Accident.ReturnCode.GetLocalizedString(), Accident.BaseRefType.GetLocalizedGenitiveString());
                    StateBackground = Brushes.Transparent;
                    StateForeground = Brushes.White;
                    break;
                case ReturnCode.RtuFrequentServiceRestarts:
                    State = Resources.SID_RTU__Attention_required_;
                    Explanation = string.Format(Accident.ReturnCode.GetLocalizedString(), Accident.BaseRefType.GetLocalizedGenitiveString());
                    StateBackground = FiberState.Minor.GetBrush(false);
                    StateForeground = FiberState.Minor.GetBrush(true);
                    break;

                default:
                    State = @"Unknown type of accident";
                    Explanation = @"Unknown type of accident";
                    StateBackground = Brushes.Red;
                    StateForeground = Brushes.Black;
                    break;
            }

        }
    }
}

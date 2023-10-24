using System.Linq;
using System.Windows.Media;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class RtuAccidentLineModel
    {
        public RtuAccident Accident { get; set; }

        public string RtuTitle { get; set; }
        public string TraceTitle { get; set; }

        public string State { get; set; }
        public Brush StateBackground { get; set; }
        public string Explanation { get; set; }


        public RtuAccidentLineModel(RtuAccident accident)
        {
            Accident = accident;
        }

        public RtuAccidentLineModel Build(Model readModel)
        {
            RtuTitle = readModel.Rtus.FirstOrDefault(r => r.Id == Accident.RtuId)?.Title ?? string.Empty;
            var trace = readModel.Traces.FirstOrDefault(t => t.TraceId == Accident.TraceId);
            TraceTitle = trace?.Title ?? string.Empty;

            SetStateAndBrush();
            return this;
        }

        private void SetStateAndBrush()
        {
            switch (Accident.ReturnCode)
            {
                case ReturnCode.MeasurementEndedNormally:
                    State = Accident.ReturnCode.RtuStatusEventToLocalizedString();
                    Explanation = Accident.ReturnCode.GetLocalizedString();
                    StateBackground = Brushes.Transparent;
                    break;
                case ReturnCode.MeasurementErrorCleared:
                case ReturnCode.MeasurementErrorClearedByInit:
                    State = string.Format(Resources.SID_Cleared__ID___0_, Accident.ClearedAccidentWithId == 0 ? @"-" : Accident.ClearedAccidentWithId.ToString());
                    Explanation = Accident.ReturnCode.GetLocalizedString();
                    StateBackground = Brushes.Transparent;
                    break;
                case ReturnCode.MeasurementBaseRefNotFound:
                    State = Accident.ReturnCode.RtuStatusEventToLocalizedString();
                    Explanation = string.Format(Accident.ReturnCode.GetLocalizedString(), Accident.BaseRefType.GetLocalizedFemaleString());
                    StateBackground = FiberState.Critical.GetBrush(false);
                    break;
                case ReturnCode.MeasurementFailedToSetParametersFromBase:
                case ReturnCode.MeasurementAnalysisFailed:
                case ReturnCode.MeasurementComparisonFailed:
                    State = Accident.ReturnCode.RtuStatusEventToLocalizedString();
                    Explanation = string.Format(Accident.ReturnCode.GetLocalizedString(), Accident.BaseRefType.GetLocalizedGenitiveString());
                    StateBackground = FiberState.Critical.GetBrush(false);
                    break;

                case ReturnCode.RtuManagerServiceWorking:
                    State = Accident.ReturnCode.RtuStatusEventToLocalizedString();
                    Explanation = Accident.ReturnCode.GetLocalizedString();
                    StateBackground = Brushes.Transparent;
                    break;
                case ReturnCode.RtuFrequentServiceRestarts:
                    State = Accident.ReturnCode.RtuStatusEventToLocalizedString();
                    Explanation = Accident.ReturnCode.GetLocalizedString();
                    StateBackground = FiberState.Minor.GetBrush(false);
                    break;

                default:
                    State = Accident.ReturnCode.RtuStatusEventToLocalizedString();
                    Explanation = Accident.ReturnCode.GetLocalizedString();
                    StateBackground = Brushes.Red;
                    break;
            }

        }

    }
}
using System.Linq;
using System.Windows.Media;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class RtuAccidentLineModel
    {
        public RtuAccident Accident { get; set; }

        public string RtuTitle { get; set; }
        public string TraceTitle { get; set; }

        public string State { get; set; }
        public Brush StateBackground { get; set; }


        public RtuAccidentLineModel(RtuAccident accident)
        {
            Accident = accident;
        }

        public RtuAccidentLineModel Build(Model readModel)
        {
            RtuTitle = readModel.Rtus.FirstOrDefault(r=>r.Id == Accident.RtuId)?.Title ?? string.Empty;
            var trace = readModel.Traces.FirstOrDefault(t=>t.TraceId == Accident.TraceId);
            TraceTitle = trace?.Title ?? string.Empty;

            SetStateAndBrush();
            return this;
        }

        private void SetStateAndBrush()
        {
            switch (Accident.ReturnCode)
            {
                case ReturnCode.MeasurementEndedNormally:
                    State = Accident.ReturnCode.GetLocalizedString();
                    StateBackground = Brushes.Transparent;
                    break;
                case ReturnCode.MeasurementBaseRefNotFound:
                    State = $@"{Accident.BaseRefType.GetLocalizedFemaleString()} {Accident.ReturnCode.GetLocalizedString()}";
                    StateBackground = FiberState.Minor.GetBrush(false);
                    break;
                case ReturnCode.MeasurementFailedToSetParametersFromBase:
                    State = string.Format(Accident.ReturnCode.GetLocalizedString(), Accident.BaseRefType.GetLocalizedGenitiveString());
                    StateBackground = FiberState.Minor.GetBrush(false);
                    break;
                default: 
                    State = @"Unknown type of accident"; 
                    StateBackground = Brushes.Red;
                    break;
            }
            
        }

    }
}
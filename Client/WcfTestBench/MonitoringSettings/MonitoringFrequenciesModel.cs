using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace WcfTestBench.MonitoringSettings
{
    public class MonitoringFrequenciesModel : PropertyChangedBase
    {
        public List<Frequency> PreciseMeasFreqs { get; set; }

        private Frequency _selectedPreciseMeasFreq;
        public Frequency SelectedPreciseMeasFreq
        {
            get { return _selectedPreciseMeasFreq; }
            set
            {
                if (_selectedPreciseMeasFreq == value)
                    return;
                _selectedPreciseMeasFreq = value;
                ValidatePreciseSaveFrequency();
            }
        }

        private void ValidatePreciseSaveFrequency()
        {
            if (SelectedPreciseSaveFreq < SelectedPreciseMeasFreq)
                SelectedPreciseSaveFreq = SelectedPreciseMeasFreq;
            PreciseSaveFreqs = FastSaveFreqs.Where(f => f == Frequency.DoNot || f >= SelectedPreciseMeasFreq).ToList();
        }

        public List<string> FastMeasFreq { get; set; }
        public string SelectedFastMeasFreq { get; set; }

        private List<Frequency> _preciseSaveFreqs;
        public List<Frequency> PreciseSaveFreqs
        {
            get { return _preciseSaveFreqs; }
            set
            {
                if (Equals(value, _preciseSaveFreqs)) return;
                _preciseSaveFreqs = value;
                NotifyOfPropertyChange();
            }
        }

        private Frequency _selectedPreciseSaveFreq;
        public Frequency SelectedPreciseSaveFreq
        {
            get { return _selectedPreciseSaveFreq; }
            set
            {
                if (value == _selectedPreciseSaveFreq) return;
                _selectedPreciseSaveFreq = value;
                NotifyOfPropertyChange();
            }
        }

        public List<Frequency> FastSaveFreqs { get; set; }
        public Frequency SelectedFastSaveFreq { get; set; }

        public void InitializeComboboxes(Frequency fastSaveFrequency, Frequency preciseMeasFrequency, Frequency preciseSaveFrequency)
        {
            FastSaveFreqs = Enum.GetValues(typeof(Frequency)).OfType<Frequency>().ToList();
            SelectedFastSaveFreq = fastSaveFrequency;
            PreciseMeasFreqs = FastSaveFreqs.Where(f => f <= Frequency.EveryDay || f == Frequency.DoNot).ToList();
            _selectedPreciseMeasFreq = preciseMeasFrequency;
            PreciseSaveFreqs = FastSaveFreqs.Where(f => f == Frequency.DoNot || f >= SelectedPreciseMeasFreq).ToList();
            SelectedPreciseSaveFreq = preciseSaveFrequency;
            FastMeasFreq = new List<string>() { Resources.SID_Permanently };
            SelectedFastMeasFreq = FastMeasFreq[0];
        }

    }
}
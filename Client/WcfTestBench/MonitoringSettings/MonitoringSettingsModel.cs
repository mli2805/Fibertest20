using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace WcfTestBench.MonitoringSettings
{
    public class MonitoringSettingsModel : PropertyChangedBase
    {
        public List<MonitoringCharonModel> Charons { get; set; } = new List<MonitoringCharonModel>();
        public MonitoringFrequencies Frequencies { get; set; } = new MonitoringFrequencies();
        public bool IsMonitoringOn { get; set; }


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

        public void F()
        {
            foreach (var charon in Charons)
            {
                charon.F();
                charon.PropertyChanged += Charon_PropertyChanged;
            }
            CycleTime = TimeSpan.FromSeconds(Charons.Sum(c => c.CycleTime)).ToString();

            InitializeComboboxes();
        }

        private void InitializeComboboxes()
        {
            FastSaveFreqs = Enum.GetValues(typeof(Frequency)).OfType<Frequency>().ToList();
            SelectedFastSaveFreq = Frequencies.FastSave;
            PreciseMeasFreqs = FastSaveFreqs.Where(f => f <= Frequency.EveryDay || f== Frequency.DoNot).ToList();
            _selectedPreciseMeasFreq = Frequencies.PreciseMeas;
            PreciseSaveFreqs = FastSaveFreqs.Where(f => f == Frequency.DoNot || f >= SelectedPreciseMeasFreq).ToList();
            SelectedPreciseSaveFreq = Frequencies.PreciseSave;
            FastMeasFreq = new List<string>() { "Permanently" };
            SelectedFastMeasFreq = FastMeasFreq[0];
        }

        private void Charon_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CycleTime")
                //                NotifyOfPropertyChange(CycleTime);
                CycleTime = TimeSpan.FromSeconds(Charons.Sum(c => c.CycleTime)).ToString();
        }

        private string _cycleTime;
        private Frequency _selectedPreciseSaveFreq;

        public string CycleTime
        {
            get { return _cycleTime; }
            set
            {
                if (value == _cycleTime) return;
                _cycleTime = value;
                NotifyOfPropertyChange();
            }
        }

    }
}
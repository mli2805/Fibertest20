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


        public List<Frequency> PreciseMeasFreqs { get; set; } = Enum.GetValues(typeof(Frequency)).OfType<Frequency>().ToList();

        private Frequency _selectedPreciseMeasFreq;
        public Frequency SelectedPreciseMeasFreq
        {
            get { return _selectedPreciseMeasFreq; }
            set
            {
                if (_selectedPreciseMeasFreq == value)
                    return;
                _selectedPreciseMeasFreq = value;
                ValidateSaveFrequency();
            }
        }

        private void ValidateSaveFrequency()
        {
            var allPreciseSaveFreqs = Enum.GetValues(typeof(Frequency)).OfType<Frequency>().ToList();
            PreciseSaveFreqs = allPreciseSaveFreqs.Where(f => f == Frequency.DoNotSave || f >= SelectedPreciseMeasFreq).ToList();
            if (SelectedPreciseSaveFreq < SelectedPreciseMeasFreq)
                SelectedPreciseSaveFreq = SelectedPreciseMeasFreq;
        }

        public List<Frequency> PreciseSaveFreqs { get; set; } = Enum.GetValues(typeof(Frequency)).OfType<Frequency>().ToList();
        public Frequency SelectedPreciseSaveFreq { get; set; }

        public List<Frequency> FastSaveFreqs { get; set; } = Enum.GetValues(typeof(Frequency)).OfType<Frequency>().ToList();
        public Frequency SelectedFastSaveFreq { get; set; }


        public void F()
        {
            foreach (var charon in Charons)
            {
                charon.F();
                charon.PropertyChanged += Charon_PropertyChanged;
            }
            CycleTime = TimeSpan.FromSeconds(Charons.Sum(c => c.CycleTime)).ToString();

            SelectedPreciseMeasFreq = Frequencies.PreciseMeas;
            SelectedPreciseSaveFreq = Frequencies.PreciseSave;
            SelectedFastSaveFreq = Frequencies.FastSave;
        }

        private void Charon_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CycleTime")
                //                NotifyOfPropertyChange(CycleTime);
                CycleTime = TimeSpan.FromSeconds(Charons.Sum(c => c.CycleTime)).ToString();
        }

        private string _cycleTime;

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


        //        public string CycleTime => TimeSpan.FromSeconds(Charons.Sum(c => c.CycleTime)).ToString();
    }
}
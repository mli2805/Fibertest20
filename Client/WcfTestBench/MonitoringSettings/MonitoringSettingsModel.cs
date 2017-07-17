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


        public List<MeasFreqs> PreciseMeasFreqs { get; set; } = Enum.GetValues(typeof(MeasFreqs)).OfType<MeasFreqs>().ToList();
        public MeasFreqs SelectedPreciseMeasFreq { get; set; } 

        public List<SaveFreqs> PreciseSaveFreqs { get; set; } = Enum.GetValues(typeof(SaveFreqs)).OfType<SaveFreqs>().ToList();
        public SaveFreqs SelectedPreciseSaveFreq { get; set; }

        public List<SaveFreqs> FastSaveFreqs { get; set; } = Enum.GetValues(typeof(SaveFreqs)).OfType<SaveFreqs>().ToList();
        public SaveFreqs SelectedFastSaveFreq { get; set; }

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
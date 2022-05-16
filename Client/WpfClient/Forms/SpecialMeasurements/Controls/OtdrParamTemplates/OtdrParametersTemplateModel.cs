using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class OtdrParametersTemplateModel : PropertyChangedBase
    {
        private string _selectedUnit;
        private double _backScatteredCoefficient;
        private double _refractiveIndex;
        private OtdrParametersTemplate _selectedOtdrParametersTemplate;

        public List<string> Units { get; set; }

        public string SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                if (value == _selectedUnit) return;
                _selectedUnit = value;
                NotifyOfPropertyChange();
            }
        }

        public double BackScatteredCoefficient
        {
            get => _backScatteredCoefficient;
            set
            {
                if (value.Equals(_backScatteredCoefficient)) return;
                _backScatteredCoefficient = value;
                NotifyOfPropertyChange();
            }
        }

        public double RefractiveIndex
        {
            get => _refractiveIndex;
            set
            {
                if (value.Equals(_refractiveIndex)) return;
                _refractiveIndex = value;
                NotifyOfPropertyChange();
            }
        }

        public List<OtdrParametersTemplate> OtdrParametersTemplates { get; set; } = new List<OtdrParametersTemplate>();

        public OtdrParametersTemplate SelectedOtdrParametersTemplate
        {
            get => _selectedOtdrParametersTemplate;
            set
            {
                if (Equals(value, _selectedOtdrParametersTemplate)) return;
                _selectedOtdrParametersTemplate = value;
                NotifyOfPropertyChange();
            }
        }

        public void Initialize(Rtu rtu)
        {
            OtdrParametersTemplates.Clear();
            var templates = OtdrParamTemplatesProvider.Get(rtu);
            foreach (var template in templates)
            {
                template.PropertyChanged += Template_PropertyChanged;
                OtdrParametersTemplates.Add(template);
            }

            var defaultTemplate = 0;
            OtdrParametersTemplates[defaultTemplate].IsChecked = true;
            SelectedOtdrParametersTemplate = OtdrParametersTemplates[defaultTemplate];
        }

        private void Template_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"IsChecked")
            {
                SelectedOtdrParametersTemplate = OtdrParametersTemplates.First(t => t.IsChecked);
            }
        }
    }
}
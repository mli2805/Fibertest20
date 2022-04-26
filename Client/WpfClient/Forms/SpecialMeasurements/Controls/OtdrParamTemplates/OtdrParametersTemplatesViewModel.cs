using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class OtdrParametersTemplatesViewModel : PropertyChangedBase
    {
        private readonly IniFile _iniFile;
        public OtdrParametersTemplateModel Model { get; set; } = new OtdrParametersTemplateModel();
        private Rtu _rtu;

        public OtdrParametersTemplatesViewModel(IniFile iniFile)
        {
            _iniFile = iniFile;
        }

        public void Initialize(Rtu rtu)
        {
            _rtu = rtu;
            Model.Initialize(rtu);

            var opUnit = _iniFile.Read(IniSection.OtdrParameters, IniKey.OpUnit, 0);
            Model.Units = rtu.AcceptableMeasParams.Units.Keys.ToList();
            Model.SelectedUnit = Model.Units.Count > opUnit ? Model.Units[opUnit] : Model.Units.First();
            var branchOfAcceptableMeasParams = rtu.AcceptableMeasParams.Units[Model.SelectedUnit];
            Model.BackScatteredCoefficient = branchOfAcceptableMeasParams.BackscatteredCoefficient;
            Model.RefractiveIndex = branchOfAcceptableMeasParams.RefractiveIndex;
        }

        public List<MeasParam> GetSelectedParameters()
        {
            var branch = _rtu.AcceptableMeasParams.Units[Model.SelectedUnit];
            var leaf = branch.Distances[Model.SelectedOtdrParametersTemplate.Lmax];

            var result = new List<MeasParam>
            {
                new MeasParam {Param = ServiceFunctionFirstParam.Unit, Value = Model.Units.IndexOf(Model.SelectedUnit)},
                new MeasParam
                    {Param = ServiceFunctionFirstParam.Bc, Value = (int) (Model.BackScatteredCoefficient * 100)},
                new MeasParam {Param = ServiceFunctionFirstParam.Ri, Value = (int) (Model.RefractiveIndex * 100000)},

                 
                new MeasParam
                    { Param = ServiceFunctionFirstParam.Lmax, Value = branch.Distances.Keys.ToList().IndexOf(Model.SelectedOtdrParametersTemplate.Lmax) },
                new MeasParam
                {
                    Param = ServiceFunctionFirstParam.Res, Value = leaf.Resolutions.ToList().IndexOf(Model.SelectedOtdrParametersTemplate.Dl)
                },
                new MeasParam
                {
                    Param = ServiceFunctionFirstParam.Pulse,
                    Value = leaf.PulseDurations.ToList().IndexOf(Model.SelectedOtdrParametersTemplate.Tp)
                },
                new MeasParam {Param = ServiceFunctionFirstParam.IsTime, Value = 1},
                new MeasParam
                {
                    Param = ServiceFunctionFirstParam.Time,
                    Value = leaf.PeriodsToAverage.ToList().IndexOf(Model.SelectedOtdrParametersTemplate.Time)
                },
            };
            return result;
        }

        public VeexMeasOtdrParameters GetVeexSelectedParameters()
        {
            var result = new VeexMeasOtdrParameters()
            {
                measurementType = @"manual",
                fastMeasurement = false,
                highFrequencyResolution = false,
                lasers = new List<Laser>() { new Laser() { laserUnit = Model.SelectedUnit } },
                opticalLineProperties = new OpticalLineProperties()
                {
                    kind = @"point_to_point",
                    lasersProperties = new List<LasersProperty>()
                    {
                        new LasersProperty()
                        {
                            laserUnit = Model.SelectedUnit,
                            backscatterCoefficient = (int)Model.BackScatteredCoefficient,
                            refractiveIndex = Model.RefractiveIndex,
                        }
                    }
                },
                distanceRange = Model.SelectedOtdrParametersTemplate.Lmax,
                resolution = Model.SelectedOtdrParametersTemplate.Dl,
                pulseDuration = Model.SelectedOtdrParametersTemplate.Tp,
                averagingTime = Model.SelectedOtdrParametersTemplate.Time,
            };

            return result;
        }

    }
}

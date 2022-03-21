using System.Collections.Generic;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{

    public class OtdrParametersThroughServerSetterViewModel : Screen
    {
        public bool IsAnswerPositive { get; set; }

        public OtdrParametersViewModel OtdrParametersViewModel { get; set; } = new OtdrParametersViewModel();


        public void Initialize(TreeOfAcceptableMeasParams treeOfAcceptableMeasParams)
        {
            OtdrParametersViewModel.Initialize(treeOfAcceptableMeasParams);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Measurement_parameters;
        }

        public void Measure()
        {
            IsAnswerPositive = true;
            TryClose();
        }

        public List<MeasParam> GetSelectedParameters()
        {
            return OtdrParametersViewModel.GetSelectedParameters();
        }

        public VeexMeasOtdrParameters GetVeexSelectedParameters()
        {
            return OtdrParametersViewModel.GetVeexSelectedParameters();
        }

        public void Close()
        {
            IsAnswerPositive = false;
            TryClose();
        }

    }
}

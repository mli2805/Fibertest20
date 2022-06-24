using System.Collections.Generic;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{

    public class OtdrParametersThroughServerSetterViewModel : Screen
    {
        public bool IsAnswerPositive { get; set; }

        public OtdrParametersViewModel OtdrParametersViewModel { get; set; } = new OtdrParametersViewModel();


        public void Initialize(TreeOfAcceptableMeasParams treeOfAcceptableMeasParams, IniFile iniFile)
        {
            OtdrParametersViewModel.Initialize(treeOfAcceptableMeasParams, iniFile);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Measurement__Client_;
        }

        public void Measure()
        {
            IsAnswerPositive = true;
            TryClose();
        }

        public List<MeasParamByPosition> GetSelectedParameters()
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

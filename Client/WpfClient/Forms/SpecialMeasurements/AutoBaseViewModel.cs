using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class AutoBaseViewModel : Screen
    {
        private readonly IMyLog _logFile;
        private readonly IWindowManager _windowManager;
        public RtuLeaf RtuLeaf { get; set; }
        private Rtu _rtu;
        
        public bool IsAnswerPositive { get; set; }

        public OtdrParametersViewModel OtdrParametersViewModel { get; set; } = new OtdrParametersViewModel();

        public AutoBaseViewModel(IMyLog logFile, IWindowManager windowManager)
        {
            _logFile = logFile;
            _windowManager = windowManager;
        }

        public void Initialize(TraceLeaf traceLeaf, Model readModel)
        {
            var parent = traceLeaf.Parent;
            RtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
            // var otauLeaf = (IPortOwner)parent;
            _rtu = readModel.Rtus.First(r => r.Id == RtuLeaf.Id);

            OtdrParametersViewModel.Initialize(_rtu.AcceptableMeasParams);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Measurement_parameters;
        }
        public void Start()
        {
            if (!RftsTemplateExt.TryLoad(@"c:\temp\template.rft", out RftsParams rftsParams, out Exception exception))
            {
                var mb = new MyMessageBoxViewModel(MessageType.Error,
                    new List<string>() { @"Failed to load template!", exception.Message });
                _windowManager.ShowDialogWithAssignedOwner(mb);
            }
            else
                _logFile.AppendLine($@"RFTS template file loaded successfully! {rftsParams.LevelNumber} levels, {rftsParams.UniversalParamNumber} params");

            IsAnswerPositive = true;
            TryClose();
        }

        public void Close()
        {
            IsAnswerPositive = false;
            TryClose();
        }
    }
}

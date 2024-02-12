using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    public class AutoAnalysisParamsViewModel : PropertyChangedBase, IDataErrorInfo
    {
        private readonly IMyLog _logFile;
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly IWindowManager _windowManager;
        private string _autoLt;
        private string _autoRt;

        public string AutoLt
        {
            get => _autoLt;
            set
            {
                if (value == _autoLt) return;
                _autoLt = value;
                NotifyOfPropertyChange();
            }
        }

        public string AutoRt
        {
            get => _autoRt;
            set
            {
                if (value == _autoRt) return;
                _autoRt = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (value == _isEnabled) return;
                _isEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _searchNewEvents = true;
        public bool SearchNewEvents
        {
            get => _searchNewEvents;
            set
            {
                if (value == _searchNewEvents) return;
                _searchNewEvents = value;
                NotifyOfPropertyChange();
            }
        }

        public AutoAnalysisParamsViewModel(IMyLog logFile, IDispatcherProvider dispatcherProvider, IWindowManager windowManager)
        {
            _logFile = logFile;
            _dispatcherProvider = dispatcherProvider;
            _windowManager = windowManager;
        }

        public bool Initialize()
        {
            return DisplayLtAndRtFromAnyTemplateFile();
        }

        // AutoLT & AutoRT are the same for all templates!
        public void SaveInAllTemplates()
        {
            var clientPath = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory);
            for (int i = 1; i <= 4; i++)
            {
              var templateFileName = clientPath + $@"\ini\RftsParamsDefaultTemplate#{i}.rft";

              RftsParamsParser.TryLoad(templateFileName, out RftsParams result, out Exception _);
              result.UniParams.First(p => p.Name == @"AutoLT").Set(double.Parse(AutoLt));
              result.UniParams.First(p => p.Name == @"AutoRT").Set(double.Parse(AutoRt));
              result.Save(templateFileName);
            }
        }

        public RftsParams LoadFromTemplate(int i)
        {
            var clientPath = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory);
            var templateFileName = clientPath + $@"\ini\RftsParamsDefaultTemplate#{i}.rft";

            if (!RftsParamsParser.TryLoad(templateFileName, out RftsParams result, out Exception exception))
            {

                _dispatcherProvider.GetDispatcher().Invoke(() =>
                {
                    var mb = new MyMessageBoxViewModel(MessageType.Error, new List<string>()
                    {
                        Resources.SID_Failed_to_load_RFTS_parameters_template_from_file_, $@"{templateFileName}", "", exception.Message
                    }, 0);
                    _windowManager.ShowDialogWithAssignedOwner(mb);
                }); // sync, GUI thread

                return null;
            }
            return result;
        }

        public RftsParams GetRftsParams(OtdrDataKnownBlocks sorData, int templateId, Rtu rtu)
        {
            RftsParams rftsParams;
            if (templateId == 0)
            {
                var lmax = sorData.OwtToLenKm(sorData.FixedParameters.AcquisitionRange);
                _logFile.AppendLine($@"Fully automatic measurement: acquisition range = {lmax}", 2);
                var index = AutoBaseParams.GetTemplateIndexByLmaxInSorData(lmax, rtu.Omid);
                _logFile.AppendLine($@"Supposedly used template #{index + 1}", 2);
                rftsParams = LoadFromTemplate(index + 1);
            }
            else
                rftsParams = LoadFromTemplate(templateId);

            if (rftsParams == null) return null;

            rftsParams.UniParams.First(p => p.Name == @"AutoLT").Set(double.Parse(AutoLt));
            rftsParams.UniParams.First(p => p.Name == @"AutoRT").Set(double.Parse(AutoRt));
            return rftsParams;
        }

        private bool DisplayLtAndRtFromAnyTemplateFile()
        {
            // AutoLT & AutoRT are the same for all templates!
            var rftsParams = LoadFromTemplate(1);
            if (rftsParams == null) return false;
            var up = rftsParams.UniParams.First(u => u.Name == @"AutoLT");
            AutoLt = up.ToString();
            AutoRt = rftsParams.UniParams.First(u => u.Name == @"AutoRT").ToString();
            return true;
        }

        public string this[string columnName]
        {
            get
            {
                var errorMessage = string.Empty;
                switch (columnName)
                {
                    case "AutoLt":
                        if (string.IsNullOrEmpty(AutoLt))
                            errorMessage = @"Value required";
                        if (!double.TryParse(AutoLt, out double l) || l < 1 || l > 4)
                            errorMessage = Resources.SID_Invalid_input;
                        break;
                    case "AutoRt":
                        if (string.IsNullOrEmpty(AutoRt))
                            errorMessage = @"Value required";
                        if (!double.TryParse(AutoRt, out double r) || r < -40 || r > -25)
                            errorMessage = Resources.SID_Invalid_input;
                        break;
                }

                return errorMessage;
            }
        }

        public string Error { get; set; }
    }
}

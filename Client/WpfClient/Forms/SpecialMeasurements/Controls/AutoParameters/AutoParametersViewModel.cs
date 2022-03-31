using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class AutoParametersViewModel : PropertyChangedBase, IDataErrorInfo
    {
        private readonly IWindowManager _windowManager;
        private IniFile _iniFile;
        private string _templateFileName;
        public RftsParametersModel Model;
        public string AutoLt { get; set; }
        public string AutoRt { get; set; }

        public AutoParametersViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public void Initialize(IniFile iniFile)
        {
            _iniFile = iniFile;
            var clientPath = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory);
            var defaultFileName = clientPath + @"\ini\RftsParamsDefaultTemplate.rft";
            if (!File.Exists(defaultFileName))
            {
                var rftsDefaults = RftsParamsDefaults.Create();
                rftsDefaults.Save(defaultFileName);
            }

            _templateFileName = iniFile.Read(IniSection.Miscellaneous, IniKey.PathToRftsParamsTemplate, defaultFileName);
            DisplayParametersFromTemplate();
        }

        public void LoadFromTemplate()
        {

        }

        public void SaveInTemplate()
        {

        }

        private void DisplayParametersFromTemplate()
        {
            if (!RftsParamsParser.TryLoad(_templateFileName, out RftsParams result, out Exception exception))
            {
                var mb = new MyMessageBoxViewModel(MessageType.Error, new List<string>() { @"Failed to load RFTS parameters template!", exception.Message });
                _windowManager.ShowDialogWithAssignedOwner(mb);
                return;
            }

            Model = result.ToModel();
            AutoLt = Model.UniParams.First(u => u.Code == @"AutoLT").Value.ToString(CultureInfo.InvariantCulture);
            AutoRt = Model.UniParams.First(u => u.Code == @"AutoRT").Value.ToString(CultureInfo.InvariantCulture);
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
                            errorMessage = Resources.SID_Degrees_is_required;
                        if (!double.TryParse(AutoLt, out double _))
                            errorMessage = Resources.SID_Invalid_input;
                        break;
                    case "AutoRt":
                        if (string.IsNullOrEmpty(AutoRt))
                            errorMessage = Resources.SID_Degrees_is_required;
                        if (!double.TryParse(AutoRt, out double _))
                            errorMessage = Resources.SID_Invalid_input;
                        break;
                }

                return errorMessage;
            }
        }

        public string Error { get; set; }
    }
}

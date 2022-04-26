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
using Microsoft.Win32;

namespace Iit.Fibertest.Client
{
    public class AutoAnalysisParamsViewModel : PropertyChangedBase, IDataErrorInfo
    {
        private readonly IWindowManager _windowManager;
        private IniFile _iniFile;
        private string _defaultFileName;
        private string _templateFileName;
        public RftsParametersModel Model;
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

        public AutoAnalysisParamsViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public bool Initialize(IniFile iniFile)
        {
            _iniFile = iniFile;
            var clientPath = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory);
            _defaultFileName = clientPath + @"\ini\RftsParamsDefaultTemplate.rft";

            _templateFileName = iniFile.Read(IniSection.Miscellaneous, IniKey.PathToRftsParamsTemplate, _defaultFileName);
            if (!DisplayParametersFromTemplateFile())
            {
                return SelectFileName(Path.GetDirectoryName(_defaultFileName), out _templateFileName) && DisplayParametersFromTemplateFile();
            }
            return true;
        }

        public void LoadFromTemplate()
        {
            var initialDir = Path.GetDirectoryName(_templateFileName) ?? AppDomain.CurrentDomain.BaseDirectory;

            if (SelectFileName(initialDir, out _templateFileName))
            {
                _iniFile.Write(IniSection.Miscellaneous, IniKey.PathToRftsParamsTemplate, _templateFileName);
                DisplayParametersFromTemplateFile();
            }
        }

        private bool SelectFileName(string initialDir, out string filename)
        {
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = initialDir,
                DefaultExt = @".rft",
                Filter = @"Template file  |*.rft"
            };
            var result = openFileDialog.ShowDialog() == true;
            filename = result ? openFileDialog.FileName : "";
            return result;
        }

        public void SaveInTemplate()
        {
            RftsParamsParser.TryLoad(_templateFileName, out RftsParams result, out Exception _);
            result.UniParams.First(p => p.Name == @"AutoLT").Set(double.Parse(AutoLt));
            result.UniParams.First(p => p.Name == @"AutoRT").Set(double.Parse(AutoRt));
            result.Save(_templateFileName);
        }

        private bool DisplayParametersFromTemplateFile()
        {
            if (!RftsParamsParser.TryLoad(_templateFileName, out RftsParams result, out Exception exception))
            {
                var mb = new MyMessageBoxViewModel(MessageType.Error, new List<string>()
                {
                    @"Failed to load RFTS parameters template from file:!", $@"{_templateFileName}", exception.Message
                });
                _windowManager.ShowDialogWithAssignedOwner(mb);

                return false;
            }

            Model = result.ToModel();
            AutoLt = Model.UniParams.First(u => u.Code == @"AutoLT").Value.ToString(CultureInfo.InvariantCulture);
            AutoRt = Model.UniParams.First(u => u.Code == @"AutoRT").Value.ToString(CultureInfo.InvariantCulture);
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

﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class AutoAnalysisParamsViewModel : PropertyChangedBase, IDataErrorInfo
    {
        private readonly IWindowManager _windowManager;
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

        public AutoAnalysisParamsViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public bool Initialize()
        {
        
            return DisplayParametersFromTemplateFile();
        }

        // AutoLT & AutoRT are the same for all templates!
        public void SaveInTemplate()
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

        private bool DisplayParametersFromTemplateFile()
        {
            // AutoLT & AutoRT are the same for all templates!
            var clientPath = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory);
            var templateFileName = clientPath + @"\ini\RftsParamsDefaultTemplate#1.rft";

            if (!RftsParamsParser.TryLoad(templateFileName, out RftsParams result, out Exception exception))
            {
                var mb = new MyMessageBoxViewModel(MessageType.Error, new List<string>()
                {
                    @"Failed to load RFTS parameters template from file:!", $@"{templateFileName}", exception.Message
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

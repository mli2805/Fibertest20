using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class AutoParametersViewModel : PropertyChangedBase
    {
        private readonly IWindowManager _windowManager;
        private IniFile _iniFile;
        private string _templateFileName;
        public RftsParametersModel Model;
        public double AutoLt { get; set; }
        public double AutoRt { get; set; }

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
            AutoLt = Model.UniParams.First(u => u.Code == @"AutoLT").Value;
            AutoRt = Model.UniParams.First(u => u.Code == @"AutoRT").Value;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;
using Microsoft.Win32;

namespace Iit.Fibertest.Client
{
    public class RftsParametersViewModel : PropertyChangedBase
    {
        private readonly IWindowManager _windowManager;
        public RftsParametersModel Model { get; set; }

        private string _templateFileName;
        public string TemplateFileName
        {
            get => _templateFileName;
            set
            {
                if (value == _templateFileName) return;
                _templateFileName = value;
                NotifyOfPropertyChange();
            }
        }

        public RftsParametersViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public void Initialize(IniFile iniFile)
        {
            var clientPath = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory);
            var defaultFileName = clientPath + @"\ini\RftsParamsDefaultTemplate.rft";
            if (!File.Exists(defaultFileName))
            {
                var rftsDefaults = RftsParamsDefaults.Create();
                rftsDefaults.Save(defaultFileName);
            }

            TemplateFileName = iniFile.Read(IniSection.Miscellaneous, IniKey.PathToRftsParamsTemplate, defaultFileName);
            if (!RftsParamsParser.TryLoad(TemplateFileName, out RftsParams result, out Exception exception))
            {
                var mb = new MyMessageBoxViewModel(MessageType.Error, new List<string>(){@"Failed to load RFTS parameters template!", exception.Message});
                _windowManager.ShowDialogWithAssignedOwner(mb);
                return;
            }

            Model = result.ToModel();
        }

        public void SelectTemplateFile()
        {
            var initialDir = Path.GetDirectoryName(TemplateFileName) ?? AppDomain.CurrentDomain.BaseDirectory;
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = initialDir,
                DefaultExt = @".rft",
                Filter = @"Template file  |*.rft"
            };
            if (openFileDialog.ShowDialog() == true)
                TemplateFileName = openFileDialog.FileName;
        }

    }
}

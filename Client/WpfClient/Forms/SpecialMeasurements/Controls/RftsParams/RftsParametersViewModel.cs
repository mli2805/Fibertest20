using System;
using System.Collections.Generic;
using System.IO;
using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class RftsParametersViewModel : PropertyChangedBase
    {
        private readonly IWindowManager _windowManager;
        public RftsParametersModel Model { get; set; }

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

            var filename = iniFile.Read(IniSection.Miscellaneous, IniKey.PathToRftsParamsTemplate, defaultFileName);
            if (!RftsParamsParser.TryLoad(filename, out RftsParams result, out Exception exception))
            {
                var mb = new MyMessageBoxViewModel(MessageType.Error, new List<string>(){@"Failed to load RFTS parameters template!", exception.Message});
                _windowManager.ShowDialogWithAssignedOwner(mb);
                return;
            }

            Model = result.ToModel();
        }
    }
}

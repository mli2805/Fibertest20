﻿using System;
using System.Collections.Generic;
using System.IO;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.WpfCommonViews
{
    public class LicenseFromFileDecoder
    {
        private readonly IWindowManager _windowManager;

        public LicenseFromFileDecoder(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public LicenseInFile Decode(string filename)
        {
            var encoded = File.ReadAllBytes(filename);
            try
            {
                return (LicenseInFile)Cryptography.Decode(encoded);
            }
            catch (Exception e)
            {
                var lines = new List<string>()
                {
                    $"{Resources.SID_Invalid_license_file_}:", filename, "", e.Message
                };
                var mb = new MyMessageBoxViewModel(MessageType.Error, lines, 0);
                _windowManager.ShowDialogWithAssignedOwner(mb);
                return null;
            }
        }
    }
}

﻿using System;
using System.IO;
using System.Linq;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public partial class MainMenuViewModel
    {
        public async void ImportRtuFromFolder()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var folder = Path.GetFullPath(Path.Combine(basePath, @"..\temp\"));
            string[] files = Directory.GetFiles(folder, "*.brtu");

            foreach (var filename in files)
            {
                var bytes = File.ReadAllBytes(filename);
                var oneRtuGraphModel = new Model();
                if (!await oneRtuGraphModel.Deserialize(_logFile, bytes)) return;
                _readModel.AddOneRtuToModel(oneRtuGraphModel);
            }

            _currentlyHiddenRtu.Collection.AddRange(_readModel.Rtus.Select(r=>r.Id));
            _currentlyHiddenRtu.IsHideAllPressed = true;
        }

        public async void ExportEvents()
        {
           await _wcfDesktopC2D.ExportEvents();
        }
    }
}

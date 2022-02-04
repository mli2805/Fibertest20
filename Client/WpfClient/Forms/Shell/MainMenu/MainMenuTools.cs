using System;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using Iit.Fibertest.Client.GraphOptimization;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public partial class MainMenuViewModel
    {
        public async Task LaunchCleaningView()
        {
            var vm = _globalScope.Resolve<DbOptimizationViewModel>();
            await vm.Initialize();
            _windowManager.ShowDialogWithAssignedOwner(vm);

            
        }

        public async Task LaunchGraphOptimizationView()
        {
            var vm = _globalScope.Resolve<GraphOptimizationViewModel>();
            await vm.Initialize();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void ImportRtuFromFolder()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var folder = Path.GetFullPath(Path.Combine(basePath, @"..\temp\"));
            string[] files = Directory.GetFiles(folder, @"*.brtu");

            foreach (var filename in files)
            {
                var bytes = File.ReadAllBytes(filename);
                var oneRtuModelFromFile = new Model();
                if (!await oneRtuModelFromFile.Deserialize(_logFile, bytes)) return;

                await _modelFromFileExporter.Apply(oneRtuModelFromFile);
            }
        }

        public async void ExportEvents()
        {
           await _wcfDesktopC2D.ExportEvents();
        }

     
    }
}

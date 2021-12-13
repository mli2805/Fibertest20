using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Autofac;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public partial class MainMenuViewModel
    {
        public void ShowUsersGuide()
        {
            var usersGuide = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                @"..\UserGuide\FIBERTEST20ClientUGru.pdf"));
            if (!File.Exists(usersGuide))
            {
                var mb = new MyMessageBoxViewModel(MessageType.Error,
                    new List<string> { Resources.SID_Cannot_find_file_with_user_s_guide_, "", usersGuide }, 0);
                _windowManager.ShowDialogWithAssignedOwner(mb);
                return;
            }
            Process.Start(usersGuide);
        }

        public void LaunchLicenseView()
        {
            var vm = _globalScope.Resolve<LicenseViewModel>();
            vm.Initialize();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void LaunchAboutView()
        {
            var vm = _globalScope.Resolve<AboutViewModel>();
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }
    }
}
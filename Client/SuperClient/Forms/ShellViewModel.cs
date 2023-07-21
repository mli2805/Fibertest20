﻿using System;
using System.Diagnostics;
using System.Reflection;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;


namespace Iit.Fibertest.SuperClient
{
    public class ShellViewModel : Screen, IShell
    {
        private readonly IMyLog _logFile;
        private readonly IWindowManager _windowManager;
        private readonly SuperClientWcfServiceHost _superClientWcfServiceHost;
        public ServersViewModel ServersViewModel { get; set; }
        public GasketViewModel GasketViewModel { get; set; }


        public ShellViewModel(IMyLog logFile, IWindowManager windowManager, SuperClientWcfServiceHost superClientWcfServiceHost,
            ServersViewModel serversViewModel, GasketViewModel gasketViewModel)
        {
            _logFile = logFile;
            _windowManager = windowManager;
            _superClientWcfServiceHost = superClientWcfServiceHost;
            ServersViewModel = serversViewModel;
            GasketViewModel = gasketViewModel;
        }

        protected override void OnViewLoaded(object view)
        {
            var assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);

            DisplayName = $@"Fibertest 2.0 SuperClient v{info.ProductVersion}";
            _logFile.AssignFile(@"sc.log");
            _logFile.AppendLine(@"Super-Client application started!");

            _superClientWcfServiceHost.StartWcfListener();
        }

        public void ShowClientVersions()
        {
            var lines = Utils.GetClientVersions();
            var vm = new ClientVersionsViewModel();
            vm.Initialize(lines);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public override async void CanClose(Action<bool> callback)
        {
            var question = Resources.SID_Close_application_;
            var vm = new MyMessageBoxViewModel(MessageType.Confirmation, question);
            _windowManager.ShowDialogWithAssignedOwner(vm);

            if (!vm.IsAnswerPositive) return;

            var info = Resources.SID_Wait_please__while_all_clients_will_be_closed_;
            var vm2 = new MyMessageBoxViewModel(MessageType.LongOperation, info);
            _windowManager.ShowWindowWithAssignedOwner(vm2);

            await ServersViewModel.CloseAllClients();
            base.CanClose(callback);
        }
    }
}
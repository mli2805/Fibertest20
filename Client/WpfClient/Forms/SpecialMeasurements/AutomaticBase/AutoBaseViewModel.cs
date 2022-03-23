﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class AutoBaseViewModel : Screen
    {
        private readonly IMyLog _logFile;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceCommonC2D _c2RWcfManager;

        private readonly ClientMeasurementModel _clientMeasurementModel;

        public bool IsOpen { get; set; }

        public OtdrParametersViewModel OtdrParametersViewModel { get; set; } = new OtdrParametersViewModel();
        public MeasurementProgressViewModel MeasurementProgressViewModel { get; set; } =
            new MeasurementProgressViewModel();

        public AutoBaseViewModel(IMyLog logFile, IWindowManager windowManager, IWcfServiceCommonC2D c2RWcfManager,
            CurrentUser currentUser, Model readModel)
        {
            _logFile = logFile;
            _windowManager = windowManager;
            _c2RWcfManager = c2RWcfManager;

            _clientMeasurementModel = new ClientMeasurementModel(currentUser, readModel);
        }

        public void Initialize(TraceLeaf traceLeaf)
        {
            _clientMeasurementModel.Initialize(traceLeaf, false);
            OtdrParametersViewModel.Initialize(_clientMeasurementModel.Rtu.AcceptableMeasParams);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = @"Assign base refs automatically";
        }

        public async void Start()
        {
            IsOpen = true;
            MeasurementProgressViewModel.ControlVisibility = Visibility.Visible;
            MeasurementProgressViewModel.IsCancelButtonEnabled = false;

            var dto = _clientMeasurementModel.PrepareDto(OtdrParametersViewModel.GetSelectedParameters(), OtdrParametersViewModel.GetVeexSelectedParameters());

            MeasurementProgressViewModel.Message = Resources.SID_Sending_command__Wait_please___;

            var startResult = await _c2RWcfManager.DoClientMeasurementAsync(dto);
            if (startResult.ReturnCode != ReturnCode.Ok)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, startResult.ErrorMessage);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }

            MeasurementProgressViewModel.Message = Resources.SID_Measurement__Client__in_progress__Please_wait___;
            MeasurementProgressViewModel.IsCancelButtonEnabled = true;

            if (_clientMeasurementModel.Rtu.RtuMaker == RtuMaker.VeEX)
                await WaitClientMeasurementFromVeex(dto, startResult);
            // if RtuMaker is IIT - result will come through WCF contract
        }

        private async Task WaitClientMeasurementFromVeex(DoClientMeasurementDto dto, ClientMeasurementStartedDto startResult)
        {
            var getDto = new GetClientMeasurementDto()
            {
                RtuId = dto.RtuId,
                VeexMeasurementId = startResult.ErrorMessage, // sorry, if ReturnCode is OK, ErrorMessage contains Id
            };
            while (true)
            {
                await Task.Delay(5000);
                var measResult = await _c2RWcfManager.GetClientMeasurementAsync(getDto);

                if (measResult.ReturnCode != ReturnCode.Ok || measResult.VeexMeasurementStatus == @"failed")
                {
                    var firstLine = measResult.ReturnCode != ReturnCode.Ok
                        ? measResult.ReturnCode.GetLocalizedString()
                        : @"Failed to do Measurement(Client)!";

                    var vm = new MyMessageBoxViewModel(MessageType.Error, new List<string>()
                    {
                        firstLine,
                        "",
                        measResult.ErrorMessage,
                    }, 0);
                    _windowManager.ShowDialogWithAssignedOwner(vm);
                    return;
                }

                if (measResult.ReturnCode == ReturnCode.Ok && measResult.VeexMeasurementStatus == @"finished")
                {
                    ProcessMeasurementResult(measResult.SorBytes);
                    return;
                }
            }
        }

        public void ProcessMeasurementResult(byte[] sorBytes)
        {
            MeasurementProgressViewModel.ControlVisibility = Visibility.Collapsed;
            ShowReflectogram(sorBytes);

            if (!RftsTemplateExt.TryLoad(@"c:\temp\template.rft", out RftsParams rftsParams, out Exception exception))
            {
                var mb = new MyMessageBoxViewModel(MessageType.Error,
                    new List<string>() { @"Failed to load template!", exception.Message });
                _windowManager.ShowDialogWithAssignedOwner(mb);
            }
            else
                _logFile.AppendLine($@"RFTS template file loaded successfully! {rftsParams.LevelNumber} levels, {rftsParams.UniversalParamNumber} params");
        }

        private void ShowReflectogram(byte[] sorBytes)
        {
            _logFile.AppendLine(@"Measurement (Client) result received");
            var clientPath = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory);
            if (!Directory.Exists(clientPath + @"\temp"))
                Directory.CreateDirectory(clientPath + @"\temp");
            var filename = clientPath + $@"\temp\meas-{DateTime.Now:yyyy-MM-dd-hh-mm-ss}.sor";
            SorData.Save(sorBytes, filename);
            var iitPath = FileOperations.GetParentFolder(clientPath);
            Process.Start(iitPath + @"\RftsReflect\Reflect.exe", filename);
        }

        public void Close()
        {
            TryClose();
        }
    }
}

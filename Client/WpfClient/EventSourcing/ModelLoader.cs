﻿using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class ModelLoader
    {
        private readonly IMyLog _logFile;
        private readonly Model _readModel;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly RenderingManager _renderingManager;
        private readonly ZoneEventsOnTreeExecutor _zoneEventsOnTreeExecutor;
        private readonly OpticalEventsDoubleViewModel _opticalEventsDoubleViewModel;
        private readonly NetworkEventsDoubleViewModel _networkEventsDoubleViewModel;
        private readonly BopNetworkEventsDoubleViewModel _bopNetworkEventsDoubleViewModel;

        public ModelLoader(IMyLog logFile, Model readModel, IWcfServiceDesktopC2D c2DWcfManager,
            RenderingManager renderingManager,
            ZoneEventsOnTreeExecutor zoneEventsOnTreeExecutor, 
            OpticalEventsDoubleViewModel opticalEventsDoubleViewModel,
            NetworkEventsDoubleViewModel networkEventsDoubleViewModel,
            BopNetworkEventsDoubleViewModel bopNetworkEventsDoubleViewModel)
        {
            _logFile = logFile;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _renderingManager = renderingManager;
            _zoneEventsOnTreeExecutor = zoneEventsOnTreeExecutor;
            _opticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
            _networkEventsDoubleViewModel = networkEventsDoubleViewModel;
            _bopNetworkEventsDoubleViewModel = bopNetworkEventsDoubleViewModel;
        }

        public async Task<int> DownloadAndApplyModel()
        {
            try
            {
                _logFile.AppendLine(@"Downloading model...");
                var dto = await _c2DWcfManager.GetModelDownloadParams(new GetSnapshotDto());
                _logFile.AppendLine($@"Model size is {dto.Size} in {dto.PortionsCount} portions, last event included {dto.LastIncludedEvent}");

                var bb = new byte[dto.Size];
                var offset = 0;

                for (int i = 0; i < dto.PortionsCount; i++)
                {
                    var portion = await _c2DWcfManager.GetModelPortion(i);
                    portion.CopyTo(bb, offset);
                    offset = offset + portion.Length;
                    _logFile.AppendLine($@"portion {i}  {portion.Length} bytes received");
                }

                await _readModel.Deserialize(_logFile, bb);

                _renderingManager.Initialize();
                await _renderingManager.RenderCurrentZoneOnApplicationStart();
                _zoneEventsOnTreeExecutor.RenderOfModelAfterSnapshot();
                _opticalEventsDoubleViewModel.RenderMeasurementsFromSnapshot();
                _networkEventsDoubleViewModel.RenderNetworkEvents();
                _bopNetworkEventsDoubleViewModel.RenderBopNetworkEvents();

                return dto.LastIncludedEvent;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($@"DownloadModel : {e.Message}");
                return -1;
            }
        }

    }
}
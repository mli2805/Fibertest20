﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.WpfCommonViews
{
    public class RftsEventsViewModel : Screen
    {
        private readonly IWindowManager _windowManager;
        private bool _isNoFiber;
        private string _traceTitle;
        public Visibility NoFiberLabelVisibility => _isNoFiber ? Visibility.Visible : Visibility.Collapsed;
        public Visibility RftsEventsTableVisibility => _isNoFiber ? Visibility.Collapsed : Visibility.Visible;

        public LevelsContent LevelsContent { get; set; } = new LevelsContent();
        public RftsEventsFooterViewModel FooterViewModel { get; set; }

        public int SelectedIndex { get; set; } = -1;

        public RftsEventsViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public void Initialize(OtdrDataKnownBlocks sorData, string traceTitle = "")
        {
            _traceTitle = traceTitle;
            _isNoFiber = sorData.RftsEvents.MonitoringResult == (int) ComparisonReturns.NoFiber;
            if (_isNoFiber) return;

            var rftsEventsBlocks = sorData.GetRftsEventsBlockForEveryLevel().ToList();

            var rftsParameters = sorData.RftsParameters;
            for (int i = 0; i < rftsParameters.LevelsCount; i++)
            {
                var level = rftsParameters.Levels[i];
                if (level.IsEnabled)
                    switch (level.LevelName)
                    {
                        case RftsLevelType.Minor:
                            LevelsContent.IsMinorExists = true;
                            LevelsContent.MinorLevelViewModel = 
                                new RftsEventsOneLevelViewModel(sorData, rftsEventsBlocks.FirstOrDefault(b=>b.LevelName == level.LevelName), level);
                            if (SelectedIndex == -1) SelectedIndex = 0;
                            break;
                        case RftsLevelType.Major:
                            LevelsContent.IsMajorExists = true;
                            LevelsContent.MajorLevelViewModel = 
                                new RftsEventsOneLevelViewModel(sorData, rftsEventsBlocks.FirstOrDefault(b=>b.LevelName == level.LevelName), level);
                            if (SelectedIndex == -1) SelectedIndex = 1;
                            break;
                        case RftsLevelType.Critical:
                            LevelsContent.IsCriticalExists = true;
                            LevelsContent.CriticalLevelViewModel = 
                                new RftsEventsOneLevelViewModel(sorData, rftsEventsBlocks.FirstOrDefault(b=>b.LevelName == level.LevelName), level);
                            if (SelectedIndex == -1) SelectedIndex = 2;
                            break;
                        case RftsLevelType.None:
                            LevelsContent.IsUsersExists = true;
                            LevelsContent.UsersLevelViewModel = 
                                new RftsEventsOneLevelViewModel(sorData, rftsEventsBlocks.FirstOrDefault(b=>b.LevelName == level.LevelName), level);
                            if (SelectedIndex == -1) SelectedIndex = 3;
                            break;
                    }
            }

            FooterViewModel = new RftsEventsFooterViewModel(sorData, LevelsContent);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Rfts_Events + $":  {_traceTitle}";
        }

        public void ExportToPdf()
        {
            var report = RftsEventsPdfProvider.Create(LevelsContent.GetAll(), _traceTitle);
            if (report == null) return;
            try
            {
                var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string filename = Path.Combine(folder, @"RFTS events.pdf");
                report.Save(filename);
                Process.Start(filename);
            }
            catch (Exception e)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, e.Message);
                _windowManager.ShowDialogWithAssignedOwner(vm);
            }
        }

        public void Close()
        {
            TryClose();
        }
    }
}

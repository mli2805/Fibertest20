﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.WpfCommonViews
{
    public class WaitViewModel : Screen
    {
        public bool IsOpen { get; set; }
        public List<MyMessageBoxLineModel> Lines { get; set; }

        private Visibility _progressBarVisibility = Visibility.Visible;
        public Visibility ProgressBarVisibility
        {
            get { return _progressBarVisibility; }
            set
            {
                if (value == _progressBarVisibility) return;
                _progressBarVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private string _progressText;
        public string ProgressText
        {
            get { return _progressText; }
            set
            {
                if (value == _progressText) return;
                _progressText = value;
                NotifyOfPropertyChange();
            }
        }

        public void Initialize(LongOperation longOperation)
        {
            var strs = longOperation.ToLines();
            Lines = strs.Select(s => new MyMessageBoxLineModel() { Line = s }).ToList();
            Lines[0].FontWeight = FontWeights.Bold;
        }


        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Long_operation__please_wait;
            IsOpen = true;
        }

        public void UpdateOptimizationProgress(DbOptimizationProgressDto dto)
        {
            ProgressText = StageToMessage(dto);
        }

        private string StageToMessage(DbOptimizationProgressDto dto)
        {
            switch (dto.Stage)
            {
                case DbOptimizationStage.Starting:
                    return Resources.SID_Db_optimization_started;
                case DbOptimizationStage.SorsRemoving:
                    return string.Format(Resources.SID_Measurements_chosen_for_deletion__0___Removing___, dto.MeasurementsChosenForDeletion);
                case DbOptimizationStage.TableCompressing:
                    return string.Format(Resources.SID_Sorfiles_table_compressing__0_0_0__, dto.Copied);
                case DbOptimizationStage.ModelAdjusting:
                    return Resources.SID_Model_adjusting___;
                case DbOptimizationStage.Done:
                    ProgressBarVisibility = Visibility.Collapsed;
                    return string.Format(Resources.SID_Before__0___After__1___Released__2_,
                        dto.OldSizeGb.ToString("0.000"), dto.NewSizeGb.ToString("0.000"), (dto.OldSizeGb - dto.NewSizeGb).ToString("0.000"));
                default: return dto.Stage.ToString();
            }
        }

        public override void CanClose(Action<bool> callback)
        {
            IsOpen = false;
            base.CanClose(callback);
        }
    }
}

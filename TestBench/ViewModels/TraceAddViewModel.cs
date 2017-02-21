﻿using System;
using System.ComponentModel;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench.Properties;

namespace Iit.Fibertest.TestBench
{
    public class TraceAddViewModel : Screen, IDataErrorInfo
    {
        private string _title;
        private string _comment;

        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title) return;
                _title = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsButtonSaveEnabled));
            }
        }

        public string Comment
        {
            get { return _comment; }
            set
            {
                if (value == _comment) return;
                _comment = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsButtonSaveEnabled => !string.IsNullOrEmpty(_title);

        public bool IsUserClickedSave { get; set; }
        public bool IsClosed { get; set; }


        public TraceAddViewModel()
        {
            IsClosed = false;
        }

        public void Save()
        {
            IsUserClickedSave = true;
            CloseView();
        }

        public void Cancel()
        {
            CloseView();
        }

        private void CloseView()
        {
            IsClosed = true;
            TryClose();
        }

        public string this[string columnName]
        {
            get
            {
                String errorMessage = String.Empty;
                switch (columnName)
                {
                    case "Title":
                        if (String.IsNullOrEmpty(Title))
                        {
                            errorMessage = Resources.SID_Title_is_required;
                        }
                        break;
                }
                return errorMessage;
            }
        }

        public string Error { get; } = null;
    }
}

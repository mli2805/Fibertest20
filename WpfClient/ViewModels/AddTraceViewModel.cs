﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public class AddTraceViewModel : Screen, IDataErrorInfo
    {
        private readonly ReadModel _readModel;
        private readonly Aggregate _aggregate;
        private readonly List<Guid> _nodes;
        private List<Guid> _equipments = new List<Guid>();
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

        public bool IsClosed { get; set; }


        public AddTraceViewModel(ReadModel readModel, Aggregate aggregate, List<Guid> nodes)
        {
            _readModel = readModel;
            _aggregate = aggregate;
            _nodes = nodes;
            CollectEquipmentForTrace();

            IsClosed = false;
        }

        //TODO: требуется реальное наполнение c запросами пользователю и проверкой, что последний узел содержит оборудование
        private void CollectEquipmentForTrace()
        {
            foreach (var nodeId in _nodes)
            {
                var equipment = _readModel.Equipments.FirstOrDefault(e=>e.NodeId == nodeId);
                _equipments.Add(equipment?.Id ?? Guid.Empty);
            }
        }

        public void Save()
        {
            _aggregate.When(new AddTrace()
            {
                Id = new Guid(),
                Title = _title,
                Nodes = _nodes,
                Equipments = _equipments,
                Comment = _comment
            });

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
                            errorMessage = "Title is required";
                        }
                        break;
                }
                return errorMessage;
            }
        }

        public string Error { get; }
    }
}

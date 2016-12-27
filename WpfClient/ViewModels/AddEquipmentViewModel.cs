using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public struct RadioButton
    {
        public string Title { get; set; }
        public bool IsSelected { get; set; }
    }
    public class AddEquipmentViewModel : Screen
    {
        private readonly Guid _nodeId;
        private readonly ReadModel _readModel;
        private readonly Aggregate _aggregate;

        public RadioButton CableReserve { get; } = new RadioButton() {Title = "CableReserve", IsSelected = false};
        public RadioButton Sleeve { get; } = new RadioButton() {Title = "Sleeve", IsSelected = true};
        public RadioButton Cross { get; } = new RadioButton() {Title = "Cross", IsSelected = false};
        public RadioButton Terminal { get; } = new RadioButton() {Title = "Terminal", IsSelected = false};
        public RadioButton Other { get; } = new RadioButton() {Title = "Other", IsSelected = false};

        public bool IsClosed { get; set; }
        public AddEquipmentViewModel(Guid nodeId, ReadModel readModel, Aggregate aggregate)
        {
            _nodeId = nodeId;
            _readModel = readModel;
            _aggregate = aggregate;

            IsClosed = false;
        }

        public void Save()
        {
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

    }
}

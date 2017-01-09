using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.Graph.Events;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public class UpdateFiberViewModel : Screen, IDataErrorInfo
    {
        private ReadModel _readModel;
        private readonly Aggregate _aggregate;
        private Fiber _fiber;
        private string _userInputedLength;

        public double GpsLength { get; set; }

        public double OpticalLength { get; set; }

        public string UserInputedLength
        {
            get { return _userInputedLength; }
            set
            {
                if (value.Equals(_userInputedLength)) return;
                _userInputedLength = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsButtonSaveEnabled = true;

        private double GetGpsLength(Fiber fiber)
        {
            var n1 = _readModel.Nodes.Single(n => n.Id == fiber.Node1);
            var n2 = _readModel.Nodes.Single(n => n.Id == fiber.Node2);
            return GetGpsLength(n1, n2);
        }

        private double GetGpsLength(Node n1, Node n2)
        {
            return 1.0;
        }
        public UpdateFiberViewModel(Guid fiberId, ReadModel readModel, Aggregate aggregate)
        {
            _readModel = readModel;
            _aggregate = aggregate;

            _fiber = _readModel.Fibers.Single(f => f.Id == fiberId);
            GpsLength = GetGpsLength(_fiber);
            OpticalLength = _fiber.OpticalLength;
            UserInputedLength = _fiber.UserInputedLength.ToString();
        }

        public void Save()
        {
            var cmd = new UpdateFiber() {Id = _fiber.Id, UserInputedLength = double.Parse(_userInputedLength)};
            _aggregate.When(cmd);
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }

        public string this[string columnName]
        {
            get
            {
                var errorMessage = string.Empty;
                switch (columnName)
                {
                    case "UserInputedLength":
                        double length;
                        if (!double.TryParse(_userInputedLength, out length))
                        {
                            errorMessage = "Length should be a number";
                        }
                        IsButtonSaveEnabled = errorMessage == string.Empty;
                        break;
                }
                return errorMessage;
            }
        }

        public string Error { get; }
    }
}

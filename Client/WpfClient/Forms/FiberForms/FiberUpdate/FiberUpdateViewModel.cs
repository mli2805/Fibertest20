using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class FiberUpdateViewModel : Screen, IDataErrorInfo
    {
        private readonly Model _readModel;
        private readonly GraphGpsCalculator _graphGpsCalculator;
        private Fiber _fiber;
        private string _userInputedLength;

        public string GpsLength { get; set; }

        public double OpticalLength { get; set; }

        public string NodeAtitle { get; set; }
        public string NodeBtitle { get; set; }

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

        public bool IsButtonSaveEnabled { get; set; }
        public UpdateFiber Command { get; set; }

       
        public FiberUpdateViewModel(Model readModel, GraphGpsCalculator graphGpsCalculator)
        {
            _readModel = readModel;
            _graphGpsCalculator = graphGpsCalculator;
        }

        public void Initialize(Guid fiberId)
        {
            _fiber = _readModel.Fibers.Single(f => f.FiberId == fiberId);
            NodeAtitle = _readModel.Nodes.Single(n => n.NodeId == _fiber.NodeId1).Title;
            NodeBtitle = _readModel.Nodes.Single(n => n.NodeId == _fiber.NodeId2).Title;

            GpsLength = $@"{_graphGpsCalculator.GetFiberFullGpsDistance(fiberId):#,##0}";
//            OpticalLength = _fiber.OpticalLength; // потом из базовых брать

            // refactor to use ReadModel instead of Graph
            UserInputedLength = _fiber.UserInputedLength.ToString(CultureInfo.InvariantCulture);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Section;
        }

        public void Save()
        {

            Command = new UpdateFiber {Id = _fiber.FiberId, UserInputedLength = int.Parse(_userInputedLength)};
            TryClose();
        }

        public void Cancel()
        {
            Command = null;
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
                        if (!double.TryParse(_userInputedLength, out _))
                        {
                            errorMessage = Resources.SID_Length_should_be_a_number;
                        }
                        IsButtonSaveEnabled = errorMessage == string.Empty;
                        break;
                }
                return errorMessage;
            }
        }

        public string Error { get; } = null;
    }
}

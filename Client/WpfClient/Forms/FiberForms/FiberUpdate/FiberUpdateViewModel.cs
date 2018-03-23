using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class FiberUpdateViewModel : Screen, IDataErrorInfo
    {
        private readonly ReadModel _readModel;
        private Fiber _fiber;
        private string _userInputedLength;

        public string NodeAtitle { get; set; }
        public string NodeBtitle { get; set; }

        public string GpsLength { get; set; }

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
        public UpdateFiber Command { get; set; }

       
        public FiberUpdateViewModel(ReadModel readModel)
        {
            _readModel = readModel;
        }

        public void Initialize(Guid fiberId)
        {
            _fiber = _readModel.Fibers.Single(f => f.Id == fiberId);

            var n1 = GetNotAdjustmentPointEdgeOfFiber(_fiber, _readModel.Nodes.First(n=>n.Id == _fiber.Node1));
            var n2 = GetNotAdjustmentPointEdgeOfFiber(_fiber, _readModel.Nodes.First(n=>n.Id == _fiber.Node2));

            NodeAtitle = n1.Title;
            NodeBtitle = n2.Title;
            GpsLength = $@"{GpsCalculator.GetDistanceBetweenPointLatLng(n1.Position, n2.Position):#,##0}";
//            OpticalLength = _fiber.OpticalLength; // потом из базовых брать

            // refactor to use ReadModel instead of Graph
            UserInputedLength = _fiber.UserInputedLength.ToString(CultureInfo.InvariantCulture);
        }

        private Node GetNotAdjustmentPointEdgeOfFiber(Fiber fX, Node n1)
        {
            while (n1.TypeOfLastAddedEquipment == EquipmentType.AdjustmentPoint)
            {
                fX = _readModel.GetAnotherFiberOfAdjustmentPoint(n1, fX.Id);
                n1 = _readModel.Nodes.First(n=>n.Id == (fX.Node1 == n1.Id ? fX.Node2 : fX.Node1));
            }
            return n1;
        }

    

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Section;
        }

        public void Save()
        {

            Command = new UpdateFiber {Id = _fiber.Id, UserInputedLength = int.Parse(_userInputedLength)};
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

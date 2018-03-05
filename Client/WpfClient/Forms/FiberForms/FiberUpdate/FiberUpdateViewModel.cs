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
        private readonly GraphReadModel _graphReadModel;
        private FiberVm _fiberVm;
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

       
        public FiberUpdateViewModel(GraphReadModel graphReadModel)
        {
            _graphReadModel = graphReadModel;
        }

        public void Initialize(Guid fiberId)
        {
            _fiberVm = _graphReadModel.Fibers.Single(f => f.Id == fiberId);

            var n1 = GetNotAdjustmentPointEdgeOfFiber(_fiberVm, _fiberVm.Node1);
            var n2 = GetNotAdjustmentPointEdgeOfFiber(_fiberVm, _fiberVm.Node2);

            NodeAtitle = n1.Title;
            NodeBtitle = n2.Title;
            GpsLength = $@"{GpsCalculator.GetDistanceBetweenPointLatLng(n1.Position, n2.Position):#,##0}";
//            OpticalLength = _fiber.OpticalLength; // потом из базовых брать
            UserInputedLength = _fiberVm.UserInputedLength.ToString(CultureInfo.InvariantCulture);
        }

        public NodeVm GetNotAdjustmentPointEdgeOfFiber(FiberVm fX, NodeVm n1)
        {
            while (n1.Type == EquipmentType.AdjustmentPoint)
            {
                fX = _graphReadModel.GetOtherFiberOfAdjustmentPoint(n1, fX.Id);
                n1 = fX.Node1.Id == n1.Id ? fX.Node2 : fX.Node1;
            }
            return n1;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Section;
        }

        public void Save()
        {

            Command = new UpdateFiber {Id = _fiberVm.Id, UserInputedLength = int.Parse(_userInputedLength)};
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

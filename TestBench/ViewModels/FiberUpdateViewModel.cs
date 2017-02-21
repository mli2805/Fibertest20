using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class FiberUpdateViewModel : Screen, IDataErrorInfo
    {
        private readonly GraphVm _graphVm;
        private FiberVm _fiber;
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

        /// <summary>
        /// вычисляет расстояние между двумя точками с координатами в градусах
        /// </summary>
        /// <param name="n1"></param>
        /// <param name="n2"></param>
        /// <returns>расстояние в метрах</returns>
        private double GetGpsLength(NodeVm n1, NodeVm n2)
        {
            const double latitude1M = 8.981e-6;

            // растояние по вертикали не зависит от широты
            const double latitude1Gr = 1 / latitude1M ; // это метров в градусе
                                                              // он же расстояние и по горизонтали , если мерять на экваторе
                                                              // иначе домножать на косинус широты на которой меряется
            double lat1 = n1.Position.Lat;
            double lat2 = n2.Position.Lat;
            double lon1 = n1.Position.Lng;
            double lon2 = n2.Position.Lng;

            // расстояние между двуми точками находящимися на одной долготе
            double d1 = (lat2 - lat1) * latitude1Gr;
            // расстояние между двуми точками находящимися на одной широте надо домножать на косинус широты
            double coslat = Math.Cos((lat1 + lat2) / 2);
            double d2 = (lon2 - lon1) * latitude1Gr * coslat;
            // расстояние между двуми точками по теореме пифагора
            double l = Math.Sqrt(d1 * d1 + d2 * d2);

            return l;
        }
        public FiberUpdateViewModel(Guid fiberId, GraphVm graphVm)
        {
            _graphVm = graphVm;
            _fiber = graphVm.Fibers.Single(f => f.Id == fiberId);
            Initialize();
        }

        private void Initialize()
        {
            var n1 = _graphVm.Nodes.Single(n => n.Id == _fiber.Node1.Id);
            var n2 = _graphVm.Nodes.Single(n => n.Id == _fiber.Node2.Id);
            NodeAtitle = n1.Title;
            NodeBtitle = n2.Title;
            GpsLength = $@"{GetGpsLength(n1, n2):#,##0}";
//            OpticalLength = _fiber.OpticalLength; // потом из базовых брать
            UserInputedLength = _fiber.UserInputedLength.ToString(CultureInfo.InvariantCulture);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Fiber;
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
                        double length;
                        if (!double.TryParse(_userInputedLength, out length))
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

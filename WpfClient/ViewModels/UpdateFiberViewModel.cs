using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

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

        /// <summary>
        /// вычисляет расстояние между двумя точками с координатами в градусах
        /// </summary>
        /// <param name="n1"></param>
        /// <param name="n2"></param>
        /// <returns>расстояние в км</returns>
        private double GetGpsLength(Node n1, Node n2)
        {
            const double latitude1M = 8.981e-6;

            // растояние по вертикали не зависит от широты
            const double latitude1Gr = 1 / latitude1M / 1000; // это км в градусе
                                                              // он же расстояние и по горизонтали , если мерять на экваторе
                                                              // иначе домножать на косинус широты на которой меряется
            double lat1 = n1.Latitude;
            double lat2 = n2.Latitude;
            double lon1 = n1.Longitude;
            double lon2 = n2.Longitude;

            // расстояние между двуми точками находящимися на одной долготе
            double d1 = (lat2 - lat1) * latitude1Gr;
            // расстояние между двуми точками находящимися на одной широте надо домножать на косинус широты
            double coslat = Math.Cos((lat1 + lat2) / 2);
            double d2 = (lon2 - lon1) * latitude1Gr * coslat;
            // расстояние между двуми точками по теореме пифагора
            double l = Math.Sqrt(d1 * d1 + d2 * d2);

            return l;
        }
        public UpdateFiberViewModel(Guid fiberId, ReadModel readModel, Aggregate aggregate)
        {
            _readModel = readModel;
            _aggregate = aggregate;

            _fiber = _readModel.Fibers.Single(f => f.Id == fiberId);
            GpsLength = GetGpsLength(_fiber);
            OpticalLength = _fiber.OpticalLength;
            UserInputedLength = _fiber.UserInputedLength.ToString(CultureInfo.InvariantCulture);
        }

        public void Save()
        {
            var cmd = new UpdateFiber {Id = _fiber.Id, UserInputedLength = double.Parse(_userInputedLength)};
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

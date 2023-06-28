using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class SystemState : PropertyChangedBase
    {
        private bool _hasActualOpticalProblems;
        private bool _hasActualNetworkProblems;
        private bool _hasActualBopNetworkProblems;
        private bool _hasActualRtuAccidents;
        private bool _hasAnyActualProblem;

        public bool HasActualOpticalProblems
        {
            get => _hasActualOpticalProblems;
            set
            {
                if (value == _hasActualOpticalProblems) return;
                _hasActualOpticalProblems = value;
                HasAnyActualProblem = _hasActualOpticalProblems || _hasActualNetworkProblems ||
                                      _hasActualBopNetworkProblems || _hasActualRtuAccidents;
            }
        }

        public bool HasActualNetworkProblems
        {
            get => _hasActualNetworkProblems;
            set
            {
                if (value == _hasActualNetworkProblems) return;
                _hasActualNetworkProblems = value;
                HasAnyActualProblem = _hasActualOpticalProblems || _hasActualNetworkProblems ||
                                      _hasActualBopNetworkProblems || _hasActualRtuAccidents;
            }
        }

        public bool HasActualBopNetworkProblems
        {
            get => _hasActualBopNetworkProblems;
            set
            {
                if (value == _hasActualBopNetworkProblems) return;
                _hasActualBopNetworkProblems = value;
                HasAnyActualProblem = _hasActualOpticalProblems || _hasActualNetworkProblems ||
                                      _hasActualBopNetworkProblems || _hasActualRtuAccidents;

            }
        }

        public bool HasActualRtuAccidents
        {
            get => _hasActualRtuAccidents;
            set
            {
                if (value == _hasActualRtuAccidents) return;
                _hasActualRtuAccidents = value;
                HasAnyActualProblem = _hasActualOpticalProblems || _hasActualNetworkProblems ||
                                      _hasActualBopNetworkProblems || _hasActualRtuAccidents;
            }
        }

        public bool HasAnyActualProblem
        {
            get => _hasAnyActualProblem;
            set
            {
                if (value == _hasAnyActualProblem) return;
                _hasAnyActualProblem = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
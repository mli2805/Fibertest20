using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class CleaningViewModel : Screen
    {
        private readonly IMyLog _logFile;
        private readonly Model _model;

        public CleaningViewModel(IMyLog logFile, Model model)
        {
            _logFile = logFile;
            _model = model;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = @"Чистилище";
        }

        public async void Snapshot()
        {
            var unused = await _model.Serialize(_logFile);

        }
    }
}

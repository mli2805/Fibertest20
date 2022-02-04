using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client.GraphOptimization
{
    public class GraphOptimizationViewModel : Screen
    {
        private readonly Model _readModel;

        public GraphOptimizationViewModel(Model readModel)
        {
            _readModel = readModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Graph_of_traces_optimization;
        }

        public async Task Remove()
        {
            await Task.Delay(0);
        }


        public void Cancel()
        {
            TryClose();
        }
    }
}

using System.ServiceModel;
using System.Threading.Tasks;
using ClientWcfServiceInterface;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ClientWcfService : IClientWcfService
    {
        public event OnMessageReceived MessageReceived;
        public delegate void OnMessageReceived(object e);

        private readonly TreeOfRtuModel _treeOfRtuModel;
        private readonly RtuStateViewsManager _rtuStateViewsManager;

        public ClientWcfService(TreeOfRtuModel treeOfRtuModel, RtuStateViewsManager rtuStateViewsManager)
        {
            _treeOfRtuModel = treeOfRtuModel;
            _rtuStateViewsManager = rtuStateViewsManager;
        }

        public async Task<int> NotifyUsersRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto dto)
        {
            _rtuStateViewsManager.NotifyUsersRtuCurrentMonitoringStep(dto);
            return 0;
        }

        public async Task<int> ProcessMonitoringResult(MonitoringResultDto dto)
        {
            MessageReceived?.Invoke(dto);
            return 0;
        }

        public async Task<int> NotifyAboutRtuChangedAvailability(ListOfRtuWithChangedAvailabilityDto dto)
        {
            _treeOfRtuModel.Apply(dto);
            foreach (var rtuWithChannelChanges in dto.List)
            {
                var rtuLeaf = (RtuLeaf)_treeOfRtuModel.Tree.GetById(rtuWithChannelChanges.RtuId);
                _rtuStateViewsManager.NotifyUserRtuAvailabilityChanged(rtuLeaf);
            }
            return 0;
        }

    }
}

using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.ClientWcfServiceInterface;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ClientWcfService : IClientWcfService
    {
        private readonly TreeOfRtuModel _treeOfRtuModel;
        private readonly TraceStateViewsManager _traceStateViewsManager;
        private readonly TraceStatisticsViewsManager _traceStatisticsViewsManager;
        private readonly RtuStateViewsManager _rtuStateViewsManager;
        private readonly OpticalEventsDoubleViewModel _opticalEventsDoubleViewModel;
        private readonly NetworkEventsDoubleViewModel _networkEventsDoubleViewModel;
        private readonly ClientMeasurementViewModel _clientMeasurementViewModel;

        public ClientWcfService(TreeOfRtuModel treeOfRtuModel,
            TraceStateViewsManager traceStateViewsManager, TraceStatisticsViewsManager traceStatisticsViewsManager,
            OpticalEventsDoubleViewModel opticalEventsDoubleViewModel,
            RtuStateViewsManager rtuStateViewsManager, NetworkEventsDoubleViewModel networkEventsDoubleViewModel,
            ClientMeasurementViewModel clientMeasurementViewModel)
        {
            _treeOfRtuModel = treeOfRtuModel;
            _traceStateViewsManager = traceStateViewsManager;
            _traceStatisticsViewsManager = traceStatisticsViewsManager;
            _rtuStateViewsManager = rtuStateViewsManager;
            _opticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
            _networkEventsDoubleViewModel = networkEventsDoubleViewModel;
            _clientMeasurementViewModel = clientMeasurementViewModel;
        }

        public Task<int> NotifyUsersRtuCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            _rtuStateViewsManager.NotifyUserRtuCurrentMonitoringStep(dto);
            return Task.FromResult(0);
        }

        public Task<int> NotifyAboutMonitoringResult(MeasurementWithSor measurementWithSor)
        {
            _treeOfRtuModel.Apply(measurementWithSor.Measurement);
            _traceStateViewsManager.NotifyAboutMonitoringResult(measurementWithSor);
            _traceStatisticsViewsManager.AddNewMeasurement(measurementWithSor.Measurement);
            _rtuStateViewsManager.NotifyUserMonitoringResult(measurementWithSor.Measurement);

            if (measurementWithSor.Measurement.EventStatus > EventStatus.JustMeasurementNotAnEvent)
            {
                _opticalEventsDoubleViewModel.Apply(measurementWithSor.Measurement);
                _opticalEventsDoubleViewModel.ApplyToTableAll(measurementWithSor.Measurement);
            }

            return Task.FromResult(0);
        }

        public Task<int> NotifyAboutNetworkEvents(List<NetworkEvent> dto)
        {
            foreach (var networkEvent in dto)
            {
                var rtuLeaf = (RtuLeaf)_treeOfRtuModel.Tree.GetById(networkEvent.RtuId);
                if (rtuLeaf == null)
                    continue;

                // should be evaluate just now, before NetworkEvent applied to Tree
                var changes = IsStateWorseOrBetterThanBefore(rtuLeaf, networkEvent);
                if (changes == RtuPartStateChanges.NoChanges) // strange case, but...
                    return Task.FromResult(0); 

                _treeOfRtuModel.Apply(networkEvent);
                _rtuStateViewsManager.NotifyUserRtuAvailabilityChanged(rtuLeaf, changes);

                _networkEventsDoubleViewModel.Apply(networkEvent);
                _networkEventsDoubleViewModel.ApplyToTableAll(networkEvent);
            }
            return Task.FromResult(0);
        }

        public Task<int> NotifyAboutMeasurementClientDone(ClientMeasurementDoneDto dto)
        {
            if (_clientMeasurementViewModel.IsOpen)
                _clientMeasurementViewModel.ShowReflectogram(dto.SorBytes);
            return Task.FromResult(0);
        }

        private RtuPartStateChanges IsStateWorseOrBetterThanBefore(RtuLeaf rtuLeaf, NetworkEvent networkEvent)
        {
            List<WorseOrBetter> parts = new List<WorseOrBetter> {
                rtuLeaf.MainChannelState.BecomeBetterOrWorse(networkEvent.MainChannelState),
                rtuLeaf.ReserveChannelState.BecomeBetterOrWorse(networkEvent.ReserveChannelState),
            };

            if (parts.Contains(WorseOrBetter.Worse) && parts.Contains(WorseOrBetter.Better))
                return RtuPartStateChanges.DifferentPartsHaveDifferentChanges;
            if (parts.Contains(WorseOrBetter.Worse))
                return RtuPartStateChanges.OnlyWorse;
            if (parts.Contains(WorseOrBetter.Better))
                return RtuPartStateChanges.OnlyBetter;
            return RtuPartStateChanges.NoChanges;
        }
    }

}

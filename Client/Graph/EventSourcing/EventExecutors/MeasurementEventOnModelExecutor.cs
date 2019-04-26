using System.Linq;
using AutoMapper;

namespace Iit.Fibertest.Graph
{
    public static class MeasurementEventOnModelExecutor
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        public static string AddMeasurement(this Model model, MeasurementAdded e)
        {
            model.Measurements.Add(Mapper.Map<Measurement>(e));
            model.ShowMonitoringResult(e);
            return null;
        }

        public static string UpdateMeasurement(this Model model, MeasurementUpdated e)
        {
            var destination = model.Measurements.First(f => f.SorFileId == e.SorFileId);
            Mapper.Map(e, destination);
            return null;
        }

        public static string AddNetworkEvent(this Model model, NetworkEventAdded e)
        {
            model.NetworkEvents.Add(Mapper.Map<NetworkEvent>(e));
            var rtu = model.Rtus.First(r => r.Id == e.RtuId);
            rtu.MainChannelState = e.OnMainChannel.ChangeChannelState(rtu.MainChannelState);
            rtu.ReserveChannelState = e.OnReserveChannel.ChangeChannelState(rtu.ReserveChannelState);
            return null;
        }


        public static string AddBopNetworkEvent(this Model model, BopNetworkEventAdded e)
        {
            model.BopNetworkEvents.Add(Mapper.Map<BopNetworkEvent>(e));
            // if BOP has 2 OTAU - both should change their state
            foreach (var otau in model.Otaus.Where(o => o.NetAddress.Ip4Address == e.OtauIp))
            {
                otau.IsOk = e.IsOk;
            }
            return null;
        }

        public static string RemoveEventsAndSors(this Model model, EventsAndSorsRemoved e)
        {
            var measurementsForDeletion = model.GetMeasurementsForDeletion(e.UpTo, e.IsMeasurementsNotEvents, e.IsOpticalEvents);
            model.Measurements.RemoveAll(m => measurementsForDeletion.Contains(m));

            if (e.IsNetworkEvents)
            {
                var networkEventsForDeletion = model.GetNetworkEventsForDeletion(e.UpTo);
                model.NetworkEvents.RemoveAll(n => networkEventsForDeletion.Contains(n));
                var bopNetworkEventsForDeletion = model.GetBopNetworkEventsForDeletion(e.UpTo);
                model.BopNetworkEvents.RemoveAll(n => bopNetworkEventsForDeletion.Contains(n));
            }

            return null;
        }
    }


}
using System.Linq;
using AutoMapper;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public static class MeasurementEventOnModelExecutor
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        public static string AddMeasurement(this Model model, MeasurementAdded e)
        {
            var meas = Mapper.Map<Measurement>(e);
            model.Measurements.Add(meas);

            var sameTraceActiveMeas = model.ActiveMeasurements.FirstOrDefault(m => m.TraceId == meas.TraceId);
            if (sameTraceActiveMeas != null)
                model.ActiveMeasurements.Remove(sameTraceActiveMeas);
            if (meas.TraceState != FiberState.Ok)
                model.ActiveMeasurements.Add(meas);

            model.ShowMonitoringResult(e);
            return null;
        }

        public static string UpdateMeasurement(this Model model, MeasurementUpdated e)
        {
            var destination = model.Measurements.First(f => f.SorFileId == e.SorFileId);
            if (destination.EventStatus == e.EventStatus)
                destination.Comment = e.Comment;
            else
                Mapper.Map(e, destination);

            var measInActiveList = model.ActiveMeasurements.FirstOrDefault(f => f.SorFileId == e.SorFileId);
            if (measInActiveList != null)
                if (measInActiveList.EventStatus == e.EventStatus)
                    measInActiveList.Comment = e.Comment;
                else
                    Mapper.Map(e, measInActiveList);

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
            foreach (var otau in model.Otaus.Where(o => o.OtauAddress.Ip4Address == e.OtauIp))
            {
                otau.IsOk = e.IsOk;
                var rtu = model.Rtus.FirstOrDefault(r => r.Id == e.RtuId);
                rtu?.SetOtauState(otau.Id, e.IsOk);
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
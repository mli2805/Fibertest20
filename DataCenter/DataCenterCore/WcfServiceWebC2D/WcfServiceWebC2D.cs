using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.D2RtuVeexLibrary;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Newtonsoft.Json;

namespace Iit.Fibertest.DataCenterCore
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public partial class WcfServiceWebC2D : IWcfServiceWebC2D
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        private readonly IMyLog _logFile;
        private readonly Model _writeModel;
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly ClientsCollection _clientsCollection;
        private readonly SorFileRepository _sorFileRepository;
        private readonly D2RtuVeexLayer1 _d2RtuVeexLayer1;
        private readonly IntermediateLayer _intermediateLayer;
        private readonly D2RtuVeexLayer3 _d2RtuVeexLayer3;
        private readonly LandmarksBaseParser _landmarksBaseParser;
        private readonly LandmarksGraphParser _landmarksGraphParser;
        private readonly AccidentLineModelFactory _accidentLineModelFactory;
        private readonly MeasurementsForWebNotifier _measurementsForWebNotifier;
        private readonly MsmqMessagesProcessor _msmqMessagesProcessor;

        public WcfServiceWebC2D(IMyLog logFile, Model writeModel, CurrentDatacenterParameters currentDatacenterParameters,
            ClientsCollection clientsCollection, SorFileRepository sorFileRepository,
            D2RtuVeexLayer1 d2RtuVeexLayer1,
            IntermediateLayer intermediateLayer, D2RtuVeexLayer3 d2RtuVeexLayer3,
            LandmarksBaseParser landmarksBaseParser, LandmarksGraphParser landmarksGraphParser,
            AccidentLineModelFactory accidentLineModelFactory, MeasurementsForWebNotifier measurementsForWebNotifier,
            MsmqMessagesProcessor msmqMessagesProcessor)
        {
            _logFile = logFile;
            _writeModel = writeModel;
            _currentDatacenterParameters = currentDatacenterParameters;
            _clientsCollection = clientsCollection;
            _sorFileRepository = sorFileRepository;
            _d2RtuVeexLayer1 = d2RtuVeexLayer1;
            _intermediateLayer = intermediateLayer;
            _d2RtuVeexLayer3 = d2RtuVeexLayer3;
            _landmarksBaseParser = landmarksBaseParser;
            _landmarksGraphParser = landmarksGraphParser;
            _accidentLineModelFactory = accidentLineModelFactory;
            _measurementsForWebNotifier = measurementsForWebNotifier;
            _msmqMessagesProcessor = msmqMessagesProcessor;
        }

        public async Task<string> CheckDataCenterConnection()
        {
            await Task.Delay(1);
            return _currentDatacenterParameters.DatacenterVersion;
        }

        public async Task<bool> ChangeGuidWithSignalrConnectionId(string oldGuid, string connId)
        {
            return await _clientsCollection.ChangeGuidWithSignalrConnectionId(oldGuid, connId);
        }

        public async Task<string> GetAboutInJson(string username)
        {
            _logFile.AppendLine(":: WcfServiceForWebProxy GetAboutInJson");
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            if (user == null)
            {
                _logFile.AppendLine("Not authorized access");
                return null;
            }
            await Task.Delay(1);

            try
            {
                var result = new AboutDto();
                result.DcSoftware = _currentDatacenterParameters.DatacenterVersion;
                result.Rtus = _writeModel.CreateAboutRtuList(user).ToList();
                _logFile.AppendLine($"Rtus contains {result.Rtus.Count} RTU");
                var resString = JsonConvert.SerializeObject(result, JsonSerializerSettings);
                return resString;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"{e.Message}");
                return "";
            }
        }

        public async Task<string> GetCurrentAccidents(string username)
        {
            _logFile.AppendLine(":: WcfServiceForWebProxy GetCurrentAccidents");
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            if (user == null)
            {
                _logFile.AppendLine("Not authorized access");
                return null;
            }

            try
            {
                var result = new AlarmsDto
                {
                    NetworkAlarms = await GetCurrentNetworkEvents(username),
                    OpticalAlarms = await GetCurrentOpticalEvents(username),
                    BopAlarms = await GetCurrentBopEvents(username),
                };
                _logFile.AppendLine($"dto contains {result.NetworkAlarms.Count} network alarms, {result.OpticalAlarms.Count} optical alarms and {result.BopAlarms.Count} bop alarms");
                var resString = JsonConvert.SerializeObject(result, JsonSerializerSettings);
                return resString;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"{e.Message}");
                return "";
            }
        }

        public async Task<string> GetTreeInJson(string username)
        {
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            if (user == null)
            {
                _logFile.AppendLine("Not authorized access");
                return null;
            }
            await Task.Delay(1);

            try
            {
                var result = _writeModel.GetTree(_logFile, user).ToList();
                var resString = JsonConvert.SerializeObject(result, JsonSerializerSettings);
                _logFile.AppendLine(resString, 0, 3);
                return resString;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"{e.Message}");
                return "";
            }
        }

        public Task<byte[]> GetClientMeasurementResult(string username, Guid measId)
        {
            return Task.FromResult(_measurementsForWebNotifier.Extract(measId));
        }

     
        public async Task<AssignBaseParamsDto> GetAssignBaseParams(string username, Guid traceId)
        {
            if (!await Authorize(username, "GetAssignBaseParams")) return null;

            var result = new AssignBaseParamsDto();
            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (trace == null) return result;
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
            if (rtu == null) return result;
            result.RtuTitle = rtu.Title;
            result.OtdrId = rtu.OtdrId;
            result.PreciseId = trace.PreciseId;
            result.FastId = trace.FastId;
            result.AdditionalId = trace.AdditionalId;
            return result;
        }
     
        
       
    }
}
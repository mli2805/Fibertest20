using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class CommonBopProcessor
    {
        private readonly IniFile _iniFile;
        private readonly Model _writeModel;
        private readonly EventStoreService _eventStoreService;
        private readonly IFtSignalRClient _ftSignalRClient;
        private readonly Smtp _smtp;
        private readonly SmsManager _smsManager;
        private readonly SnmpNotifier _snmpNotifier;

        public CommonBopProcessor(IniFile iniFile, Model writeModel, 
            EventStoreService eventStoreService, IFtSignalRClient ftSignalRClient,
            Smtp smtp, SmsManager smsManager, SnmpNotifier snmpNotifier)
        {
            _iniFile = iniFile;
            _writeModel = writeModel;
            _eventStoreService = eventStoreService;
            _ftSignalRClient = ftSignalRClient;
            _smtp = smtp;
            _smsManager = smsManager;
            _snmpNotifier = snmpNotifier;
        }

        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingWebApiProfile>()).CreateMapper();

        public async Task PersistBopEvent(AddBopNetworkEvent cmd)
        {
            var result = await _eventStoreService.SendCommand(cmd, "system", "OnServer");
            if (string.IsNullOrEmpty(result))
            {
                var bopEvent = _writeModel.BopNetworkEvents.LastOrDefault();
                var signal = Mapper.Map<BopEventDto>(bopEvent);

                await _ftSignalRClient.NotifyAll("AddBopEvent", signal.ToCamelCaseJson());
                var unused = Task.Factory.StartNew(() => SendNotificationsAboutBop(bopEvent));
            }
        }

        private void SendNotificationsAboutBop(BopNetworkEvent cmd)
        {
            SetCulture();
            _smtp.SendBopState(cmd);
            _smsManager.SendBopState(cmd);
            _snmpNotifier.SendBopNetworkEvent(cmd);
        }

        private void SetCulture()
        {
            var cu = _iniFile.Read(IniSection.General, IniKey.Culture, "ru-RU");
            var currentCulture = new CultureInfo(cu);
            Thread.CurrentThread.CurrentUICulture = currentCulture;
        }
    }
}

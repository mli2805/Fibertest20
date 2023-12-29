using Iit.Fibertest.UtilsNet6;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Iit.Fibertest.RtuMngr
{
    public class MessageStorage
    {
        private readonly ILogger<RtuManager> _logger;

        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new() { TypeNameHandling = TypeNameHandling.All };

        public MessageStorage(ILogger<RtuManager> logger)
        {
            _logger = logger;
        }

        public void Initialize()
        {
            _logger.Info(Logs.RtuService, "Unsent message storage initialization ");
            // initialize DB sqlite
            _logger.Info(Logs.RtuService, "There is XXX messages ");
        }

        public void Push(object obj)
        {
            var json = JsonConvert.SerializeObject(obj, JsonSerializerSettings);
            _logger.Info(Logs.RtuService, json);
            //TODO
            // сохранить в sqlite - обеспечит потокобезопасность и хранение между перезагрузками
        }

        public void Pop()
        {
            // удалять из sqlite сразу когда отдали серверу или по отдельной команде от сервера
        }
    }
}

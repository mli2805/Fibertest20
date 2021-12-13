using System;
using System.IO;
using System.Threading.Tasks;
using NEventStore;
using Newtonsoft.Json;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfServiceDesktopC2D
    {
        public async Task<int> ExportEvents()
        {
            await Task.Factory.StartNew(ExportEventsInThread);
            return 0;
        }

        private async Task ExportEventsInThread()
        {
            var serializeSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            var ordinal = _eventStoreService.LastEventNumberInSnapshot + 1;
            var fileStream = File.CreateText(@"..\events.exp");
            fileStream.AutoFlush = true;

            var stream = _eventStoreService.StoreEvents.OpenStream(_eventStoreService.StreamIdOriginal, 0);
            try
            {
                foreach (var msg in stream.CommittedEvents)
                {
                    var json = JsonConvert.SerializeObject(msg.Body, serializeSettings);
                    await fileStream.WriteLineAsync($"{ordinal++,7} {msg.Headers["Timestamp"]} {msg.Headers["Username"]} {json}");
                }
            }
            catch (Exception e)
            {
                await fileStream.WriteLineAsync($"Exception {e.Message}");
            }
        }
    }
}

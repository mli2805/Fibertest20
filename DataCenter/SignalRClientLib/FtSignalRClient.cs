using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Microsoft.AspNetCore.SignalR.Client;

namespace Iit.Fibertest.FtSignalRClientLib
{
    public class FtSignalRClient
    {
        private HubConnection connection;
        private string _webApiUrl;

        public void Build()
        {
            var bindingProtocol = "https";
            _webApiUrl = $"{bindingProtocol}://localhost:11080/webApiSignalRHub";
            connection = new HubConnectionBuilder()
                .WithUrl(_webApiUrl)
                .Build();

            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };
        }

        public async Task Connect()
        {
            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                var newMessage = $"{user}: {message}";
                Console.WriteLine(newMessage);
            });

            try
            {
                await connection.StartAsync();
                Console.WriteLine("Connection started");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task Send()
        {
            try
            {
                var currentMonitoringStepDto = new CurrentMonitoringStepDto() { BaseRefType = BaseRefType.Fast };
                await connection.InvokeAsync("NotifyMonitoringStep", currentMonitoringStepDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}

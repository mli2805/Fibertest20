using Iit.Fibertest.Dto;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalRClientLib
{
    public class SignalRClient
    {
        private HubConnection connection;

        public void Build()
        {
            connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:11080/webApiSignalRHub")
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
                await connection.InvokeAsync("NotifyMonitoringStep", new CurrentMonitoringStepDto() { BaseRefType = BaseRefType.Fast });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}

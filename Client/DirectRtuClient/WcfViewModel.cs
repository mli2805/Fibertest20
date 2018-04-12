using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace DirectRtuClient
{
    public class WcfViewModel : Screen
    {
        private readonly IMyLog _logFile;
        private readonly IniFile _iniFile;

        private readonly string _username;
        private readonly Guid _clientId;
        private readonly C2DWcfManager _c2DWcfManager;

        private int _currentEventsCount;

        private readonly DoubleAddress _serverDoubleAddress;
        public string ServerAddress { get; set; }
        public List<string> Emails { get; set; }
        public string Email { get; set; }

        public WcfViewModel(IniFile iniFile, IMyLog logFile)
        {
           _iniFile = iniFile;
            _logFile = logFile;

            _username = @"developer";

            Guid.TryParse(iniFile.Read(IniSection.General, IniKey.ClientGuidOnServer, Guid.NewGuid().ToString()), out _clientId);
            _serverDoubleAddress  = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToClient);
            ServerAddress = _serverDoubleAddress.Main.Ip4Address;
            _c2DWcfManager = new C2DWcfManager(_iniFile, _logFile);
            var clientAddresses = _iniFile.Read(IniSection.ClientLocalAddress, (int)TcpPorts.ClientListenTo);
            _c2DWcfManager.SetServerAddresses(new DoubleAddress() { Main = new NetAddress(ServerAddress, TcpPorts.ServerListenToClient) }, _username, clientAddresses.Ip4Address);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = @"WCF";
        }

        public async void Register()
        {
           await RegisterClient(false);
        }

        public async void SendHeartbeat()
        {
           await RegisterClient(true);
        }

        public async void GetEvents()
        {
            var events = await GetEvents(_currentEventsCount);
            _currentEventsCount = _currentEventsCount + events.Length;
        }

        public override async void CanClose(Action<bool> callback)
        {
            await UnregisterClient();
            base.CanClose(callback);
        }

        public void SaveAddress()
        {
            _serverDoubleAddress.Main.Ip4Address = ServerAddress;
            _iniFile.WriteServerAddresses(_serverDoubleAddress);
        }

        private async Task<string[]> GetEvents(int currentEventsCount)
        {
            try
            {
                var events = await _c2DWcfManager.GetEvents(currentEventsCount);
                if (events.Length > 0)
                {
                    MessageBox.Show($@"{events.Length} events received");
                }
                return events;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        private async Task RegisterClient(bool isHeartbeat)
        {
            var dto = new RegisterClientDto()
            {
                ClientId = _clientId,
                UserName = _username,
                Password = _username,
                IsHeartbeat = isHeartbeat,
                Addresses = new DoubleAddress(),
            };

            try
            {
                var result = await _c2DWcfManager.RegisterClientAsync(dto);
                if (result.ReturnCode != ReturnCode.ClientRegisteredSuccessfully)
                {
                    MessageBox.Show(@"Error! Check log files");
                    _logFile.AppendLine(result.ExceptionMessage);
                }
                else
                {
                    if (!isHeartbeat)
                    {
                        var message = @"Registered successfully";
                        MessageBox.Show(message);
                        _logFile.AppendLine(message);
                    }
                    else _logFile.AppendLine(@"Heartbeat sent successfully");
                }
            
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
            }
        }

        private async Task UnregisterClient()
        {
            try
            {
                var dto = new UnRegisterClientDto(){ClientId = _clientId};
                await _c2DWcfManager.UnregisterClientAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
            }
        }

        // sslPort = 465; tlsPort = 587; tcpPort = 25;
        public async void SendEmails()
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.EnableSsl = true;
                    smtpClient.Timeout = 10000;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.UseDefaultCredentials = false;
                    // got letter from Gmail with request to allow to access my mail box from unsafe application, 
                    // after getting permission works fine
                    smtpClient.Credentials = new NetworkCredential("mli2805@gmail.com", "cde34rfV");

                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress("mli2805@gmail.com");
                    mail.To.Add(@"mli2805@yandex.rw");
                    mail.To.Add(@"mli2805@mail.ru");

                    mail.Subject = @"Test email";
                    mail.Body = @"Test email content";

                    await smtpClient.SendMailAsync(mail);
                }
            }
            catch (Exception e)
            {
                    Console.WriteLine(e);
                    throw;
            }
           
        }
    }
}

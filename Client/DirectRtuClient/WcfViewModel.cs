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
        private readonly string _clientIp;
        private readonly CommonC2DWcfManager _commonC2DWcfManager;
        private readonly DesktopC2DWcfManager _desktopC2DWcfManager;

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

            _serverDoubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToDesktopClient);
            ServerAddress = _serverDoubleAddress.Main.Ip4Address;
            var clientAddresses = _iniFile.Read(IniSection.ClientLocalAddress, (int)TcpPorts.ClientListenTo);
            _clientIp = clientAddresses.Ip4Address;

            _desktopC2DWcfManager = new DesktopC2DWcfManager(_iniFile, _logFile);
            _desktopC2DWcfManager.SetServerAddresses(new DoubleAddress()
            {
                Main = new NetAddress(ServerAddress, TcpPorts.ServerListenToDesktopClient)
            }, _username, clientAddresses.Ip4Address);

            _commonC2DWcfManager = new CommonC2DWcfManager(_iniFile, _logFile);
            var da = (DoubleAddress)_serverDoubleAddress.Clone();
            da.Main.Port = (int)TcpPorts.ServerListenToCommonClient;
            if (da.HasReserveAddress) da.Reserve.Port = (int)TcpPorts.ServerListenToCommonClient;
            _commonC2DWcfManager.SetServerAddresses(da, _username, clientAddresses.Ip4Address);
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
                var events = await _desktopC2DWcfManager.GetEvents(new GetEventsDto() { Revision = currentEventsCount });
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
                ClientIp = _clientIp,
                UserName = _username,
                Password = _username,
                Addresses = new DoubleAddress(),
            };

            try
            {
                var result = await _commonC2DWcfManager.RegisterClientAsync(dto);
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
                var dto = new UnRegisterClientDto() { ClientIp = _clientIp };
                await _commonC2DWcfManager.UnregisterClientAsync(dto);
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
                var smtpHost = _iniFile.Read(IniSection.Smtp, IniKey.SmtpHost, @"smtp.yandex.ru");
                var smtpPort = _iniFile.Read(IniSection.Smtp, IniKey.SmtpPort, 587);

                var mailFrom = _iniFile.Read(IniSection.Smtp, IniKey.MailFrom, @"mli2805@yandex.ru");
                var password = _iniFile.Read(IniSection.Smtp, IniKey.MailFromPassword, @"zaq1@WSX");

                using (SmtpClient smtpClient = new SmtpClient(smtpHost, smtpPort))
                {
                    smtpClient.EnableSsl = true;
                    smtpClient.Timeout = _iniFile.Read(IniSection.Smtp, IniKey.SmtpTimeoutMs, 10000);
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.UseDefaultCredentials = false;
                    // I got letter from Gmail with request to allow to access my mail box from unsafe application, 
                    // after getting permission works fine
                    smtpClient.Credentials = new NetworkCredential(mailFrom, password);

                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress(mailFrom);
                    mail.To.Add(@"mli2805@gmail.com");
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

        //        public async void SendEmails()
        //        {
        //            try
        //            {
        //                var smtpHost = _iniFile.Read(IniSection.Smtp, IniKey.SmtpHost, @"smtp.gmail.com");
        //                var smtpPort = _iniFile.Read(IniSection.Smtp, IniKey.SmtpPort, 587);
        //
        //                var mailFrom = _iniFile.Read(IniSection.Smtp, IniKey.MailFrom, @"fibertest2018@gmail.com");
        //                var password = _iniFile.Read(IniSection.Smtp, IniKey.MailFromPassword, @"Fibertest2018!");
        //
        //                using (SmtpClient smtpClient = new SmtpClient(smtpHost, smtpPort))
        //                {
        //                    smtpClient.EnableSsl = true;
        //                    smtpClient.Timeout = _iniFile.Read(IniSection.Smtp, IniKey.SmtpTimeoutMs, 10000);
        //                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        //                    smtpClient.UseDefaultCredentials = false;
        //                    // got letter from Gmail with request to allow to access my mail box from unsafe application, 
        //                    // after getting permission works fine
        //                    smtpClient.Credentials = new NetworkCredential(mailFrom, password);
        //
        //                    MailMessage mail = new MailMessage();
        //                    mail.From = new MailAddress(mailFrom);
        //                    mail.To.Add(@"mli2805@yandex.rw");
        //                    mail.To.Add(@"mli2805@mail.ru");
        //
        //                    mail.Subject = @"Test email";
        //                    mail.Body = @"Test email content";
        //
        //                    await smtpClient.SendMailAsync(mail);
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                    Console.WriteLine(e);
        //                    throw;
        //            }
        //           
        //        }
    }
}

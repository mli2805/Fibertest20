using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class Smtp
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;

        public Smtp(IniFile iniFile, IMyLog logFile, Model writeModel)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _writeModel = writeModel;
        }

        public async Task<bool> SendTestDispatch(CurrentDatacenterSmtpParametersDto dto)
        {
            _iniFile.Write(IniSection.Smtp, IniKey.SmtpHost, dto.SmptHost);
            _iniFile.Write(IniSection.Smtp, IniKey.SmtpPort, dto.SmptPort);
            _iniFile.Write(IniSection.Smtp, IniKey.MailFrom, dto.MailFrom);
            _iniFile.Write(IniSection.Smtp, IniKey.MailFromPassword, dto.MailFromPassword);
            _iniFile.Write(IniSection.Smtp, IniKey.SmtpTimeoutMs, dto.SmtpTimeoutMs);

            return await SendEmails(@"Test email - Тестовое сообщение",
                @"You received this letter because you are included in Fibertest alarm subscription. - Вы получили данное сообщение так как включены в  список рассылки Fibertest'a");
        }

        public async Task<bool> SendMonitoringResult()
        {
            return await SendEmails("", "");
        }

        private async Task<bool> SendEmails(string subject, string body)
        {
            try
            {
                var mailFrom = _iniFile.Read(IniSection.Smtp, IniKey.MailFrom, "");
                using (SmtpClient smtpClient = GetSmtpClient(mailFrom))
                {
                    var mail = GetMailMessage(mailFrom);
                    mail.Subject = subject;
                    mail.Body = body;
                    await smtpClient.SendMailAsync(mail);
                    return true;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return false;
            }
        }

        private MailMessage GetMailMessage(string mailFrom)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(mailFrom);

            foreach (var address in _writeModel.Users.Where(u => u.Email.IsActivated).Select(u => u.Email.Address))
                mail.To.Add(address);

            return mail;
        }

        private SmtpClient GetSmtpClient(string mailFrom)
        {
            var smtpHost = _iniFile.Read(IniSection.Smtp, IniKey.SmtpHost, "");
            var smtpPort = _iniFile.Read(IniSection.Smtp, IniKey.SmtpPort, 0);

            var password = _iniFile.Read(IniSection.Smtp, IniKey.MailFromPassword, "");

            SmtpClient smtpClient = new SmtpClient(smtpHost, smtpPort);

            smtpClient.EnableSsl = true;
            smtpClient.Timeout = _iniFile.Read(IniSection.Smtp, IniKey.SmtpTimeoutMs, 0);
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(mailFrom, password);

            return smtpClient;
        }
    }
}

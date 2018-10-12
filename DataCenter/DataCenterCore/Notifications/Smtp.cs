using System;
using System.Collections.Generic;
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

        private const string TestEmailSubj = @"Test email - Тестовое сообщение";
        private const string TestEmailMessage =
            @"You received this letter because you are included in Fibertest alarm subscription. - Вы получили данное сообщение так как включены в  список рассылки Fibertest'a";

        public Smtp(IniFile iniFile, IMyLog logFile, Model writeModel)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _writeModel = writeModel;
        }

        public void SaveSmtpSettings(SmtpSettingsDto dto)
        {
            _iniFile.Write(IniSection.Smtp, IniKey.SmtpHost, dto.SmptHost);
            _iniFile.Write(IniSection.Smtp, IniKey.SmtpPort, dto.SmptPort);
            _iniFile.Write(IniSection.Smtp, IniKey.MailFrom, dto.MailFrom);
            _iniFile.Write(IniSection.Smtp, IniKey.MailFromPassword, dto.MailFromPassword);
            _iniFile.Write(IniSection.Smtp, IniKey.SmtpTimeoutMs, dto.SmtpTimeoutMs);
        }

        public async Task<bool> SendTest(string address)
        {
            var mailTo = new List<string> { address };
            return await SendEmail(TestEmailSubj, TestEmailMessage, mailTo);
        }

        public async Task<bool> SendMonitoringResult(MonitoringResultDto dto)
        {
            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == dto.PortWithTrace.TraceId);
            if (trace == null) return false;
            var subj = $"Trace <<{trace.Title}>> state is {trace.State.ToLocalizedString()}.";

            // TODO Create report body
            var body = subj;
            var mailTo = _writeModel.Users.Where(u => u.Email.IsActivated).Select(u => u.Email.Address).ToList();
            return await SendEmail(subj, body, mailTo);
        }

        // userId - if empty - all users who have email
        private async Task<bool> SendEmail(string subject, string body, List<string> addresses)
        {
            try
            {
                var mailFrom = _iniFile.Read(IniSection.Smtp, IniKey.MailFrom, "");
                using (SmtpClient smtpClient = GetSmtpClient(mailFrom))
                {
                    var mail = new MailMessage
                    {
                        From = new MailAddress(mailFrom),
                        Subject = subject,
                        Body = body
                    };
                    foreach (var address in addresses)
                        mail.To.Add(address);
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

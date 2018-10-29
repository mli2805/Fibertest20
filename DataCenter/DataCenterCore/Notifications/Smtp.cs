using System;
using System.Collections.Generic;
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
            return await SendEmail(TestEmailSubj, TestEmailMessage, null, mailTo);
        }

        public async Task<bool> SendMonitoringResult(MonitoringResultDto dto)
        {
            var mailTo = _writeModel.GetEmailsToSendMonitoringResult(dto);
            _logFile.AppendLine($"There are {mailTo.Count} addresses to send e-mail");
            if (mailTo.Count == 0) return true;

            var subj = _writeModel.GetShortMessageForMonitoringResult(dto);
            var reportModel = _writeModel.CreateReportModelFromMoniresult(dto);
            var attachment = reportModel == null ? null : EventReport.FillInHtmlReportForTraceState(reportModel);
            return await SendEmail(subj, subj, attachment, mailTo);
        }

        public async Task<bool> SendNetworkEvent(Guid rtuId, bool isMainChannel, bool isOk)
        {
            var mailTo = _writeModel.GetEmailsToSendNetworkEvent(rtuId);
            _logFile.AppendLine($"There are {mailTo.Count} addresses to send e-mail");
            if (mailTo.Count == 0) return true;

            var subj = _writeModel.GetShortMessageForNetworkEvent(rtuId, isMainChannel, isOk);
            return await SendEmail(subj, subj, null, mailTo);
        }
        public async Task<bool> SendBopState(AddBopNetworkEvent cmd)
        {
            var mailTo = _writeModel.GetEmailsToSendBopNetworkEvent(cmd);
            _logFile.AppendLine($"There are {mailTo.Count} addresses to send e-mail");
            if (mailTo.Count == 0) return true;

            var subj = EventReport.GetShortMessageForBopState(cmd);
            return await SendEmail(subj, subj, null, mailTo);
        }
      
        // userId - if empty - all users who have email
        private async Task<bool> SendEmail(string subject, string body, string attachmentFilename, List<string> addresses)
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

                    if (attachmentFilename != null)
                        mail.Attachments.Add(new Attachment(attachmentFilename));

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

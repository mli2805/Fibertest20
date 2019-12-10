using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly AccidentLineModelFactory _accidentLineModelFactory;
        private readonly TraceStateReportProvider _traceStateReportProvider;

        private const string TestEmailSubj = @"Test email - Тестовое сообщение";
        private const string TestEmailMessage =
            @"You received this letter because you are included in Fibertest alarm subscription. - Вы получили данное сообщение так как включены в  список рассылки Fibertest'a";

        public Smtp(IniFile iniFile, IMyLog logFile, Model writeModel, CurrentDatacenterParameters currentDatacenterParameters,
            AccidentLineModelFactory accidentLineModelFactory, TraceStateReportProvider traceStateReportProvider)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _writeModel = writeModel;
            _currentDatacenterParameters = currentDatacenterParameters;
            _accidentLineModelFactory = accidentLineModelFactory;
            _traceStateReportProvider = traceStateReportProvider;
        }

        public void SaveSmtpSettings(SmtpSettingsDto dto)
        {
            _iniFile.Write(IniSection.Smtp, IniKey.SmtpHost, dto.SmptHost);
            _iniFile.Write(IniSection.Smtp, IniKey.SmtpPort, dto.SmptPort);
            _iniFile.Write(IniSection.Smtp, IniKey.MailFrom, dto.MailFrom);
            _iniFile.Write(IniSection.Smtp, IniKey.MailFromPassword, dto.MailFromPassword);
            _iniFile.Write(IniSection.Smtp, IniKey.SmtpTimeoutMs, dto.SmtpTimeoutMs);

            _currentDatacenterParameters.Smtp.SmptHost = dto.SmptHost;
            _currentDatacenterParameters.Smtp.SmptPort = dto.SmptPort;
            _currentDatacenterParameters.Smtp.MailFrom = dto.MailFrom;
            _currentDatacenterParameters.Smtp.MailFromPassword = dto.MailFromPassword;
            _currentDatacenterParameters.Smtp.SmtpTimeoutMs = dto.SmtpTimeoutMs;
        }

        public async Task<bool> SendTest(string address)
        {
            var mailTo = new List<string> { address };
            return await SendEmail(TestEmailSubj, TestEmailMessage, null, mailTo);
        }

        public async Task<bool> SendOpticalEvent(MonitoringResultDto dto, AddMeasurement addMeasurement)
        {
            var mailTo = _writeModel.GetEmailsToSendMonitoringResult(dto);
            _logFile.AppendLine($"There are {mailTo.Count} addresses to send e-mail");
            if (mailTo.Count == 0) return true;

            var subj = _writeModel.GetShortMessageForMonitoringResult(dto);
            var attachmentFilename = PreparePdfAttachment(addMeasurement);
            return await SendEmail(subj, subj, attachmentFilename, mailTo);
        }

        private string PreparePdfAttachment(AddMeasurement addMeasurement)
        {
            try
            {
                var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string filename = Path.Combine(folder, $@"TraceStateReport{DateTime.Now:yyyy-MM-dd-hh-mm-ss}.pdf");
                var trace = _writeModel.Traces.First(t=>t.TraceId == addMeasurement.TraceId);
                var rtu = _writeModel.Rtus.First(r => r.Id == addMeasurement.RtuId);
                var reportModel = new TraceReportModel()
                {
                    TraceTitle = trace.Title,
                    TraceState = trace.State.ToLocalizedString(),
                    RtuTitle = rtu.Title,
                    RtuSoftwareVersion = rtu.Version,
                    PortTitle = trace.OtauPort.IsPortOnMainCharon
                        ? trace.OtauPort.OpticalPort.ToString()
                        : $@"{trace.OtauPort.Serial}-{trace.OtauPort.OpticalPort}",
                    MeasurementTimestamp = $@"{addMeasurement.MeasurementTimestamp:G}",
                    RegistrationTimestamp = $@"{addMeasurement.EventRegistrationTimestamp:G}",

                    Accidents = ConvertAccidents(addMeasurement.Accidents).ToList(),
                };
                _traceStateReportProvider.Create(reportModel, _currentDatacenterParameters).Save(filename);
                return filename;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(@"ShowReport: " + e.Message);
                return null;
            }
        }

        private IEnumerable<AccidentLineModel> ConvertAccidents(List<AccidentOnTraceV2> list)
        {
            var number = 0;
            var isGisOn = !_iniFile.Read(IniSection.General, IniKey.IsWithoutMapMode, false);
            foreach (var accidentOnTraceV2 in list)
            {
                yield return _accidentLineModelFactory.Create(accidentOnTraceV2, ++number, isGisOn);
            }
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
                var mailFrom = _currentDatacenterParameters.Smtp.MailFrom;
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
            SmtpClient smtpClient = new SmtpClient(_currentDatacenterParameters.Smtp.SmptHost,
                _currentDatacenterParameters.Smtp.SmptPort)
            {
                EnableSsl = true,
                Timeout = _currentDatacenterParameters.Smtp.SmtpTimeoutMs,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(mailFrom, _currentDatacenterParameters.Smtp.MailFromPassword)
            };

            return smtpClient;
        }
    }
}

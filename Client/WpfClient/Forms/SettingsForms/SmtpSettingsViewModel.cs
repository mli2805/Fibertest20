using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class SmtpSettingsViewModel : Screen
    {
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly IWindowManager _windowManager;

        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string MailFrom { get; set; }
        public string MailFromPassword { get; set; }
        public int SmtpTimeoutMs { get; set; }

        public bool IsEditEnabled { get; set; }

        public SmtpSettingsViewModel(CurrentDatacenterParameters currentDatacenterParameters, CurrentUser currentUser,
            IWcfServiceDesktopC2D c2DWcfManager, IWindowManager windowManager)
        {
            _currentDatacenterParameters = currentDatacenterParameters;
            IsEditEnabled = currentUser.Role <= Role.Root;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;

            SmtpHost = _currentDatacenterParameters.Smtp.SmptHost;
            SmtpPort = _currentDatacenterParameters.Smtp.SmptPort;
            MailFrom = _currentDatacenterParameters.Smtp.MailFrom;
            MailFromPassword = _currentDatacenterParameters.Smtp.MailFromPassword;
            SmtpTimeoutMs = _currentDatacenterParameters.Smtp.SmtpTimeoutMs;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_E_mail_settings;
        }

        public async void Save()
        {
            bool res;
            using (new WaitCursor())
            {
                _currentDatacenterParameters.Smtp.SmptHost = SmtpHost;
                _currentDatacenterParameters.Smtp.SmptPort = SmtpPort;
                _currentDatacenterParameters.Smtp.MailFrom = MailFrom;
                _currentDatacenterParameters.Smtp.MailFromPassword = MailFromPassword;
                _currentDatacenterParameters.Smtp.SmtpTimeoutMs = SmtpTimeoutMs;

                var dto = new SmtpSettingsDto()
                {
                    SmptHost = SmtpHost,
                    SmptPort = SmtpPort,
                    MailFrom = MailFrom,
                    MailFromPassword = MailFromPassword,
                    SmtpTimeoutMs = SmtpTimeoutMs,
                };
                res = await _c2DWcfManager.SaveSmtpSettings(dto);
            }

            if (res)
                TryClose();
            else
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Failed_to_save_smtp_settings_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
            }
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}

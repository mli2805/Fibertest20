using Iit.Fibertest.Dto;
using Iit.Fibertest.InstallLib;

namespace Iit.Fibertest.Install
{
    public static class CurrentInstallationExt
    {
        public static string GetWebApiSettingsJson(this CurrentInstallation currentInstallation)
        {
            // name and domain are used by DataCenter, not WebApi, but
            // placed here to read them (and all other parameters) during next installation

            return new WebApiSettings()
                {
                    ApiProtocol = currentInstallation.IsWebByHttps
                        ? "https"
                        : "http",
                    SslCertificateName = currentInstallation.SslCertificateName,
                    SslCertificateDomain = currentInstallation.SslCertificateDomain,
                    SslCertificatePath = currentInstallation.SslCertificatePath,
                    SslCertificatePassword = currentInstallation.SslCertificatePassword,
                }
                .ToCamelCaseJson();
        }

        public static string GetWebClientSettingsJson(this CurrentInstallation currentInstallation)
        {
            return new WebClientSettings()
                {
                    ApiProtocol = currentInstallation.IsWebByHttps
                        ? "https"
                        : "http",
                    SslCertificateDomain = currentInstallation.SslCertificateDomain,
                    ApiPort = (int)TcpPorts.WebApiListenTo,
                    Version = currentInstallation.ProductVersion,
                }
                .ToCamelCaseJson();
        }
    }
}
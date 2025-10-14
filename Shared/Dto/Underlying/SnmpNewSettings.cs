using System;

namespace Iit.Fibertest.Dto
{
    [Serializable]
    public class SnmpNewSettings
    {
        public bool Enabled { get; set; }
        public string SnmpVersion { get; set; } = "v1";
        public bool UseIitOid { get; set; } = true;
        public string CustomOid { get; set; }
        public string Community { get; set; }
        public string AuthoritativeEngineId { get; set; }
        public string UserName { get; set; }
        public bool IsAuthPswSet { get; set; }
        public string AuthenticationPassword { get; set; }
        public string AuthenticationProtocol { get; set; }
        public bool IsPrivPswSet { get; set; }
        public string PrivacyPassword { get; set; }
        public string PrivacyProtocol { get; set; }
        public string TrapReceiverAddress { get; set; }
        public int TrapReceiverPort { get; set; }

        public string SnmpEncoding { get; set; } = "utf8"; // не используется, всегда UTF8

        // влияет только на константы Состояние/Статус внутри трапов типа Fiber break / Обрыв волокна
        public string SnmpLanguage { get; set; } = "en-US";
    }
}

using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using Lextm.SharpSnmpLib;
using System.Collections.Generic;
using System.Net;
using System;
using System.Linq;
using System.Text;
using Iit.Fibertest.Dto;


namespace Utils471
{

    public interface ISnmpService
    {
        void SendSnmpTrap(SnmpNewSettings snmpNewSettings, FtTrapType specificTrapValue, Dictionary<FtTrapProperty, string> payload);
    }

#pragma warning disable CS0618

    public class SnmpService : ISnmpService
    {
        public void SendSnmpTrap(SnmpNewSettings snmpNewSettings, 
            FtTrapType specificTrapValue, Dictionary<FtTrapProperty, string> payload)
        {
            var ipAddress = IPAddress.Parse(snmpNewSettings.TrapReceiverAddress);
            var enterpriseOid = snmpNewSettings.UseIitOid ? "1.3.6.1.4.1.36220" : snmpNewSettings.CustomOid;
            var variables = GetVariables(payload, enterpriseOid).ToList();
            var trapOid = enterpriseOid + "." + (int)specificTrapValue;
            
            if (snmpNewSettings.SnmpVersion == "v1")
            {
                SendSnmpV1TrapV1(ipAddress, snmpNewSettings.TrapReceiverPort,
                    snmpNewSettings.Community, enterpriseOid, specificTrapValue, variables);
            }
            else // v3
            {
                // variables.Add(
                //     new Variable(
                //         new ObjectIdentifier(enterpriseOid + ".10"),
                //         new Integer32(specificTrapValue)));

                var privacyProvider = GetPrivacyProvider(
                    snmpNewSettings.AuthenticationProtocol,
                    snmpNewSettings.AuthenticationPassword,
                    snmpNewSettings.PrivacyProtocol,
                    snmpNewSettings.PrivacyPassword);
                SendSnmpV3TrapV2(ipAddress, snmpNewSettings.TrapReceiverPort, trapOid,
                    snmpNewSettings.AuthoritativeEngineId,
                    snmpNewSettings.UserName, privacyProvider, variables);
            }
        }

        private IEnumerable<Variable> GetVariables(Dictionary<FtTrapProperty, string> payload, string enterpriseOid)
        {
            foreach (KeyValuePair<FtTrapProperty, string> pair in payload)
            {
                // если отправить данные в этой кодировке, то PowerSNMP Free Manager правильно отображает русские и английские строки
                //var encoding1251 = Encoding.GetEncoding(1251);
                //ISnmpData data = new OctetString(pair.Value, encoding1251);

                ISnmpData data = new OctetString(pair.Value, Encoding.UTF8);
                var oid = enterpriseOid + $".{(int)pair.Key}";
                yield return new Variable(new ObjectIdentifier(oid), data);
            }
        }

        private IAuthenticationProvider GetAuthenticationProvider(string authProtocol, string authPassword)
        {
            switch (authProtocol.ToLowerInvariant())
            {
                case "md5":
                    return new MD5AuthenticationProvider(new OctetString(authPassword));
                case "sha":
                    return new SHA1AuthenticationProvider(new OctetString(authPassword));
                case "sha256":
                    return new SHA256AuthenticationProvider(new OctetString(authPassword));
                case "sha384":
                    return new SHA384AuthenticationProvider(new OctetString(authPassword));
                case "sha512":
                    return new SHA512AuthenticationProvider(new OctetString(authPassword));
                default:
                    throw new ArgumentException("Unknown authentication protocol");
            }
        }

        /// <summary>
        /// Builds privacy provider for SnmpV3 TrapV2
        /// if the authProtocol is "none" then the privProtocol is not considered
        /// </summary>
        /// <param name="authProtocol">none | md5 | sha | sha256 | sha384 | sha512</param>
        /// <param name="authPassword"></param>
        /// <param name="privProtocol"></param>
        /// <param name="privPassword">none | des | tripledes | aes128 | aes192 | aes256</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">if one of protocols is unknown</exception>
        private IPrivacyProvider GetPrivacyProvider(
            string authProtocol, string authPassword, string privProtocol, string privPassword)
        {
            // if the authProtocol is "none" then the privProtocol is not considered
            if (authProtocol == "none") return DefaultPrivacyProvider.DefaultPair;

            var authenticationProvider = GetAuthenticationProvider(authProtocol, authPassword);
            var privacy = new OctetString(privPassword);

            switch (privProtocol.ToLowerInvariant())
            {
                case "none":
                    return new DefaultPrivacyProvider(authenticationProvider);
                case "des":
                    return new DESPrivacyProvider(privacy, authenticationProvider);
                case "tripledes":
                    return new TripleDESPrivacyProvider(privacy, authenticationProvider);
                case "aes128":
                    return new AESPrivacyProvider(privacy, authenticationProvider);
                case "aes192":
                    return new AES192PrivacyProvider(privacy, authenticationProvider);
                case "aes256":
                    return new AES256PrivacyProvider(privacy, authenticationProvider);
                default:
                    throw new ArgumentException("Unknown privacy protocol");
            }
        }

        private void SendSnmpV3TrapV2(IPAddress address, int port, string trapOid,
            string engineId, string authUserName, IPrivacyProvider privacyProvider,
            List<Variable> payload)
        {
            var trap = new TrapV2Message(
                VersionCode.V3,
                0,
                0,
                new OctetString(authUserName),
                new ObjectIdentifier(trapOid),
                0,
                payload,
                privacyProvider,
                0x10000,
                new OctetString(ByteTool.Convert(engineId)),
                0,
                0);
            trap.Send(new IPEndPoint(address, port));
        }

        private void SendSnmpV1TrapV1(IPAddress address, int port, string community, string enterpriseOid,
            FtTrapType specificTrapValue, List<Variable> payload)
        {
            Messenger.SendTrapV1(
                new IPEndPoint(address, port),
                IPAddress.Loopback,
                new OctetString(community),
                new ObjectIdentifier(enterpriseOid),
                GenericCode.EnterpriseSpecific,
                (int)specificTrapValue,
                0,
                payload
            );
        }

        public bool SendOltTrap(SnmpNewSettings snmpNewSettings,  
            ObjectIdentifier enterpriseObjectIdentifier, uint uptime, List<Variable> payload)
        {
            var endpoint = CreateEndPoint(snmpNewSettings);
            if (endpoint == null) return false;

            Messenger.SendTrapV2(
                0,
                VersionCode.V2,
                CreateEndPoint(snmpNewSettings),
                new OctetString("public"), 
                enterpriseObjectIdentifier,
                uptime, 
                payload);

            return true;
        }

        private IPEndPoint CreateEndPoint(SnmpNewSettings snmpNewSettings)
        {
            if (snmpNewSettings == null)
                throw new ArgumentNullException(nameof(snmpNewSettings));

            if (!IPAddress.TryParse(snmpNewSettings.TrapReceiverAddress, out var ipAddress))
                return null;

            return new IPEndPoint(ipAddress, snmpNewSettings.TrapReceiverPort);
        }
    }
}
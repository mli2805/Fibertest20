using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Iit.Fibertest.DataCenterWebApi
{

    [Route("[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IMyLog _logFile;
        private readonly DoubleAddress _doubleAddress;
        private readonly DoubleAddress _doubleAddressForWebWcfManager;
        private readonly CommonC2DWcfManager _commonC2DWcfManager;
        private readonly WebC2DWcfManager _webC2DWcfManager;
        private readonly string _localIpAddress;
        private readonly string _version;

        public AuthenticationController(IniFile iniFile, IMyLog logFile)
        {
            _version = iniFile.Read(IniSection.General, IniKey.Version, "2.1.0.0");
            _logFile = logFile;
            _doubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToCommonClient);
            _doubleAddressForWebWcfManager = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToWebClient);
            _localIpAddress = iniFile.Read(IniSection.ClientLocalAddress, -1).Ip4Address;
            _commonC2DWcfManager = new CommonC2DWcfManager(iniFile, logFile);
            _webC2DWcfManager = new WebC2DWcfManager(iniFile, logFile);
        }

        private string GetRemoteAddress()
        {
            var ip1 = HttpContext.Connection.RemoteIpAddress.ToString();
            // browser started on the same pc as this service
            return ip1 == "::1" ? _localIpAddress : ip1;
        }

        [Authorize]
        [HttpPost("ChangeConnectionId")]
        public async Task ChangeGuidWithSignalrConnectionId()
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }
            dynamic user = JObject.Parse(body);
            var oldGuid = (string)user.oldGuid;
            var connId = (string)user.connId;
            await _webC2DWcfManager
                .SetServerAddresses(_doubleAddressForWebWcfManager, "WebApi", GetRemoteAddress())
                .ChangeGuidWithSignalrConnectionId(oldGuid, connId);
            _logFile.AppendLine($"User changed connection id.");
            Response.StatusCode = 201;
        }

        [Authorize]
        [HttpPost("Logout")]
        public async Task Logout()
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }
            dynamic dto = JObject.Parse(body);
            var username = (string)dto.username;
            await _commonC2DWcfManager
                .SetServerAddresses(_doubleAddress, username, GetRemoteAddress())
                .UnregisterClientAsync(
                    new UnRegisterClientDto { ClientIp = GetRemoteAddress(), Username = username });
            _logFile.AppendLine($"User {username} logged out.");
            Response.StatusCode = 201;
        }

        [Authorize]
        [HttpGet("Heartbeat/{connectionId}")]
        public async Task<RequestAnswer> Heartbeat(string connectionId)
        {
            var clientIp = GetRemoteAddress();
            var result = await _commonC2DWcfManager
                .SetServerAddresses(_doubleAddress, User.Identity.Name, clientIp)
                .RegisterHeartbeat(connectionId);
            return result;
        }



        [HttpPost("Login")]
        public async Task Login()
        {
            var clientIp = GetRemoteAddress();
            _logFile.AppendLine($"Authentication request from {clientIp}");
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }
            dynamic user = JObject.Parse(body);

            if (user.version != _version)
            {
                _logFile.AppendLine($"Web client version is {user.version}, Web API version is {_version}");
                await ReturnError(ReturnCode.VersionsDoNotMatch, "");
                return;
            }

            var connectionId = Guid.NewGuid().ToString();
            var clientRegisteredDto = await _commonC2DWcfManager
                .SetServerAddresses(_doubleAddress, (string)user.username, clientIp)
                .RegisterClientAsync(
                    new RegisterClientDto()
                    {
                        Addresses = new DoubleAddress()
                        {
                            Main = new NetAddress() { Ip4Address = clientIp, Port = 11080 },
                            HasReserveAddress = false
                        },
                        UserName = (string)user.username,
                        Password = (string)user.password,
                        ConnectionId = connectionId,
                        IsUnderSuperClient = false,
                        IsWebClient = true,
                    });

            if (clientRegisteredDto.ReturnCode != ReturnCode.ClientRegisteredSuccessfully)
            {
                await ReturnError(clientRegisteredDto.ReturnCode, clientRegisteredDto.ErrorMessage);
                return;
            }
          
            var identity = GetIdentity((string)user.username, clientRegisteredDto.Role);

            var now = DateTime.UtcNow;
            // create JWT
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromDays(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                username = (string)user.username,
                role = clientRegisteredDto.Role,
                zone = clientRegisteredDto.ZoneTitle,
                connectionId,
                jsonWebToken = encodedJwt,
                serverVersion = _version,
            };

            // response serialization
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }

        private async Task ReturnError(ReturnCode returnCode, string exceptionMessage)
        {
            Response.StatusCode = 401;
            var responseError = new { returnCode, exceptionMessage, serverVersion = _version };
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(responseError,
                new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }

        private ClaimsIdentity GetIdentity(string username, Role role)
        {
            _logFile.AppendLine($"User {username.ToUpper()} is logged in");

            var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, username),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, role.ToString()),
                };
            ClaimsIdentity claimsIdentity =
                new ClaimsIdentity(claims, "Token",
                    ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
            return claimsIdentity;

        }

    }

    public class AuthOptions
    {
        public const string ISSUER = "MyAuthServer"; // издатель токена
        public const string AUDIENCE = "http://localhost:4200/"; // потребитель токена ????
        const string KEY = "100TimesMoreSecret_SecretKey_С_русскими_буквами!";   // ключ для шифрации
        public const int LIFETIME = 400; // время жизни токена - дней
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
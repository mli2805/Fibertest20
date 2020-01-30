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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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
        private readonly IActionContextAccessor _accessor;
        private readonly DoubleAddress _doubleAddress;
        private readonly CommonC2DWcfManager _commonC2DWcfManager;

        public AuthenticationController(IniFile iniFile, IMyLog logFile, IActionContextAccessor accessor)
        {
            _logFile = logFile;
            _accessor = accessor;
            _doubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToCommonClient);
            _commonC2DWcfManager = new CommonC2DWcfManager(iniFile, logFile);
        }

        [HttpPost("Login")]
        public async Task Login()
        {
            var ip1 = HttpContext.Connection.RemoteIpAddress.ToString();
            _logFile.AppendLine($"Authentication request from {ip1}");
            var ip2 = _accessor.ActionContext.HttpContext.Connection.RemoteIpAddress.ToString();
            _logFile.AppendLine($"Authentication request from {ip2}");
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }
            dynamic user = JObject.Parse(body);
            _commonC2DWcfManager.SetServerAddresses(_doubleAddress, (string)user.username, "localhost");
            var clientRegisteredDto = await _commonC2DWcfManager.RegisterClientAsync(
                new RegisterClientDto()
                {
                    Addresses = new DoubleAddress()
                    {
                        Main = new NetAddress()
                        {
                            Ip4Address = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                        },
                        HasReserveAddress = false
                    },
                    UserName = (string)user.username,
                    Password = (string)user.password,
                    IsUnderSuperClient = false,
                    IsWebClient = true,
                });

            if (clientRegisteredDto.ReturnCode != ReturnCode.ClientRegisteredSuccessfully)
            {
                Response.StatusCode = 401;
                await Response.WriteAsync(clientRegisteredDto.ExceptionMessage + " ; Invalid username or password.");
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
                    expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                username = (string)user.username,
                role = clientRegisteredDto.Role,
                zone = clientRegisteredDto.ZoneId,
                jsonWebToken = encodedJwt,
            };

            // response serialization
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));
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

        // just for debug
        [HttpGet("Test")]
        public async Task<string> Test()
        {
            await Task.Delay(1);
            return "just to start under visual studio debug";
        }

    }

    public class AuthOptions
    {
        public const string ISSUER = "MyAuthServer"; // издатель токена
        public const string AUDIENCE = "http://localhost:4200/"; // потребитель токена ????
        const string KEY = "100TimesMoreSecret_SecretKey_С_русскими_буквами!";   // ключ для шифрации
        public const int LIFETIME = 1000; // время жизни токена - 1000 минута
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
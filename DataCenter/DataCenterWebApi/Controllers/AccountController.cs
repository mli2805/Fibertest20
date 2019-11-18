using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace Iit.Fibertest.DataCenterWebApi
{
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IMyLog _logFile;
        private readonly WebProxy2DWcfManager _webProxy2DWcfManager;

        public AccountController(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _webProxy2DWcfManager = new WebProxy2DWcfManager(iniFile, logFile);
            var doubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToWebProxy);
            _webProxy2DWcfManager.SetServerAddresses(doubleAddress, "webProxy", "localhost");
        }

        [HttpGet("GetInt")]
        public async Task<int> GetInt()
        {
            await Task.Delay(1);
            return 1;
        }

        [HttpPost("login")]
        public async Task<UserDto> Login()
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }
            dynamic user = JObject.Parse(body);
            var identity = await GetIdentity((string)user.username, (string)user.password);
            return identity;
        }

        private async Task<UserDto> GetIdentity(string username, string password)
        {
            var result = await _webProxy2DWcfManager.LoginWebClient(username, password);
            if (result == null) return null;
            _logFile.AppendLine($"User {username.ToUpper()} logged in");

            var now = DateTime.UtcNow;
            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                notBefore: now,
                expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            result.JsonWebToken = new JwtSecurityTokenHandler().WriteToken(jwt);

            return result;
        }

    }

    public class UserPair
    {
        public string username;
        public string password;
    }

    public class AuthOptions
    {
        public const string ISSUER = "MyAuthServer"; // издатель токена
        public const string AUDIENCE = "http://localhost:51884/"; // потребитель токена ????
        const string KEY = "mysupersecret_secretkey!123";   // ключ для шифрации
        public const int LIFETIME = 1; // время жизни токена - 1 минута
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
﻿using System;
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
        private readonly WebProxy2DWcfManager _webProxy2DWcfManager;

        public AuthenticationController(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _webProxy2DWcfManager = new WebProxy2DWcfManager(iniFile, logFile);
            var doubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToWebProxy);
            _webProxy2DWcfManager.SetServerAddresses(doubleAddress, "webProxy", "localhost");
        }

        [HttpPost("Login")]
        public async Task Login()
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }
            dynamic user = JObject.Parse(body);
            var userDto = await _webProxy2DWcfManager.LoginWebClient((string)user.username, (string)user.password);
            if (userDto == null)
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("Invalid username or password.");
                return;
            }

            var identity = await GetIdentity((string)user.username, (string)user.password);

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
                username = userDto.Username,
                role = userDto.Role,
                zone = userDto.Zone,
                jsonWebToken = encodedJwt,
            };

            // response serialization
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }

        private async Task<ClaimsIdentity> GetIdentity(string username, string password)
        {
            var result = await _webProxy2DWcfManager.LoginWebClient(username, password);
            if (result == null) return null;

            _logFile.AppendLine($"User {username.ToUpper()} logged in");

            var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, result.Username),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, result.Role)
                };
            ClaimsIdentity claimsIdentity =
            new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            return claimsIdentity;

        }
    }

    public class AuthOptions
    {
        public const string ISSUER = "MyAuthServer"; // издатель токена
        public const string AUDIENCE = "http://localhost:4200/"; // потребитель токена ????
        const string KEY = "mysupersecret_secretkey!123";   // ключ для шифрации
        public const int LIFETIME = 1000; // время жизни токена - 1000 минута
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
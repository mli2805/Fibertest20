using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public static class DigestAuth
    {
        private const string Username = "rtuadmin";

        public static string GetAuthorizationString(HttpResponseMessage responseMessage, string uri, string password, int nc)
        {
            var dict = ParseAuthHeader(responseMessage.Headers.WwwAuthenticate.ToString());
            var ha1 = GetHa1(Username, dict["realm"], password);
            var digestUrl = CutUrl(uri);
            var cnonce = GetRandomAsciiString(16);
            Debug.WriteLine($"cnonce is {cnonce}");
            var ha2 = GetHa2("GET", digestUrl);
            var ncStr = $"{nc:X8}";
            var response = GetHashResponse(ha1, dict["nonce"], ncStr, cnonce, "auth", ha2);

            var a1 = $"username=\"{Username}\", ";
            var a2 = $"realm=\"{dict["realm"]}\", ";
            var a3 = $"uri=\"{digestUrl}\", ";
            var a4 = $"algorithm={dict["algorithm"]}, ";
            Debug.WriteLine(a4);
            var a5 = "qop=auth, ";
            var a6 = $"nonce=\"{dict["nonce"]}\", ";
            var a7 = $"nc={ncStr}, ";
            var a8 = $"cnonce=\"{cnonce}\", ";
            var a9 = $"response=\"{response}\", ";
            var a10 = "opaque=\"undefined\"";
            return a1 + a2 + a3 + a4 + a5 + a6 + a7 + a8 + a9 + a10;
        }

        private static Dictionary<string, string> ParseAuthHeader(string authHeader)
        {
            var dict = new Dictionary<string, string>();
            if (authHeader.StartsWith("Digest "))
                authHeader = authHeader.Substring(7);
            var parts = authHeader.Split(',');
            foreach (var part in parts)
            {
                var halves = part.Trim().Split('=');
                if (halves[1][0] == '"')
                    halves[1] = halves[1].Substring(1, halves[1].Length - 2);
                if (!dict.ContainsKey(halves[0]))
                    dict.Add(halves[0], halves[1]);
            }

            return dict;
        }

        public static string GetHashResponse(string ha1, string nonce, string ncStr, string cnonce, string qop, string ha2)
        {
            return GetSha256($"{ha1}:{nonce}:{ncStr}:{cnonce}:{qop}:{ha2}");
        }
       
        public static string GetHa1(string username, string realm, string password)
        {
            return GetSha256($"{username}:{realm}:{password}");
        }

        public static string GetHa2(string method, string digestUri)
        {
            return GetSha256($"{method}:{digestUri}");
        }

        private static string GetSha256(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            SHA256Managed hashComputer = new SHA256Managed();
            byte[] hash = hashComputer.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += $"{x:x2}";
            }
            return hashString;
        }

        private static string CutUrl(string fullUrl)
        {
            var index = fullUrl.IndexOf("/api/v1", StringComparison.InvariantCulture);
            return fullUrl.Substring(index);
        }

        // cnonce generation
        private static Random _rd = new Random();
        private const string AllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
        private static string GetRandomAsciiString(int length)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(AllowedChars[_rd.Next(61)]);
            }
            return sb.ToString();
        }
    }
}

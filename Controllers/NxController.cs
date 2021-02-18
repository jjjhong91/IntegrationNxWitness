using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IntegrationNxWitness.AddControllersWithViews
{
    [ApiController]
    [Route("api/v1/[controller]/[action]")]
    public class NxController : Controller
    {
        private readonly string _username = "demo@networkoptix.com";
        private readonly string _password = "NxDemoUser";
        private readonly string _baseUrl = "http://demo.networkoptix.com:7001";
        private readonly IHttpClientFactory _client;
        private readonly ILogger<NxController> _logger;

        public NxController(IHttpClientFactory client, ILogger<NxController> logger)
        {
            _client = client;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Test()
        {
            return Ok("Test");
        }

        [HttpGet]
        public async Task<ActionResult<string>> NxAuthentication()
        {
            try
            {
                if (!await IsAuthenticationAsync())
                {
                    var nonce = await GetNonce();
                    var auth_digest = CalculatingAuthenticationHash(nonce);
                    var response = await CookieLogin(auth_digest);
                    return auth_digest;
                }

            }
            catch (System.Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }

            return "null";

        }

        private Task<HttpResponseMessage> CookieLogin(string auth_digest)
        {
            using (var client = _client.CreateClient())
            {
                var content = new StringContent(JsonSerializer.Serialize(new AuthContent
                {
                    AuthDigest = auth_digest
                }), Encoding.UTF8, "application/json");
                return client.PostAsync(_baseUrl + "/api/cookieLogin", content);

                //var result = await client.GetAsync($"{_baseUrl}/method?auth={auth_digest}");
                //return result;
            }
        }

        private async Task<bool> IsAuthenticationAsync()
        {
            using (var client = _client.CreateClient())
            {
                var response = await client.GetAsync(_baseUrl + "/api/getCurrentUser");
                return response.StatusCode == System.Net.HttpStatusCode.OK ? true : false;
            }
        }

        private async Task<Nonce> GetNonce()
        {
            using (var client = _client.CreateClient())
            {
                var response = await client.GetStringAsync(_baseUrl + "/api/getNonce");
                var nonce = JsonSerializer.Deserialize<Nonce>(response);
                return nonce;
            }

        }

        private string CalculatingAuthenticationHash(Nonce nonce)
        {
            var nonceValue = nonce.Reply.Nonce;
            var realm = nonce.Reply.Realm;

            var digest = MD5_Hex($"{_username.ToLower()}:{realm}:{_password}");
            var partial_ha2 = MD5_Hex("GET:");
            var simplified_ha2 = MD5_Hex($"{digest}:{nonceValue}:{partial_ha2}");
            var auth_digest = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username.ToLower()}:{nonceValue}:{simplified_ha2}"));
            return auth_digest;
        }

        private string MD5_Hex(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder();

                for (int i = 0; i < data.Length; i++)
                {
                    sb.Append(data[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }

    public class Nonce
    {
        [JsonPropertyName("error")]
        public string Error { get; set; }
        [JsonPropertyName("errorString")]
        public string ErrorString { get; set; }
        [JsonPropertyName("reply")]
        public Reply Reply { get; set; }
    }

    public class Reply
    {
        [JsonPropertyName("nonce")]
        public string Nonce { get; set; }
        [JsonPropertyName("realm")]
        public string Realm { get; set; }
    }

    public class AuthContent
    {
        [JsonPropertyName("auth")]
        public string AuthDigest { get; set; }
    }
}
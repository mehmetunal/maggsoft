using Maggsoft.Core.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Maggsoft.Framework.HttpClientApi
{
    public class MaggsoftHttpClient : IMaggsoftHttpClient
    {
        #region Fields

        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private decimal? _languageId = 0;

        #endregion

        #region Ctor

        public MaggsoftHttpClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            void Init()
            {
                _languageId = GetLang();
                if (_languageId != null)
                    _httpClient.DefaultRequestHeaders.Add("X-LanguageID", _languageId.ToString());

                var token = GetToken();
                if (!string.IsNullOrEmpty(token))
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var baseAddress = _configuration["HttpClientBaseAddress"];
                if (string.IsNullOrEmpty(baseAddress))
                    throw new ArgumentNullException("HttpClientBaseAddress");

                _httpClient.BaseAddress = new Uri(baseAddress);
            }

            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
            _configuration = configuration;
            Init();
        }

        #endregion

        #region Method

        /// <summary>
        /// Check whether the site is available
        /// </summary>
        /// <returns>The asynchronous task whose result determines that request is completed</returns>
        public virtual async Task PingAsync()
        {
            await _httpClient.GetStringAsync("/");
        }

        public virtual async Task<List<T>> GetAllAsync<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            var jObject = JsonConvert.DeserializeObject<Result<object>>(responseBody).Data.ToString();
            if (string.IsNullOrEmpty(jObject))
                return Activator.CreateInstance<List<T>>();

            var result = JsonConvert.DeserializeObject<List<T>>(jObject.ToString());

            return result;
        }

        public virtual async Task<T> GetAsync<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            var jObject = JsonConvert.DeserializeObject<Result<object>>(responseBody).Data.ToString();
            if (string.IsNullOrEmpty(jObject))
                return Activator.CreateInstance<T>();

            var result = JsonConvert.DeserializeObject<T>(jObject.ToString());

            return result;
        }

        public virtual async Task<HttpResponseMessage> GetClientAsync(string url, Dictionary<string, string> qParametre = null)
        {
            try
            {
                //if (qParametre != null)
                //{
                //    _queryParametre = qParametre.Union(_queryParametre.Where(k => !qParametre.ContainsKey(k.Key))).ToDictionary(k => k.Key, v => v.Value);
                //}
                var requestUri = QueryHelpers.AddQueryString(url, qParametre);
                return await _httpClient.GetAsync(requestUri);
            }
            catch
            {
                return null;
            }
        }

        public async Task<Result<T>> PostAsJsonAsync<T>(string url, T body) where T : class
        {
            HttpResponseMessage obj = await _httpClient.PostAsJsonAsync(url, body);
            obj.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<Result<T>>(await obj.Content.ReadAsStringAsync());
        }

        public async Task<Result<T>> PostAsync<T>(string url, T body) where T : class
        {
            var bodyJson = JsonConvert.SerializeObject(body);
            var stringContent = new StringContent(bodyJson, Encoding.UTF8, "application/json");

            HttpResponseMessage obj = await _httpClient.PostAsync(url, stringContent);
            obj.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<Result<T>>(await obj.Content.ReadAsStringAsync());
        }

        public async Task<Result<object>> PostAsync(string url, HttpContent content)
        {
            HttpResponseMessage obj = await _httpClient.PostAsync(url, content);
            obj.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<Result<object>>(await obj.Content.ReadAsStringAsync());
        }


        private string GetToken() => _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");

        public decimal? GetLang()
        {
            var languageId = _httpContextAccessor.HttpContext.Request.Headers["X-LanguageID"].FirstOrDefault();

            if (string.IsNullOrEmpty(languageId))
                return null;

            return decimal.Parse(languageId);
        }

        #endregion
    }
}

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
using System.Threading;
using System.Threading.Tasks;

namespace Maggsoft.Framework.HttpClientApi
{
    public class MaggsoftHttpClient : IMaggsoftHttpClient
    {
        #region Fields

        public readonly HttpClient _httpClient;
        public readonly IHttpContextAccessor _httpContextAccessor;
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

        public virtual async Task<List<TResult>> GetAllAsync<TResult>(string url) where TResult : class
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            var jObject = JsonConvert.DeserializeObject<Result<object>>(responseBody).Data.ToString();
            if (string.IsNullOrEmpty(jObject))
                return Activator.CreateInstance<List<TResult>>();

            var result = JsonConvert.DeserializeObject<List<TResult>>(jObject.ToString());

            return result;
        }

        public virtual async Task<TResult> GetAsync<TResult>(string url) where TResult : class
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();

                var jObject = JsonConvert.DeserializeObject<Result<object>>(responseBody).Data.ToString();
                if (string.IsNullOrEmpty(jObject))
                    return Activator.CreateInstance<TResult>();

                var result = JsonConvert.DeserializeObject<TResult>(jObject.ToString());

                return result;
            }
            catch (Exception ex)
            {

                throw;
            }
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

        public virtual async Task<Result<TResult>> PostAsJsonAsync<TResult>(string url, object body) where TResult : class
        {
            HttpResponseMessage obj = await _httpClient.PostAsJsonAsync(url, body);
            obj.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<Result<TResult>>(await obj.Content.ReadAsStringAsync());
        }

        public virtual async Task<Result<TResult>> PostAsync<TResult>(string url, object body) where TResult : class
        {
            var bodyJson = JsonConvert.SerializeObject(body);
            var stringContent = new StringContent(bodyJson, Encoding.UTF8, "application/json");

            HttpResponseMessage obj = await _httpClient.PostAsync(url, stringContent);
            obj.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<Result<TResult>>(await obj.Content.ReadAsStringAsync());
        }

        public virtual async Task<TResult> SendAsync<TResult>(string url, object body, HttpMethod method) where TResult : class
        {
            var request = new HttpRequestMessage(method, url);
            var bodyJson = JsonConvert.SerializeObject(body);
            var stringContent = new StringContent(bodyJson, Encoding.UTF8, "application/json");
            request.Content = stringContent;
            HttpResponseMessage obj = await _httpClient.SendAsync(request);
            obj.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<TResult>(await obj.Content.ReadAsStringAsync());
        }

        public virtual async Task<Result<object>> PostHttpContentAsync(string url, HttpContent content)
        {
            HttpResponseMessage obj = await _httpClient.PostAsync(url, content);
            obj.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<Result<object>>(await obj.Content.ReadAsStringAsync());
        }

        public virtual async Task<Result<TResult>> PutAsJsonAsync<TResult>(string url, object body) where TResult : class
        {
            HttpResponseMessage obj = await _httpClient.PutAsJsonAsync(url, body);
            obj.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<Result<TResult>>(await obj.Content.ReadAsStringAsync());
        }

        public virtual async Task<Result<TResult>> PutAsync<TResult>(string url, object body) where TResult : class
        {
            var bodyJson = JsonConvert.SerializeObject(body);
            var stringContent = new StringContent(bodyJson, Encoding.UTF8, "application/json");

            HttpResponseMessage obj = await _httpClient.PutAsync(url, stringContent);
            obj.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<Result<TResult>>(await obj.Content.ReadAsStringAsync());
        }

        public virtual async Task<Result<object>> PutHttpContentAsync(string url, HttpContent content)
        {
            HttpResponseMessage obj = await _httpClient.PutAsync(url, content);
            obj.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<Result<object>>(await obj.Content.ReadAsStringAsync());
        }

        public virtual async Task<Result> DeleteAsync(string url, object id)
        {
            HttpResponseMessage obj = await _httpClient.DeleteAsync($"{url}/{id}", CancellationToken.None);
            obj.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<Result>(await obj.Content.ReadAsStringAsync());
        }

        public virtual string GetToken() => _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");

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

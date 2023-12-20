using Maggsoft.Core.Base;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Maggsoft.Framework.HttpClientApi
{
    public interface IMaggsoftHttpClient
    {
        Task PingAsync();
        Task<List<T>> GetAllAsync<T>(string url);
        Task<T> GetAsync<T>(string url);
        Task<HttpResponseMessage> GetClientAsync(string url, Dictionary<string, string> qParametre = null);

        Task<Result<T>> PostAsJsonAsync<T>(string url, T body) where T : class;
        Task<Result<T>> PostAsync<T>(string url, T body) where T : class;
        Task<Result<object>> PostAsync(string url, HttpContent content);
    }
}

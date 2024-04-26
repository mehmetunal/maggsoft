using Maggsoft.Core.Base;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Maggsoft.Framework.HttpClientApi
{
    public interface IMaggsoftHttpClient
    {
        Task PingAsync();
        Task<List<TResult>> GetAllAsync<TResult>(string url) where TResult : class;
        Task<TResult> GetAsync<TResult>(string url) where TResult : class;
        Task<HttpResponseMessage> GetClientAsync(string url, Dictionary<string, string> qParametre = null);

        Task<Result<TResult>> PostAsJsonAsync<TResult>(string url, object body) where TResult : class;
        Task<Result<TResult>> PostAsync<TResult>(string url, object body) where TResult : class;
        Task<TResult> SendAsync<TResult>(string url, object body, HttpMethod method) where TResult : class;
        Task<Result<object>> PostAsync(string url, HttpContent content);

        Task<Result<TResult>> PutAsJsonAsync<TResult>(string url, object body) where TResult : class;
        Task<Result<TResult>> PutAsync<TResult>(string url, object body) where TResult : class;
        Task<Result<object>> PutAsync(string url, HttpContent content);

        Task<Result> DeleteAsync(string url, object id);
    }
}

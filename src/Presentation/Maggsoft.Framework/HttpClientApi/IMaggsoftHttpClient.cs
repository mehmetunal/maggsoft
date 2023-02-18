using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;

namespace Maggsoft.Framework.HttpClientApi
{
    public interface IMaggsoftHttpClient
    {
        Task PingAsync();
        Task<string> GetResource(string resourceKey);
        Task<List<T>> GetAllAsync<T>(string url);
        Task<T> GetAsync<T>(string url);
        Task<HttpResponseMessage> GetClientAsync(string url, Dictionary<string, string> qParametre = null);
    }
}

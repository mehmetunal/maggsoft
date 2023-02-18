using Maggsoft.Framework.Middleware;
using System;
using System.Threading.Tasks;

namespace Maggsoft.Framework.HttpClientApi
{
    public partial class MaggsoftAPIClientHelper
    {
        private readonly APIAuthMiddelwareHelper _ApiClient;
        private readonly string _ServerUrl;
        public MaggsoftAPIClientHelper(string accessToken, string serverUrl)
        {
            _ApiClient = new APIAuthMiddelwareHelper(accessToken);
            _ServerUrl = serverUrl;
        }


        public async Task<object> Get(string path)
        {
            string requestUriString = string.Format("{0}{1}", _ServerUrl, path);

            using (_ApiClient.ApiMiddelwareClient)
            {
                var result = await _ApiClient.ApiMiddelwareClient.GetAsync(requestUriString);

                if (result.IsSuccessStatusCode)
                {
                    string resultContent = await result.Content.ReadAsStringAsync();

                    return resultContent;
                }
                else
                {
                    throw new Exception(result.ReasonPhrase);
                }
            }
        }
    }
}

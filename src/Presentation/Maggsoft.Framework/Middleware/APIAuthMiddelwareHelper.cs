using Maggsoft.Core.Infrastructure;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Maggsoft.Framework.Middleware
{
    public class APIAuthMiddelwareHelper
    {
        public HttpClient ApiMiddelwareClient;
        private readonly string _AccessToken;

        public APIAuthMiddelwareHelper(string accessToken)
        {
            _AccessToken = accessToken;
            InitializeClient();
        }

        private void InitializeClient()
        {
            string api = MaggsoftContext.Current.Resolve<IConfiguration>().GetSection("ApiUrl").Value;

            ApiMiddelwareClient = new HttpClient();
            ApiMiddelwareClient.BaseAddress = new Uri(api);
            ApiMiddelwareClient.DefaultRequestHeaders.Accept.Clear();
            if (!string.IsNullOrEmpty(_AccessToken))
                ApiMiddelwareClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _AccessToken);
            ApiMiddelwareClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}

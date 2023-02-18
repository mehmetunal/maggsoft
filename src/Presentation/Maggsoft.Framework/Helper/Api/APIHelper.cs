using Maggsoft.Framework.Middleware;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Maggsoft.Framework.Helper.Api
{
    public class APIHelper
    {
        private readonly APIAuthMiddelwareHelper _ApiClient;

        public APIHelper()
        {
            _ApiClient = new APIAuthMiddelwareHelper("");
        }

        public async Task<object> Authenticate(string username, string password)
        {
            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password),
            });

            using (HttpResponseMessage response = await _ApiClient.ApiMiddelwareClient.PostAsync("/Token", data))
            {
                if (response.IsSuccessStatusCode)
                {
                    var result =JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                    result.IsLoggedIn = true;

                    var loginObject = new 
                    {
                        AuthenticatedUser = result,
                    };

                    return loginObject;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public async Task<object> Registrate(string username, string password, string confirmPassword)
        {
            var obj = new 
            {
                Email = username,
                Password = password,
                ConfirmPassword = confirmPassword,
            };

            var jsonString = JsonConvert.SerializeObject(obj);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            using (_ApiClient.ApiMiddelwareClient)
            {
                var result = await _ApiClient.ApiMiddelwareClient.PostAsync(_ApiClient.ApiMiddelwareClient.BaseAddress + "/api/Account/Register", content);

                if (result.IsSuccessStatusCode)
                {
                    string resultContent = await result.Content.ReadAsStringAsync();
                    var newObj = new 
                    {
                        RegisterModel = obj
                    };

                    return newObj;
                }
                else
                {
                    throw new Exception(result.ReasonPhrase);
                }
            }
        }
    }
}

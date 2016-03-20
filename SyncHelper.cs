using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

namespace GraphyClient
{
    public static class SyncHelper
    {
        public static async Task<List<T>> GetAsync<T>(string resourceEndpoint)
        {
            var uri = String.Format("{0}{1}/", ServerConstants.ApiRoot, resourceEndpoint);
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var contentString = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<List<T>>(contentString);
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

        public static async Task<bool> PostAsync(string resourceEndpoint, object data)
        {
            var uri = String.Format("{0}{1}/", ServerConstants.ApiRoot, resourceEndpoint);

            var jsonString = JsonConvert.SerializeObject(data);
            var body = new StringContent(jsonString, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(uri, body);

                if (response.IsSuccessStatusCode)
                {
                    // To be careful we can check the json result. However, it will be slower.
//                    var contentString = response.Content.ReadAsStringAsync().Result;
//                    var result = JsonConvert.DeserializeObject<T>(contentString);

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}


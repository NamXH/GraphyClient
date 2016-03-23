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
        public static async Task<KeyValuePair<int, List<T>>> GetAsync<T>(string resourceEndpoint)
        {
            var uri = String.Format("{0}{1}/", ServerConstants.ApiRoot, resourceEndpoint);
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var contentString = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<List<T>>(contentString);

                    return new KeyValuePair<int, List<T>>((int)response.StatusCode, result);
                }
                else
                {
                    return new KeyValuePair<int, List<T>>((int)response.StatusCode, null);
                }
            }
        }

        public static async Task<KeyValuePair<int, string>> PostAsync(string resourceEndpoint, object data)
        {
            var uri = String.Format("{0}{1}/", ServerConstants.ApiRoot, resourceEndpoint);

            var jsonString = JsonConvert.SerializeObject(data);
            var body = new StringContent(jsonString, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(uri, body);

                return new KeyValuePair<int, string>((int)response.StatusCode, "Post");

//                if (response.IsSuccessStatusCode)
//                {
                // To be careful we can check the json result. However, it will be slower.
//                    var contentString = response.Content.ReadAsStringAsync().Result;
//                    var result = JsonConvert.DeserializeObject<T>(contentString);
//                }
            }
        }

        public static async Task<KeyValuePair<int, string>> PutAsync(string resourceEndpoint, string resourceId, object data)
        {
            var uri = String.Format("{0}{1}/{2}/", ServerConstants.ApiRoot, resourceEndpoint, resourceId);

            var jsonString = JsonConvert.SerializeObject(data);
            var body = new StringContent(jsonString, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var response = await client.PutAsync(uri, body);

                return new KeyValuePair<int, string>((int)response.StatusCode, "Put");
            } 
        }

        public static async Task<KeyValuePair<int, string>> DeleteAsync(string resourceEndpoint, string resourceId, DateTime objectLastModified)
        {
            var uri = String.Format("{0}{1}/{2}/", ServerConstants.ApiRoot, resourceEndpoint, resourceId);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.IfUnmodifiedSince = objectLastModified; // By default, convert UTC to DatetimeOffset: https://msdn.microsoft.com/en-us/library/bb546101(v=vs.110).aspx

                var response = await client.DeleteAsync(uri);

                return new KeyValuePair<int, string>((int)response.StatusCode, "Delete");
            } 
        }
    }
}


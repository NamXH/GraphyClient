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

        // Hijack operationId in the params to make it easier when delete ops
        /// <returns>Tuple of: status code, return content string, http verb, associated sync op.</returns>
        public static async Task<Tuple<int, string, string, SyncOperation>> PostAsync(string resourceEndpoint, object data, SyncOperation operation)
        {
            var uri = String.Format("{0}{1}/", ServerConstants.ApiRoot, resourceEndpoint);

            var jsonString = JsonConvert.SerializeObject(data);
            var body = new StringContent(jsonString, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(uri, body);
                var content = await response.Content.ReadAsStringAsync();

                return new Tuple<int, string, string, SyncOperation>((int)response.StatusCode, content, "Post", operation);
            }
        }

        /// <returns>Tuple of: status code, return content string, http verb, associated sync op.</returns>
        public static async Task<Tuple<int, string, string, SyncOperation>> PutAsync(string resourceEndpoint, string resourceId, object data, SyncOperation operation)
        {
            var uri = String.Format("{0}{1}/{2}/", ServerConstants.ApiRoot, resourceEndpoint, resourceId);

            var jsonString = JsonConvert.SerializeObject(data);
            var body = new StringContent(jsonString, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var response = await client.PutAsync(uri, body);
                var content = await response.Content.ReadAsStringAsync();

                return new Tuple<int, string, string, SyncOperation>((int)response.StatusCode, content, "Put", operation);
            } 
        }

        /// <returns>Tuple of: status code, return content string, http verb, associated sync op.</returns>
        public static async Task<Tuple<int, string, string, SyncOperation>> DeleteAsync(string resourceEndpoint, string resourceId, DateTime objectLastModified, SyncOperation operation)
        {
            var uri = String.Format("{0}{1}/{2}/", ServerConstants.ApiRoot, resourceEndpoint, resourceId);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.IfUnmodifiedSince = objectLastModified; // By default, convert UTC to DatetimeOffset: https://msdn.microsoft.com/en-us/library/bb546101(v=vs.110).aspx

                var response = await client.DeleteAsync(uri);

                return new Tuple<int, string, string, SyncOperation>((int)response.StatusCode, null, "Delete", operation); // Always returns null string as a hack so all 3 functions Post, Put, Delete has the same signature
            } 
        }
    }
}


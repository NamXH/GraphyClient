using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace GraphyClient
{
    public class ClientExamples
    {
        public async Task GetTest()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(ServerConstants.ApiRoot + ServerConstants.ContactEndpoint);

                if (response.IsSuccessStatusCode)
                {
                    var contentString = response.Content.ReadAsStringAsync().Result;

                    var a = JsonConvert.DeserializeObject<List<Contact>>(contentString);
                }
                else
                {
                }
            }
        }

        public async Task PostTest()
        {
            var contact = new Contact
            {   
                Id = Guid.NewGuid(),
                FirstName = "Test05",
                Organization = "Test05",
                LastModified = DateTime.UtcNow,
            };
            var jsonString = JsonConvert.SerializeObject(contact);

            var body = new StringContent(jsonString, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(ServerConstants.ApiRoot + ServerConstants.ContactEndpoint, body);

                if (response.IsSuccessStatusCode)
                {
                    var contentString = response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                }
            }
        }

        public async Task DeleteTest()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.IfUnmodifiedSince = DateTime.UtcNow;
                var response = await client.DeleteAsync(ServerConstants.ApiRoot + ServerConstants.ContactEndpoint + "a669e4af-0000-4eeb-9929-c6529bf81386");

                if (response.IsSuccessStatusCode)
                {
                    var contentString = response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                }
            } 
        }

        public ClientExamples()
        {
        }
    }
}


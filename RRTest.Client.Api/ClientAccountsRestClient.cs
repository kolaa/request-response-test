using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using RRTest.Contracts;

namespace RRTest.Client.Api
{
    public class ClientAccountsRestClient
    {

        private readonly HttpClient _client;

        // Constructor
        public ClientAccountsRestClient(HttpClient client)
        {
            _client = client;
            _client.Timeout = new TimeSpan(0, 0, 30);
            _client.DefaultRequestHeaders.Clear();
        }

        public async Task<IEnumerable<Account>> GetAccountsAsync(int clientId)
        {
            //var response = await _client.GetAsync($"accounts/{clientId}");

            //response.EnsureSuccessStatusCode();

            //await using var responseStream = await response.Content.ReadAsStreamAsync();
            //return await JsonSerializer.DeserializeAsync<IEnumerable<Account>>(responseStream, new JsonSerializerOptions
            //{
            //    PropertyNameCaseInsensitive = true
            //});

            var responseString = await _client.GetStringAsync($"accounts/{clientId}");

            return Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Account>>(responseString);
        }

    }
}

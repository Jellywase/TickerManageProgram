using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using TickerManageProgram.TradeWatching;
using static System.Net.WebRequestMethods;

namespace TickerManageProgram
{
    internal class TiingoClient
    {
        static TiingoClient instance;
        static SemaphoreSlim instanceSS = new SemaphoreSlim(1, 1);
        bool initialized;

        readonly string apiKey = "764e0b5e97a7ddc7210c9c814fd91752e39959c0";

        public static async Task<TiingoClient> GetInstance()
        {
            await instanceSS.WaitAsync();
            try
            {
                if (instance == null)
                { instance = new TiingoClient(); }
                if (!instance.initialized)
                { await instance.Initialize(); }
                return instance;
            }
            catch
            {
                throw;
            }
            finally
            {
                instanceSS.Release();
            }
        }

        async Task Initialize()
        {
            initialized = true;
        }

        public async Task<JsonNode> GetLatestPrice(string ticker)
        {
            string endpoint = $"https://api.tiingo.com/tiingo/daily/{ticker}/prices";
            return await GetDataInternal(endpoint);
        }
        public async Task<JsonNode> GetHistoryPrices(string ticker, DateOnly from, DateOnly to)
        {
            string endpoint = $"https://api.tiingo.com/tiingo/daily/{ticker}/prices?startDate={from:yyyy-MM-dd}&endDate={to:yyyy-MM-dd}&format=csv&resampleFreq=daily";
            return await GetDataInternal(endpoint);
        }

        async Task<JsonNode> GetDataInternal(string endpoint)
        {
            var httpClient = await HttpClientProvider.GetInstance();

            var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
            req.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", apiKey);

            var response = await httpClient.SendAsync(req);
            response.EnsureSuccessStatusCode();
            using var stream = await response.Content.ReadAsStreamAsync();
            JsonNode json = JsonSerializer.Deserialize<JsonNode>(stream);
            if (json == null)
            {
                throw new Exception("json parsed tiingo response is null");
            }
            return json;
        }
    }
}

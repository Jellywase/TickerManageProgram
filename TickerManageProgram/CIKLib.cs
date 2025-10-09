using System.Text.Json;

namespace TickerManageProgram
{
    internal class CIKLib : ICIKLibrary
    {
        readonly static string userInfo = "Kwon Yongsoo (kys0521016@gmail.com)";
        readonly HttpClient httpClient;
        Dictionary<string, string> cikTable = null;
        object lockObj = new();


        public CIKLib()
        {
            httpClient = new();
            httpClient.DefaultRequestHeaders.Add("User-Agent", userInfo);
        }
        public string GetCIK(string ticker)
        {
            if (cikTable == null)
            {
                LogChannel.EnqueueLog(new Log(Log.LogType.system, "CIK table not initialized."));
                return string.Empty;
            }
            if (!cikTable.TryGetValue(ticker.ToUpper(), out string cikStr))
            {
                LogChannel.EnqueueLog(new Log(Log.LogType.system, $"Ticker not found: {ticker}"));
                return string.Empty;
            }
            return cikStr;
        }

        public async Task FetchTable()
        {
            try
            {
                string url = "https://www.sec.gov/files/company_tickers.json";
                string json = await httpClient.GetStringAsync(url);

                using JsonDocument doc = JsonDocument.Parse(json);
                cikTable = new();
                var elements = doc.RootElement.EnumerateObject();
                foreach (var element in elements)
                {
                    var cikStr = element.Value.GetProperty("cik_str").GetInt32();
                    var sym = element.Value.GetProperty("ticker").GetString();
                    if (sym == null)
                    { continue; }
                    cikTable.Add(sym.ToUpper(), cikStr.ToString().PadLeft(10, '0'));
                }
            }
            catch (Exception ex)
            {
                LogChannel.EnqueueLog(new Log(Log.LogType.system, "Fetch table 오류 발생: " + ex.Message));
            }
        }
    }
}

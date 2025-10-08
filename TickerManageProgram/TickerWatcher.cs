using System.Text;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace TickerManageProgram
{
    internal class TickerWatcher
    {
        readonly string ticker;
        readonly FormFetcher formFetcher;
        readonly FormWatcher form4Watcher;
        readonly FormWatcher form8kWatcher;
        readonly FormWatcher form10qWatcher;
        readonly FormWatcher form10kWatcher;

        JsonNode recentFilings = null;

        public TickerWatcher(string ticker)
        {
            this.ticker = ticker;

            // 경로 생성
            string directory = Path.Combine(AppContext.BaseDirectory, "tickers");
            directory = Path.Combine(directory, ticker);
            Directory.CreateDirectory(directory);

            formFetcher = new FormFetcher(ticker);
            form4Watcher = new FormWatcher(ticker, "4", directory);
            form8kWatcher = new FormWatcher(ticker, "8-K", directory);
            form10qWatcher = new FormWatcher(ticker, "10-Q", directory);
            form10kWatcher = new FormWatcher(ticker, "10-K", directory);
        }

        public async Task Update()
        {
            try
            {
                recentFilings = await formFetcher.FetchFilings();
                if (recentFilings == null)
                {
                    LogChannel.EnqueueLog(new Log(Log.LogType.system, "Failed to fetch filings."));
                }
                // 새로운 form4 감지
                else
                {
                    var newForm4IndexArr = form4Watcher.DetectAndApplyNewForm(recentFilings);
                    Task logNewForm4Task = LogNewForm4(newForm4IndexArr);
                    
                    var newForm8kIndexArr = form8kWatcher.DetectAndApplyNewForm(recentFilings);
                    var newForm10qIndexArr = form10qWatcher.DetectAndApplyNewForm(recentFilings);
                    var newForm10kIndexArr = form10kWatcher.DetectAndApplyNewForm(recentFilings);

                    // todo: analyse new forms?

                    await logNewForm4Task;
                }
            }
            catch (Exception ex)
            {
                LogChannel.EnqueueLog(new Log(Log.LogType.system, $"{ticker} Watcher - An error has occured: " + ex.Message));
            }
        }

        private async Task LogNewForm4(int[] newForm4IndexArr)
        {
            // 새로운 form4 감지시 로그
            if (newForm4IndexArr.Length > 0)
            {
                Form4XMLTranslator form4XMLTranslator = new();
                StringBuilder sb = new();
                foreach (var index in newForm4IndexArr)
                {
                    XDocument xmlForm4 = await formFetcher.GetXMLForm(recentFilings, index);

                    sb.Append("\n");
                    sb.Append(ticker + "\n");
                    sb.Append(form4XMLTranslator.Summary(xmlForm4));

                    LogChannel.EnqueueLog(new Log(Log.LogType.info, sb.ToString()));
                    sb.Clear();
                    // SEC 서버 부하 방지 위해 0.2초 지연
                    await Task.Delay(200);
                }
            }
        }
    }
}

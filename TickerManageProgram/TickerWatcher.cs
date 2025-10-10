using LLMLibrary;
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

        readonly LLMReportAnalyzer llmReportAnalyzer;

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
            llmReportAnalyzer = new LLMReportAnalyzer();
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
                else
                {
                    var newForm4IndexArr = form4Watcher.DetectAndApplyNewForm(recentFilings);
                    Task logNewForm4Task = Prefs.muteTickerLogging ? Task.CompletedTask : LogNewForm4s(recentFilings, newForm4IndexArr);
                    
                    var newForm8kIndexArr = form8kWatcher.DetectAndApplyNewForm(recentFilings);
                    Task analyzeForm8Task = Prefs.muteTickerLogging ? Task.CompletedTask : AnalyzeHTMLNewForm(recentFilings, newForm8kIndexArr, "8-K");

                    var newForm10qIndexArr = form10qWatcher.DetectAndApplyNewForm(recentFilings);
                    Task analyzeForm10qTask = Prefs.muteTickerLogging ? Task.CompletedTask : AnalyzeHTMLNewForm(recentFilings, newForm10qIndexArr, "10-Q");

                    var newForm10kIndexArr = form10kWatcher.DetectAndApplyNewForm(recentFilings);
                    Task analyzeForm10kTask = Prefs.muteTickerLogging ? Task.CompletedTask : AnalyzeHTMLNewForm(recentFilings, newForm10kIndexArr, "10-K");

                    Task.WaitAll(logNewForm4Task, analyzeForm8Task, analyzeForm10qTask, analyzeForm10kTask);
                }
            }
            catch (Exception ex)
            {
                LogChannel.EnqueueLog(new Log(Log.LogType.system, $"{ticker} Watcher - An error has occured: " + ex.Message));
            }
        }

        async Task LogNewForm4s(JsonNode recentFilings, int[] newForm4IndexArr)
        {
            // 새로운 form4 로그
            if (newForm4IndexArr.Length > 0)
            {
                Form4XMLTranslator form4XMLTranslator = new();
                StringBuilder sb = new();
                foreach (var index in newForm4IndexArr)
                {
                    XDocument xmlForm4 = formFetcher.ParseToXML(await formFetcher.GetFormString(recentFilings, index));

                    sb.AppendLine(ticker + " form" + "4");
                    sb.AppendLine("게시일: " + recentFilings["filingDate"].AsArray()[index].GetValue<string>() + "\n");
                    sb.Append(form4XMLTranslator.Summary(xmlForm4));

                    LogChannel.EnqueueLog(new Log(Log.LogType.info, sb.ToString()));
                    sb.Clear();
                    // SEC 서버 부하 방지 위해 0.2초 지연
                    await Task.Delay(200);
                }
            }
        }

        async Task AnalyzeHTMLNewForm(JsonNode recentFilings, int[] newFormsIndexArr, string formType)
        {
            // 새로운 form 분석 후 로그
            if (newFormsIndexArr.Length > 0)
            {
                StringBuilder sb = new();
                foreach (var index in newFormsIndexArr)
                {
                    string report = await formFetcher.ParseFromHTML(await formFetcher.GetFormString(recentFilings, index));

                    sb.AppendLine(ticker + " form" + formType);
                    sb.AppendLine("게시일: " + recentFilings["filingDate"].AsArray()[index].GetValue<string>());
                    sb.AppendLine("LLM의 분석: \n");
                    sb.Append(await llmReportAnalyzer.AnalyzeReport(ticker, report));

                    // 추후 llm 성능 개선시 플러스 지연 시도도 가능.
                    await llmReportAnalyzer.FlushContext(ticker);

                    LogChannel.EnqueueLog(new Log(Log.LogType.info, sb.ToString()));
                    sb.Clear();
                    // SEC 서버 부하 방지 위해 0.2초 지연
                    await Task.Delay(200);
                }
            }
        }
    }
}

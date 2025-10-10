namespace TickerManageProgram
{
    using LLMLibrary;
    using System.Text.Json.Nodes;
    using System.Xml.Linq;

    class TickerManageProgram
    {
        public static CancellationTokenSource mainCTS { get; private set; } = new CancellationTokenSource();
        static async Task Main()
        {
            // LLM Provider 초기화
            await LLMProvider.Initialize();

            // CIK Provider 초기화
            await CIKProvider.Initialize();

            // 로그 루프 시작
            _ = Task.Run(() => LogChannel.LoggingLoop(mainCTS.Token));

            FormFetcher ff = new("CALM");
            var recentFilings = await ff.FetchFilings();
            var formString = await ff.GetFormString(recentFilings, 2);
            var report = await ff.ParseFromHTML(formString);
            LLMReportAnalyzer analyzer = new();
            var analysis = await analyzer.AnalyzeReport("CALM", report);










            //// LLM Provider 초기화
            //await LLMProvider.Initialize();

            //// CIK Provider 초기화
            //await CIKProvider.Initialize();

            //// 로그 루프 시작
            //_ = Task.Run(() => LogChannel.LoggingLoop(mainCTS.Token));

            //// 명령어 입력 루프 시작
            //CommandInputter.CommandLoop(mainCTS.Token);

            return;
        }
    }
}
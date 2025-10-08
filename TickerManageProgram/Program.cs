namespace TickerManageProgram
{
    using LLMLibrary;
    class TickerManageProgram
    {
        public static ICIKProvider cikProvider { get; private set; }
        public static CancellationTokenSource mainCTS { get; private set; } = new CancellationTokenSource();
        static async Task Main()
        {
            // CIK 테이블 초기화
            CIKLib cikLib = new();
            cikProvider = cikLib;
            await cikLib.FetchTable();

            // 로그 루프 시작
            _ = Task.Run(() => LogChannel.LoggingLoop(mainCTS.Token));

            // 명령어 입력 루프 시작
            CommandInputter.CommandLoop(mainCTS.Token);

            return;
        }
    }
}
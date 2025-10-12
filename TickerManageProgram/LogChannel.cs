using System.Collections.Concurrent;

namespace TickerManageProgram
{
    internal static class LogChannel
    {
        static BlockingCollection<Log> logsQueue = new();

        public static async void LoggingLoop(CancellationToken token)
        {
            List<ILogPlatform> logPlatforms = new();

            // 외부 로그용 콘솔 먼저 초기화
            try
            {
                logPlatforms.Add(await new ExternalConsoleLogPlatform().Initialize());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Log Platform Initialization Failed: " + ex.Message);
            }

            // 여러 로그 플랫폼 초기화
            logPlatforms.Add(new KakaoTalkLogPlatform());
            logPlatforms.Add(new TelegramLogPlatform());

            foreach (var platform in logPlatforms)
            {
                try
                {
                    await platform.Initialize();
                }
                catch (Exception ex)
                {
                    LogChannel.EnqueueLog(new Log(Log.LogType.system, "Log Platform Initialization Failed: " + ex.Message));
                    logPlatforms.Remove(platform);
                }
            }

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var log = logsQueue.Take(token);
                    foreach (var platform in logPlatforms)
                    {
                        await platform?.SendLog(log);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                EnqueueLog(new Log(Log.LogType.system, "Logging Loop Cancelled"));
            }
            catch (Exception ex)
            {
                EnqueueLog(new Log(Log.LogType.system, "An error occurred: " + ex.Message));
            }
            finally
            {
                logPlatforms.Clear();
            }
        }

        public static void EnqueueLog(Log log)
        {
            logsQueue.Add(log);
        }
    }
}

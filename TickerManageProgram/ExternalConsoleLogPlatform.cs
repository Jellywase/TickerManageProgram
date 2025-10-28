using System.Diagnostics;
using System.IO.Pipes;

namespace TickerManageProgram
{
    internal class ExternalConsoleLogPlatform : ILogPlatform
    {
        public bool initialized { get; private set; }
        NamedPipeClientStream client;
        StreamWriter writer;
        ~ExternalConsoleLogPlatform()
        {
            client.Dispose();
            writer.Dispose();

            client = null;
            writer = null;
        }
        public async Task<ILogPlatform> Initialize()
        {
            if (initialized)
            { return this; }
            await Task.CompletedTask;
            try
            {
                // 콘솔 로거 프로세스 실행
                var psi = new ProcessStartInfo
                {
                    FileName = Path.Combine(AppContext.BaseDirectory, "Logger/Logger.exe"),
                    UseShellExecute = true,
                    CreateNoWindow = false
                };
                var logProc = Process.Start(psi);
                if (logProc == null)
                {
                    LogChannel.EnqueueLog(new Log(Log.LogType.system, "External Console Logging process is null"));
                    return this;
                }
                initialized = true;
            }
            catch (Exception ex)
            {
                LogChannel.EnqueueLog(new Log(Log.LogType.system, "External Console Logging process failed to start: " + ex.Message));
                return this;
            }

            try
            {
                // 로거와 네임드파이프 연결
                client = new NamedPipeClientStream(".", "LoggerPipe", PipeDirection.Out);
                client.Connect();

                // writer 해제시 로거 프로세스 종료됨.
                writer = new StreamWriter(client) { AutoFlush = true };
            }
            catch (Exception ex)
            {
                LogChannel.EnqueueLog(new Log(Log.LogType.system, "External Console Logging process failed to connect: " + ex.Message));
                return this;
            }
            return this;
        }

        public Task SendLog(Log log)
        {
            if (log.type is Log.LogType.ticker or Log.LogType.fj or Log.LogType.system)
            { 
                writer.WriteLine(log.message);
                writer.Flush();
            }
            return Task.CompletedTask;
        }
    }
}

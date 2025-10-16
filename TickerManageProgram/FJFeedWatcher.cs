using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinancialJuiceObserver;

namespace TickerManageProgram
{
    internal static class FJFeedWatcher
    {
        static object lockObj = new object();
        static FeedWatcher feedWatcher = new();
        static CancellationTokenSource cts;

        static async Task WatchLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await feedWatcher.Update();

                    await Task.Delay(60000, token);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                cts.Cancel();
            }
            finally
            {
                StopWatchLoop();
            }
        }

        public static void StartWatchLoop()
        {
            lock (lockObj)
            {
                if (cts != null)
                {
                    Console.WriteLine("Financial juice watch loop is already running.");
                    return;
                }
                cts = new CancellationTokenSource();
                _ = WatchLoop(cts.Token);
            }
        }
        public static void StopWatchLoop()
        {
            lock (lockObj)
            {
                if (cts == null)
                {
                    return;
                }
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
        }
    }
}

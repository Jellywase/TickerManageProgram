namespace TickerManageProgram
{
    internal static class TickerWatchManager
    {
        static object lockObj = new();
        static CancellationTokenSource cts = null;

        static async Task WatchLoop(CancellationToken token)
        {
            // Ticker Watcher 컨테이너
            Dictionary<string, TickerWatcher> tws = new();
            Action tickerChangedHandler = null;

            // 감시 티커 변경 이벤트 핸들러
            Action<string> tickerAddedHandler = (newTicker) =>
            {
                lock (lockObj)
                {
                    tickerChangedHandler += () =>
                    {
                        try
                        {
                            tws.Add(newTicker, new TickerWatcher(newTicker));
                        }
                        catch (ArgumentException)
                        {
                            Console.WriteLine("Duplicate ticker: " + newTicker);
                        }
                    };
                }
            };
            Action<string> tickerRemovedHandler = (removedTicker) =>
            {
                lock (lockObj)
                {
                    tickerChangedHandler += () =>
                    {
                        tws.Remove(removedTicker);
                    };
                }
            };

            Prefs.OnTickerAdded += tickerAddedHandler;
            Prefs.OnTickerRemoved += tickerRemovedHandler;

            // 초기 티커 로드
            var tickers = Prefs.GetTickers();
            foreach (var ticker in tickers)
            {
                tws.Add(ticker, new TickerWatcher(ticker));
            }

            try
            {
                // 개별 감시 루프
                while (!token.IsCancellationRequested)
                {
                    foreach (var twKVP in tws)
                    {
                        var tw = twKVP.Value;

                        await tw.Update();
                        // SEC 서버 부하 방지 위해  0.5초 지연
                        await Task.Delay(500);
                    }

                    // 감시 티커 변경 처리
                    lock (lockObj)
                    {
                        tickerChangedHandler?.Invoke();
                        tickerChangedHandler = null;
                    }

                    // 매 분 0초에 재개
                    DateTime now = DateTime.Now;
                    DateTime next = now.AddMinutes(1);
                    DateTime nextWithoutSeconds = new DateTime(next.Year, next.Month, next.Day, next.Hour, next.Minute, 0);
                    int waitSeconds = (int)(nextWithoutSeconds - now).TotalSeconds;

                    Console.WriteLine($"티커 감시루프 완료 {now.ToString()}");
                    await Task.Delay(waitSeconds * 1000, token);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in WatchLoop: " + ex.Message);
                StopWatchLoop();
            }
            finally
            {
                Prefs.OnTickerAdded -= tickerAddedHandler;
                Prefs.OnTickerRemoved -= tickerRemovedHandler;
            }
        }

        public static void StartWatchLoop()
        {
            lock (lockObj)
            {
                if (cts != null)
                {
                    Console.WriteLine("Watch loop is already running.");
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
                    Console.WriteLine("Watch loop is not running.");
                    return;
                }
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
        }
    }
}

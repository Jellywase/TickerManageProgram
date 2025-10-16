using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinancialJuiceObserver;
using LLMLibrary;

namespace TickerManageProgram
{
    internal static class FJFeedWatcher
    {
        static object lockObj = new object();
        static FeedWatcher feedWatcher = new();
        static CancellationTokenSource cts;
        static ILLMClient llmClient => LLMProvider.defaultLLMClient;
        enum FeedImportance { important, ordinary}

        static async Task WatchLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var newFeeds = await feedWatcher.Update();
                    await HandleNewFeeds(newFeeds);

                    // FinancialJuice 조회 텀: 1분
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

        static async Task HandleNewFeeds(IEnumerable<NewFeedInfo> newFeeds)
        {
            int cnt = 0;
            foreach (var newFeed in newFeeds)
            {
                string feedSummary = BuildFeedSummary(newFeed);

                // LLM에게 중요도 분석의뢰
                var importance = await AskImportanceToLLM(feedSummary, newFeed.pubDate.ToString() + cnt);
                
                // 중요도 높을 시 로그
                if (importance is FeedImportance.important)
                {
                    LogChannel.EnqueueLog(new Log(Log.LogType.info, feedSummary));
                }

                cnt++;
            }
        }

        static async Task<FeedImportance> AskImportanceToLLM(string feedSummary, string chatID)
        {
            const string systemMessage = "다음 소식을 듣고 미국의 주식과 경제에 큰 영향을 미칠경우 important라고만 답하고, 사소하다면 ordinary라고만 답해주세요.";
            await llmClient.SetSystemMessage(chatID, systemMessage);

            string importance = await llmClient.SendUserMessage(chatID, feedSummary);
            await llmClient.DisposeChat(chatID);
            switch (importance)
            {
                case "important":
                    return FeedImportance.important;
                    break;

                case "ordinary":
                    return FeedImportance.ordinary;
                    break;

                default:
                    return FeedImportance.ordinary;
                    break;
            }
        }
        static string BuildFeedSummary(NewFeedInfo feedInfo)
        {
            StringBuilder sb = new();
            sb.AppendLine($"보고일: {feedInfo.pubDate.ToString()}");
            sb.AppendLine($"제목: {feedInfo.title}");
            sb.Append($"내용: {feedInfo.description}");
            return sb.ToString();
        }
    }
}

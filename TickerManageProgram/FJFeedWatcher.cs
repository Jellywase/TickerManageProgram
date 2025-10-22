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
                    IEnumerable<NewFeedInfo> newFeeds = null;
                    try
                    {
                        newFeeds = await feedWatcher.Update();
                        await EvaluateAndLogNewFeeds(newFeeds);
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.ToString());
                    }

                    Console.WriteLine($"FJ 감시루프 완료 {DateTime.Now.ToString()}");
                    // FinancialJuice 조회 텀: 1분
                    await Task.Delay(60000, token);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
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

        static async Task EvaluateAndLogNewFeeds(IEnumerable<NewFeedInfo> newFeeds)
        {
            if (newFeeds == null || newFeeds.Count() == 0)
            {
                return;
            }
            int cnt = 0;
            foreach (var newFeed in newFeeds)
            {
                string feedSummary = BuildFeedSummary(newFeed);

                // LLM에게 중요도 분석의뢰
                var evaluation = await AskImportanceToLLM(feedSummary, newFeed.pubDate.ToString() + cnt);
                
                // 중요도 높을 시 로그
                if (evaluation.importance is FeedImportance.important)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(feedSummary);
                    sb.AppendLine();
                    sb.AppendLine("평가:");
                    sb.AppendLine(evaluation.reason);
                    LogChannel.EnqueueLog(new Log(Log.LogType.fj, sb.ToString()));
                }

                cnt++;
            }
        }

        static async Task<(FeedImportance importance, string reason)> AskImportanceToLLM(string feedSummary, string chatID)
        {
            const string systemMessage = "다음 소식을 듣고 미국의 주식과 경제에 큰 영향을 미칠경우 첫 줄에 imp, 사소하다면 ord라고만 답한 후, 이유도 간단하게 함께 알려주세요.";
            await llmClient.SetSystemMessage(chatID, systemMessage);

            string response = await llmClient.SendUserMessage(chatID, feedSummary);
            await llmClient.DisposeChat(chatID);
            string importance = new string(response.Take(3).ToArray());
            string reason = response.Substring(3);
            if (reason.StartsWith('\n') || reason.StartsWith(' '))
            { reason = reason.Substring(1); }
            switch (importance)
            {
                case "imp":
                    return (FeedImportance.important, reason);

                case "ord":
                    return (FeedImportance.ordinary, reason);

                default:
                    return (FeedImportance.ordinary, reason);
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

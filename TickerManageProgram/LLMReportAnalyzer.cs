using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LLMLibrary;
using SharpToken;

namespace TickerManageProgram
{
    public class LLMReportAnalyzer
    {
        ILLMClient llmClient;
        public LLMReportAnalyzer(ILLMClient llmClient = null)
        {
            this.llmClient = llmClient ?? LLMProvider.defaultLLMClient;
        }
        public async Task<string> AnalyzeReport(string chatID, string report)
        {
            if (llmClient == null || !(await llmClient.isConnected))
            { return string.Empty; }

            string response = string.Empty;
            try
            {
                response = await AnalyzeReportRecursive(chatID, report, 0);
                await llmClient.DisposeChat(chatID);
            }
            catch (Exception ex)
            {
                LogChannel.EnqueueLog(new Log(Log.LogType.system, "LLM 보고서 분석 오류 발생: " + ex.Message));
            }
            return response;
        }

        async Task<string> AnalyzeReportRecursive(string chatID, string report, int recursiveDepth)
        {
            const int recursiveLimit = 5;
            if (recursiveDepth > recursiveLimit)
            {
                Console.WriteLine($"{chatID} 재귀 한도 초과. 보고서가 너무 깁니다.");
                return string.Empty;
            }

            string response = string.Empty;
            List<string> reportChunks = LLMUtility.ChunkByTokens(report);
            if (reportChunks.Count <= 1)
            {
                string systemPrompt = "당신은 되도록 한글로 답해야합니다.";
                await llmClient.SetSystemMessage(chatID, systemPrompt);
                string prompt = $"다음 보고서를 분석, 요약하고 호재인지 악재인지 평가해주세요:\n\n{report}";
                response = await llmClient.SendUserMessage(chatID, prompt);
            }
            else
            {
                StringBuilder sb = new();
                for (int i = 0; i < reportChunks.Count; i++)
                {
                    string chunk = reportChunks[i];
                    string prompt = $"다음은 보고서의 일부입니다. 이 부분을 요약해 주세요:\n\n{chunk}";
                    string chunkChatID = $"{chatID}-{recursiveDepth}-{i}";
                    string chunkSummary = await llmClient.SendUserMessage(chunkChatID, prompt);
                    await llmClient.DisposeChat(chunkChatID);
                    sb.AppendLine(chunkSummary);
                }
                string combinedSummary = sb.ToString();
                response = await AnalyzeReportRecursive(chatID, combinedSummary, recursiveDepth);
            }

            return response;
        }

        public async Task FlushContext(string chatID)
        {
            if (llmClient == null || !(await llmClient.isConnected))
            { return; }

            try
            {
                await llmClient.DisposeChat(chatID);
            }
            catch (Exception ex)
            {
                LogChannel.EnqueueLog(new Log(Log.LogType.system, "LLM 보고서 분석기 컨텍스트 플러시 오류 발생: " + ex.Message));
            }
        }
    }
}

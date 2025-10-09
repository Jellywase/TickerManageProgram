using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LLMLibrary;

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
                string prompt = $"다음 보고서를 분석, 요약하고 호재인지 악재인지 평가해주세요:\n\n{report}";
                response = await llmClient.SendUserMessage(chatID, prompt);
            }
            catch (Exception ex)
            {
                LogChannel.EnqueueLog(new Log(Log.LogType.system, "LLM 보고서 분석 오류 발생: " + ex.Message));
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

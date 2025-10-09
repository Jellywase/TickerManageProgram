using LLMLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickerManageProgram
{
    public static class LLMProvider
    {
        static object lockObj = new();
        public static ILLMClient defaultLLMClient
        {
            get
            {
                lock (lockObj)
                {
                    llmClient_I ??= new LMStudioClient(ModelInfo.ModelName.gpt_oss_20b, null);
                }
                return llmClient_I;
            }
        }
        static ILLMClient llmClient_I;
    }
}

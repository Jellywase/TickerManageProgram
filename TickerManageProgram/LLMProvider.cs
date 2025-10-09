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
        public static ILLMClient defaultLLMClient { get; private set; }
        public static async Task Initialize()
        {
            defaultLLMClient ??= new LMStudioClient(ModelInfo.ModelName.gpt_oss_20b, null);
            await defaultLLMClient.Connect();
        }
    }
}

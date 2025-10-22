using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickerManageProgram
{
    internal abstract class TelegramLogPlatform : ILogPlatform
    {
        public bool initialized { get; private set; }
        readonly HttpClient httpClient = new HttpClient();
        protected abstract string botToken { get; }
        protected abstract string tickerLogChatID { get; }

        public async Task<ILogPlatform> Initialize()
        {
            initialized = true;
            return this;
        }

        protected abstract bool SendLogCondition(Log.LogType logType);

        public async Task SendLog(Log log)
        {
            try
            {
                if (SendLogCondition(log.type))
                {
                    string url = $"https://api.telegram.org/bot{botToken}/sendMessage";

                    using var parameters = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("chat_id", tickerLogChatID),
                        new KeyValuePair<string, string>("text", log.message)
                    });

                    using var response = await httpClient.PostAsync(url, parameters);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending log to Telegram: {ex.Message}");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickerManageProgram
{
    internal class TelegramLogPlatform : ILogPlatform
    {
        public bool initialized { get; private set; }
        readonly HttpClient httpClient = new HttpClient();
        readonly static string chatID = "-1002793460216";
        readonly static string botToken = "7443923469:AAHlV0vPLWbGcHW1VKGarBTPe2rNq0lL9vs";

        public async Task<ILogPlatform> Initialize()
        {
            initialized = true;
            return this;
        }

        public async Task SendLog(Log log)
        {
            try
            {
                if (log.type == Log.LogType.info)
                {
                    string url = $"https://api.telegram.org/bot{botToken}/sendMessage";

                    using var parameters = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("chat_id", chatID),
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

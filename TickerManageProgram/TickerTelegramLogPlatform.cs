using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickerManageProgram
{
    internal class TickerTelegramLogPlatform : TelegramLogPlatform
    {
        protected override string botToken { get; } = "7443923469:AAHlV0vPLWbGcHW1VKGarBTPe2rNq0lL9vs";

        protected override string tickerLogChatID { get; } = "-1002793460216";

        protected override bool SendLogCondition(Log.LogType logType)
        {
            return logType is Log.LogType.ticker or Log.LogType.test;
        }
    }
}

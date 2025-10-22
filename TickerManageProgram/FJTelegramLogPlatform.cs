using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickerManageProgram
{
    internal class FJTelegramLogPlatform : TelegramLogPlatform
    {
        protected override string botToken { get; } = "7443923469:AAHlV0vPLWbGcHW1VKGarBTPe2rNq0lL9vs";

        protected override string tickerLogChatID { get; } = "-1003124772104";

        protected override bool SendLogCondition(Log.LogType logType)
        {
            return logType is Log.LogType.fj or Log.LogType.test;
        }
    }
}

namespace TickerManageProgram
{

    internal class Log
    {
        public enum LogType { system, ticker, fj, test }
        public LogType type { get; private set; }
        public string message { get; private set; }
        DateTime time;
        public Log(LogType type, string message, bool addTime = true, bool addLine = true)
        {
            this.type = type;
            this.message = message;
            if (addTime)
            { 
                time = DateTime.Now;
                this.message = time.ToString("yyyy-MM-dd HH:mm:ss") + "\n" + this.message;
            }
            if (addLine)
            {
                this.message = "\n" + this.message;
            }
        }
    }
}

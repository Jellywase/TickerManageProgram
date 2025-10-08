namespace TickerManageProgram
{

    internal interface ILogPlatform
    {
        public bool initialized { get; }
        public Task<ILogPlatform> Initialize();
        public Task SendLog(Log log);
    }
}

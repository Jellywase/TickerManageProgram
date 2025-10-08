namespace TickerManageProgram
{
    internal static class CommandInputter
    {
        public static void CommandLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                string cmd = Console.ReadLine() ?? string.Empty;
                cmd = cmd.ToUpperInvariant();
                if (cmd.Equals(string.Empty))
                {
                    continue;
                }
                if (!CommandManager.commands.TryGetValue(cmd, out var command))
                {
                    Console.WriteLine($"{cmd}에 해당하는 명령이 없습니다.");
                }
                else
                {
                    CommandManager.commands[cmd]?.Execute();
                    Console.WriteLine($"{cmd} 실행됨");
                }
            }
        }
    }
}

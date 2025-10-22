namespace TickerManageProgram
{
    internal static class CommandManager
    {
        public static Dictionary<string, Command> commands = new();
        static CommandManager()
        {
            AddCommand(new AddTickerCommand());
            AddCommand(new RemoveTickerCommand());
            AddCommand(new ListTickersCommand());
            AddCommand(new ClearTickersCommand());
            AddCommand(new StartWatchingTickerCommand());
            AddCommand(new StopWatchingTickerCommand());
            AddCommand(new ExitCommand());
            AddCommand(new MuteTickerLogCommand());
            AddCommand(new ListCommandsCommand());
            AddCommand(new LogCommand());
            AddCommand(new StartWatchingFJCommand());
            AddCommand(new StopWatchingFJCommand());
        }
        static void AddCommand(Command command)
        {
            commands.Add(command.commandID.ToUpperInvariant(), command);
        }
    }
    internal abstract class Command
    {
        public abstract string commandID { get; }
        public abstract void Execute();
    }
    internal class AddTickerCommand : Command
    {
        public override string commandID => "Add Ticker";

        public override void Execute()
        {
            Console.WriteLine("Ticker를 입력:");
            string ticker = Console.ReadLine()?.Trim().ToUpperInvariant() ?? string.Empty;
            if (!ticker.Equals(string.Empty))
            {
                if (!Prefs.AddTicker(ticker))
                {
                    Console.WriteLine("중복된 티커: " + ticker);
                }
            }
        }
    }

    internal class RemoveTickerCommand : Command
    {
        public override string commandID => "Remove Ticker";
        public override void Execute()
        {
            Console.WriteLine("Ticker를 입력:");
            string ticker = Console.ReadLine()?.Trim().ToUpperInvariant() ?? string.Empty;
            if (!ticker.Equals(string.Empty))
            {
                Prefs.RemoveTicker(ticker);
            }
        }
    }

    internal class ListTickersCommand : Command
    {
        public override string commandID => "List Tickers";
        public override void Execute()
        {
            Console.WriteLine("관찰중인 Ticker 목록: ");
            foreach (string ticker in Prefs.GetTickers())
            {
                Console.WriteLine(ticker);
            }
        }
    }

    internal class ClearTickersCommand : Command
    {
        public override string commandID => "Clear Tickers";
        public override void Execute()
        {
            Console.WriteLine("모든 티커를 제거합니다. 계속하려면 Y를 입력하세요:");
            string input = Console.ReadLine()?.Trim().ToUpperInvariant() ?? string.Empty;
            if (input == "Y")
            {
                var tickers = Prefs.GetTickers().ToList();
                foreach (var ticker in tickers)
                {
                    Prefs.RemoveTicker(ticker);
                }
                Console.WriteLine("모든 티커가 제거되었습니다.");
            }
            else
            {
                Console.WriteLine("취소됨.");
            }
        }
    }

    internal class StartWatchingTickerCommand : Command
    {
        public override string commandID => "Start Watching Ticker";
        public override void Execute()
        {
            Console.WriteLine("티커 관찰 시작");
            TickerWatchManager.StartWatchLoop();
        }
    }

    internal class StopWatchingTickerCommand : Command
    {
        public override string commandID => "Stop Watching Ticker";
        public override void Execute()
        {
            Console.WriteLine("티커 관찰 중지");
            TickerWatchManager.StopWatchLoop();
        }
    }

    internal class ExitCommand : Command
    {
        public override string commandID => "Exit";
        public override void Execute()
        {
            TickerManageProgram.mainCTS.Cancel();
            Environment.Exit(0);
        }
    }
    
    internal class MuteTickerLogCommand : Command
    {
        public override string commandID => "Mute Ticker Log";
        public override void Execute()
        {
            Console.WriteLine("음소거: o, 음소거 해제: x");
            string input = Console.ReadLine()?.Trim().ToLowerInvariant() ?? string.Empty;
            if (input == "o")
            {
                Prefs.muteTickerLogging = true;
            }
            else if (input == "x")
            {
                Prefs.muteTickerLogging = false;
            }
            else
            {
                Console.WriteLine("잘못된 입력. 취소됨.");
            }
        }
    }

    internal class ListCommandsCommand : Command
    {
        public override string commandID => "List Commands";
        public override void Execute()
        {
            Console.WriteLine("사용 가능한 명령어:");
            foreach (var cmd in CommandManager.commands.Values)
            {
                Console.WriteLine("- " + cmd.commandID);
            }
        }
    }

    internal class LogCommand : Command
    {
        public override string commandID => "Log";
        public override void Execute()
        {
            Console.WriteLine("Log Message: ");
            LogChannel.EnqueueLog(new Log(Log.LogType.test, Console.ReadLine() ?? string.Empty));
        }
    }

    internal class StartWatchingFJCommand : Command
    {
        public override string commandID => "Start Watching FJ";
        public override void Execute()
        {
            Console.WriteLine("Financial Juice 관찰 시작");
            FJFeedWatcher.StartWatchLoop();
        }
    }

    internal class StopWatchingFJCommand : Command
    {
        public override string commandID => "Stop Watching FJ";
        public override void Execute()
        {
            Console.WriteLine("Financial Juice 관찰 중지");
            FJFeedWatcher.StopWatchLoop();
        }
    }
}

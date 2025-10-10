using System.Text.Json;
using System.Text.Json.Serialization;

namespace TickerManageProgram
{
    internal static class Prefs
    {
        static PrefsData prefsData;
        static object lockObj = new();
        public static event Action<string> OnTickerAdded;
        public static event Action<string> OnTickerRemoved;
        public static bool muteTickerLogging
        {
            get
            {
                lock (lockObj)
                {
                    return prefsData.muteTickerLogging;
                }
            }
            set
            {
                lock (lockObj)
                {
                    prefsData.muteTickerLogging = value;
                    Save();
                }
            }
        }
        static Prefs()
        {
            Load();
        }

        static void Save()
        {
            var json = JsonSerializer.Serialize(prefsData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("prefs.json", json);
        }

        static void Load()
        {
            lock (lockObj)
            {
                if (File.Exists("prefs.json"))
                {
                    var json = File.ReadAllText("prefs.json");
                    prefsData = JsonSerializer.Deserialize<PrefsData>(json) ?? new PrefsData();
                }
                else
                {
                    prefsData = new PrefsData();
                }
            }
        }

        public static bool AddTicker(string ticker)
        {
            lock (lockObj)
            {
                if (prefsData.tickers.Contains(ticker.ToUpperInvariant()))
                { return false; }
                prefsData.tickers.Add(ticker.ToUpperInvariant());
                Save();
            }
            OnTickerAdded?.Invoke(ticker);
            return true;
        }

        public static bool RemoveTicker(string ticker)
        {
            lock (lockObj)
            {
                if (!prefsData.tickers.Contains(ticker.ToUpperInvariant()))
                { return false; }
                prefsData.tickers.Remove(ticker.ToUpperInvariant());
                Save();
            }
            OnTickerRemoved?.Invoke(ticker);
            return true;
        }
        public static IEnumerable<string> GetTickers()
        {
            return prefsData.tickers;
        }

        public class PrefsData
        {
            [JsonInclude]
            public bool muteTickerLogging;
            [JsonInclude]
            public List<string> tickers;
            public PrefsData()
            {
                tickers = new List<string>();
            }
        }
    }
}

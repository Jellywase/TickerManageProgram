using System.Text.Json;
using System.Text.Json.Serialization;

namespace TickerManageProgram
{
    internal class FormState
    {
        StateData stateData;

        readonly string directory;
        readonly string fileName;
        readonly object fileLock = new();
        string path => Path.Combine(directory, fileName + ".json");
        public DateTime latestDate => stateData.latestDate;
        public HashSet<string> accessionNumbers => stateData.accessionNumbers;

        public FormState(string directory, string fileName)
        {
            this.directory = directory;
            this.fileName = fileName;
            LoadState();
        }

        public void UpdateLatestForm(DateTime newLatestDate, string accessionNumber)
        {
            if (newLatestDate == latestDate)
            {
                stateData.accessionNumbers.Add(accessionNumber);
            }
            else if (newLatestDate > latestDate)
            {
                stateData.accessionNumbers.Clear();
                stateData.accessionNumbers.Add(accessionNumber);
                stateData.latestDate = newLatestDate;
            }
            SaveState();
        }

        void SaveState()
        {
            lock (fileLock)
            {
                var json = JsonSerializer.Serialize(stateData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
        }
        void LoadState()
        {
            if (!File.Exists(path))
            {
                stateData = new StateData();
                SaveState();
                return;
            }
            var json = File.ReadAllText(path);
            stateData = JsonSerializer.Deserialize<StateData>(json) ?? new StateData();
        }
    }
    public class StateData
    {
        [JsonInclude]
        public DateTime latestDate;
        [JsonInclude]
        public HashSet<string> accessionNumbers;
        public StateData()
        {
            latestDate = DateTime.MinValue;
            accessionNumbers = new HashSet<string>();
        }
    }
}

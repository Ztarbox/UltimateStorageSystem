namespace UltimateStorageSystem.Tools
{
    public class Change
    {
        public string LogName { get; set; } = null!;

        public string Action { get; set; } = null!;

        public string Target { get; set; } = null!;

        public List<string> TargetField { get; set; } = new List<string>();

        public Dictionary<string, Entry> Entries { get; set; } = new Dictionary<string, Entry>();
    }
}
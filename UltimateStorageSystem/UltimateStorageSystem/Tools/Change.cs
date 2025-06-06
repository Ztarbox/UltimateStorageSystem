namespace UltimateStorageSystem.Tools
{
    public class Change
    {
        public string LogName { get; set; }

        public string Action { get; set; }

        public string Target { get; set; }

        public List<string> TargetField { get; set; }

        public Dictionary<string, Entry> Entries { get; set; }
    }
}
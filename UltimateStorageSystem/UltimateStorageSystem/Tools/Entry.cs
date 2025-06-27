namespace UltimateStorageSystem.Tools
{
    public class Entry
    {
        public string ItemId { get; set; } = null!;

        public string Description { get; set; } = null!;

        public bool IsRecipe { get; set; }

        public int Price { get; set; }
    }
}
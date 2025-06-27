using StardewValley;

namespace UltimateStorageSystem.Utilities
{
    public class ItemEntry
    {
        public string Name { get; set; }

        public int Quantity { get; set; }

        public int SingleValue { get; set; }

        public int TotalValue { get; set; }

        public Item? Item { get; set; }

        public ItemEntry(string name, int quantity, int singleValue, int totalValue, Item? item = null)
        {
            this.Name = name;
            this.Quantity = quantity;
            this.SingleValue = ((singleValue >= 0) ? singleValue : 0);
            this.TotalValue = ((totalValue >= 0) ? totalValue : 0);
            this.Item = item;
        }
    }
}
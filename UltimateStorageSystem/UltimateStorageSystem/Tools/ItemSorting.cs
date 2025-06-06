using UltimateStorageSystem.Utilities;

namespace UltimateStorageSystem.Tools
{
    public static class ItemSorting
    {
        public static List<ItemEntry> SortItems(List<ItemEntry> items, string criteria, bool ascending)
        {
            return criteria switch
            {
                "Name" => ascending ? items.OrderBy((ItemEntry e) => e.Name).ToList() : items.OrderByDescending((ItemEntry e) => e.Name).ToList(),
                "Quantity" => ascending ? items.OrderBy((ItemEntry e) => e.Quantity).ToList() : items.OrderByDescending((ItemEntry e) => e.Quantity).ToList(),
                "SingleValue" => ascending ? items.OrderBy((ItemEntry e) => e.SingleValue).ToList() : items.OrderByDescending((ItemEntry e) => e.SingleValue).ToList(),
                "TotalValue" => ascending ? items.OrderBy((ItemEntry e) => e.TotalValue).ToList() : items.OrderByDescending((ItemEntry e) => e.TotalValue).ToList(),
                _ => items,
            };
        }
    }
}
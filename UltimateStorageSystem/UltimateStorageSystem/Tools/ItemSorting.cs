using UltimateStorageSystem.Utilities;

namespace UltimateStorageSystem.Tools
{
    public static class ItemSorting
    {
        public static List<ItemEntry> SortItems(List<ItemEntry> items, string criteria, bool ascending)
        {
            return criteria switch
            {
                "Name" => ascending ? items.OrderBy(e => e.Name).ToList() : items.OrderByDescending(e => e.Name).ToList(),
                "Quantity" => ascending ? items.OrderBy(e => e.Quantity).ToList() : items.OrderByDescending(e => e.Quantity).ToList(),
                "SingleValue" => ascending ? items.OrderBy(e => e.SingleValue).ToList() : items.OrderByDescending(e => e.SingleValue).ToList(),
                "TotalValue" => ascending ? items.OrderBy(e => e.TotalValue).ToList() : items.OrderByDescending(e => e.TotalValue).ToList(),
                _ => items,
            };
        }
    }
}
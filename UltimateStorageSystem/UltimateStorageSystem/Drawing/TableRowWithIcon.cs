using StardewValley;

namespace UltimateStorageSystem.Drawing
{
    public class TableRowWithIcon
    {
        public Item ItemIcon { get; set; }

        public List<string> Cells { get; set; }

        public int CraftingQuantity { get; set; }

        public TableRowWithIcon(Item itemIcon, List<string> cells, int craftingQuantity = 0)
        {
            ItemIcon = itemIcon;
            Cells = cells;
            CraftingQuantity = craftingQuantity;
        }
    }
}
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using UltimateStorageSystem.Drawing;
using UltimateStorageSystem.Utilities;

namespace UltimateStorageSystem.Tools
{
    public class ItemTransferManager
    {
        private readonly List<Chest> chests;

        private readonly DynamicTable itemTable;

        public ItemTransferManager(List<Chest> chests, DynamicTable itemTable)
        {
            this.chests = chests;
            this.itemTable = itemTable;
        }

        public void UpdateChestItemsAndSort()
        {
            itemTable.ClearItems();
            Dictionary<string, ItemEntry> groupedItems = new();
            foreach (Chest chest in chests)
            {
                foreach (Item item in chest.Items)
                {
                    if (item != null)
                    {
                        string key = $"{item.DisplayName}_{item.Category}_{item.Quality}_{item.ParentSheetIndex}";
                        int itemPrice = item.sellToStorePrice(-1L);
                        if (groupedItems.ContainsKey(key))
                        {
                            groupedItems[key].Quantity += item.Stack;
                            groupedItems[key].TotalValue += Math.Max(0, itemPrice) * item.Stack;
                        }
                        else
                        {
                            groupedItems[key] = new ItemEntry(item.DisplayName, item.Stack, Math.Max(0, itemPrice), Math.Max(0, itemPrice) * item.Stack, item);
                        }
                    }
                }
            }
            List<ItemEntry> sortedItems = groupedItems.Values.ToList();
            sortedItems = ItemSorting.SortItems(sortedItems, itemTable.sortedColumn, itemTable.isAscending);
            foreach (ItemEntry entry in sortedItems)
            {
                itemTable.AddItem(entry);
            }
            itemTable.Refresh();
            itemTable.SortItemsBy(itemTable.sortedColumn, itemTable.isAscending);
        }

        private List<Item> CollectItemsFromChests(Item item, int amount)
        {
            List<Item> collectedItems = new();
            int remainingAmount = amount;
            var sortedChests = (from chest2 in chests
                                select new
                                {
                                    Chest = chest2,
                                    ItemCount = chest2.Items.Where(item2 => item2?.canStackWith(item) ?? false).Sum(item2 => item2.Stack)
                                } into anon
                                where anon.ItemCount > 0
                                orderby anon.ItemCount
                                select anon).ToList();
            foreach (var chestInfo in sortedChests)
            {
                Chest chest = chestInfo.Chest;
                for (int i = chest.Items.Count - 1; i >= 0; i--)
                {
                    Item chestItem = chest.Items[i];
                    if (chestItem != null && chestItem.canStackWith(item))
                    {
                        int transferAmount = Math.Min(chestItem.Stack, remainingAmount);
                        if (chestItem.Stack - transferAmount <= 0 && chestItem.Stack > 1 && chest == sortedChests.Last().Chest)
                        {
                            transferAmount = chestItem.Stack - 1;
                        }
                        remainingAmount -= transferAmount;
                        Item collectedItem = chestItem.getOne();
                        collectedItem.Stack = transferAmount;
                        collectedItems.Add(collectedItem);
                        chestItem.Stack -= transferAmount;
                        if (chestItem.Stack <= 0)
                        {
                            chest.Items.RemoveAt(i);
                        }
                        if (remainingAmount <= 0)
                        {
                            break;
                        }
                    }
                }
                if (remainingAmount <= 0)
                {
                    break;
                }
            }
            return collectedItems;
        }

        public void TransferFromInventoryToChests(Item item, int amount)
        {
            int remainingAmount = amount;
            var sortedChests = (from chest2 in chests
                                select new
                                {
                                    Chest = chest2,
                                    ItemCount = chest2.Items.Where(i => (i?.canStackWith(item) ?? false) || i?.maximumStackSize() == 1 && (i?.QualifiedItemId == item.QualifiedItemId && (chest2.GetActualCapacity() - chest2.Items.Count) > 1)).Sum(i => i.Stack)
                                } into anon
                                where anon.ItemCount > 0
                                orderby anon.ItemCount descending
                                select anon).ToList();
            foreach (var chestInfo in sortedChests)
            {
                Chest chest = chestInfo.Chest;
                Item remainingItem = item.getOne();
                remainingItem.Stack = remainingAmount;
                remainingAmount = chest.addItem(remainingItem)?.Stack ?? 0;
                int transferredAmount = amount - remainingAmount;
                item.Stack -= transferredAmount;
                if (item.Stack <= 0)
                {
                    Game1.player.removeItemFromInventory(item);
                    break;
                }
                remainingAmount = amount - transferredAmount;
                if (remainingAmount <= 0)
                {
                    break;
                }
            }
            if (remainingAmount > 0)
            {
                Game1.addHUDMessage(new HUDMessage(ModHelper.Helper.Translation.Get("no_storage_space_message"), 3));
            }
            UpdateChestItemsAndSort();
        }

        public void TransferFromChestsToInventory(Item item, int amount)
        {
            List<Item> collectedItems = CollectItemsFromChests(item, amount);
            foreach (Item collectedItem in collectedItems)
            {
                Game1.player.addItemToInventory(collectedItem);
            }
            UpdateChestItemsAndSort();
        }

        public void HandleLeftClick(Item item, bool isInInventory, bool shiftPressed)
        {
            if (item is Furniture || item is Ring || item is MeleeWeapon || item is Tool || item is Boots)
            {
                if (!isInInventory)
                {
                    Chest sourceChest = chests.FirstOrDefault((Chest chest) => chest.Items.Contains(item));
                    if (sourceChest != null)
                    {
                        sourceChest.Items.Remove(item);
                        Game1.player.addItemToInventory(item);
                    }
                }
                else
                {
                    TransferFromInventoryToChests(item, item.Stack);
                }
            }
            else if (isInInventory)
            {
                int amountToTransfer = (shiftPressed ? Math.Max(1, item.Stack / 2) : item.Stack);
                TransferFromInventoryToChests(item, amountToTransfer);
            }
            else
            {
                ItemEntry entry = itemTable.GetItemEntries().FirstOrDefault((ItemEntry e) => e.Item == item);
                int amountToTransfer;
                if (entry != null)
                {
                    int maxStackSize = item.maximumStackSize();
                    int stackSize = Math.Min(maxStackSize / 2, entry.Quantity / 2);
                    amountToTransfer = (shiftPressed ? stackSize : Math.Min(entry.Quantity, maxStackSize - 1));
                }
                else
                {
                    amountToTransfer = (shiftPressed ? Math.Max(1, item.Stack / 2) : item.Stack);
                }
                TransferFromChestsToInventory(item, amountToTransfer);
            }
            UpdateChestItemsAndSort();
            itemTable.Refresh();
        }

        public void HandleRightClick(Item item, bool isInInventory, bool shiftPressed)
        {
            int amountToTransfer = ((!shiftPressed) ? 1 : 10);
            ItemEntry entry = itemTable.GetItemEntries().FirstOrDefault((ItemEntry e) => e.Item == item);
            if (entry != null)
            {
                int maxStackSize = item.maximumStackSize();
                amountToTransfer = Math.Min(amountToTransfer, Math.Min(entry.Quantity, maxStackSize - 1));
            }
            if (item.maximumStackSize() == 1)
            {
                if (!isInInventory)
                {
                    Chest sourceChest = chests.FirstOrDefault((Chest chest) => chest.Items.Contains(item));
                    if (sourceChest != null)
                    {
                        sourceChest.Items.Remove(item);
                        Game1.player.addItemToInventory(item);
                    }
                }
                else
                {
                    TransferFromInventoryToChests(item, amountToTransfer);
                }
            }
            else if (isInInventory)
            {
                TransferFromInventoryToChests(item, amountToTransfer);
            }
            else
            {
                TransferFromChestsToInventory(item, amountToTransfer);
            }
            UpdateChestItemsAndSort();
            itemTable.Refresh();
        }
    }
}
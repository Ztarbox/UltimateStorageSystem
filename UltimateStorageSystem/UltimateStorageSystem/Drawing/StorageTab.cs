using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using UltimateStorageSystem.Tools;
using UltimateStorageSystem.Utilities;

namespace UltimateStorageSystem.Drawing
{
    public class StorageTab : IClickableMenu
    {
        private InventoryMenu playerInventoryMenu;

        private readonly FarmLinkTerminalMenu terminalMenu;

        private Scrollbar scrollbar;

        public InputHandler? inputHandler;

        private ItemTransferManager itemTransferManager;

        private List<Item> unsortedItems;

        private List<ItemEntry> aggregatedItems;

        private int computerMenuHeight;

        private int inventoryMenuWidth;

        private int inventoryMenuHeight = 280;

        private List<string> columnHeaders = new();

        public DynamicTable ItemTable { get; set; }

        public StorageTab(int xPositionOnScreen, int yPositionOnScreen, int containerWidth, int containerHeight, FarmLinkTerminalMenu terminalMenu)
            : base(xPositionOnScreen, yPositionOnScreen, containerWidth, containerHeight)
        {
            this.terminalMenu = terminalMenu;
            this.computerMenuHeight = containerHeight - this.inventoryMenuHeight;
            (List<Item>, List<ItemEntry>) storageData = this.CollectAllChestItems();
            this.unsortedItems = storageData.Item1;
            this.aggregatedItems = storageData.Item2;
            this.columnHeaders = new List<string>
            {
                ModHelper.Helper.Translation.Get("column.item"),
                ModHelper.Helper.Translation.Get("column.qty"),
                ModHelper.Helper.Translation.Get("column.value"),
                ModHelper.Helper.Translation.Get("column.total")
            };
            List<int> columnWidths = new() { 400, 150, 110, 200 };
            List<bool> columnAlignments = new() { false, true, true, true };
            List<TableRowWithIcon> tableRows = this.aggregatedItems.Select(entry => new TableRowWithIcon(entry.Item, new List<string>
            {
                entry.Name,
                entry.Quantity.ToString(),
                entry.SingleValue.ToString(),
                entry.TotalValue.ToString()
            })).ToList();
            this.ItemTable = new DynamicTable(xPositionOnScreen + 30, yPositionOnScreen + 40, this.columnHeaders, columnWidths, columnAlignments, tableRows, null);
            this.scrollbar = new Scrollbar(xPositionOnScreen + containerWidth - 50, yPositionOnScreen + 103, this.ItemTable);
            this.ItemTable.Scrollbar = this.scrollbar;
            List<Chest> chests = terminalMenu.GetAllStorageObjects();
            this.itemTransferManager = new ItemTransferManager(chests, this.ItemTable);
            this.itemTransferManager.UpdateChestItemsAndSort();
            this.scrollbar.UpdateScrollBarPosition();
            int slotsPerRow = 12;
            int slotSize = 64;
            this.inventoryMenuWidth = slotsPerRow * slotSize;
            int inventoryMenuX = base.xPositionOnScreen + (containerWidth - this.inventoryMenuWidth) / 2 - 90;
            int inventoryMenuY = base.yPositionOnScreen + this.computerMenuHeight + 70;
            this.playerInventoryMenu = new InventoryMenu(inventoryMenuX, inventoryMenuY, playerInventory: false);
        }

        private bool IsBlockedChest(Chest chest)
        {
            return chest.Items.Any(item => item is StardewValley.Object obj && obj.QualifiedItemId == "(BC)holybananapants.UltimateStorageSystemContentPack_BlockTerminal");
        }

        private (List<Item> unsortedItems, List<ItemEntry> aggregatedItems) CollectAllChestItems()
        {
            List<Item> unsortedItems = new();
            Dictionary<string, ItemEntry> itemDictionary = new();
            foreach (GameLocation location in Game1.locations)
            {
                foreach (StardewValley.Object obj in location.Objects.Values)
                {
                    if (!(obj is Chest chest) || chest.Items.Count <= 0 || this.IsBlockedChest(chest))
                    {
                        continue;
                    }
                    foreach (Item item in chest.Items)
                    {
                        if (item != null)
                        {
                            unsortedItems.Add(item);
                            string key = item.DisplayName;
                            if (itemDictionary.ContainsKey(key))
                            {
                                itemDictionary[key].Quantity += item.Stack;
                                itemDictionary[key].TotalValue += item.salePrice() * item.Stack;
                            }
                            else
                            {
                                itemDictionary[key] = new ItemEntry(item.DisplayName, item.Stack, item.salePrice(), item.salePrice() * item.Stack, item);
                            }
                        }
                    }
                }
                if (!(location is FarmHouse farmHouse))
                {
                    continue;
                }
                Chest fridgeChest = farmHouse.fridge.Value;
                if (fridgeChest == null || this.IsBlockedChest(fridgeChest))
                {
                    continue;
                }
                foreach (Item item2 in fridgeChest.Items)
                {
                    if (item2 != null)
                    {
                        unsortedItems.Add(item2);
                        if (itemDictionary.ContainsKey(item2.DisplayName))
                        {
                            itemDictionary[item2.DisplayName].Quantity += item2.Stack;
                            itemDictionary[item2.DisplayName].TotalValue += item2.salePrice() * item2.Stack;
                        }
                        else
                        {
                            itemDictionary[item2.DisplayName] = new ItemEntry(item2.DisplayName, item2.Stack, item2.salePrice(), item2.salePrice() * item2.Stack, item2);
                        }
                    }
                }
            }
            List<ItemEntry> aggregatedItems = itemDictionary.Values.ToList();
            aggregatedItems = (from e in aggregatedItems
                               orderby e.Name, (e.Item as StardewValley.Object)?.Quality ?? 0 descending
                               select e).ToList();
            return (unsortedItems: unsortedItems, aggregatedItems: aggregatedItems);
        }

        public void TransferItemToPlayerInventory(Item item)
        {
            this.itemTransferManager.TransferFromChestsToInventory(item, item.Stack);
        }

        public void TransferItemToChest(Item item)
        {
            this.itemTransferManager.TransferFromInventoryToChests(item, item.Stack);
        }

        public override void draw(SpriteBatch b)
        {
            List<string> list = new()
            {
                ModHelper.Helper.Translation.Get("column.item") ?? "Item",
                ModHelper.Helper.Translation.Get("column.item") ?? "Qty",
                ModHelper.Helper.Translation.Get("column.item") ?? "Value",
                ModHelper.Helper.Translation.Get("column.item") ?? "Total"
            };
            this.columnHeaders = list;
            base.draw(b);
            int fixedWidth = 1000;
            int upperFrameHeight = 620;
            int inventoryFrameHeight = 280;
            int tableWidth = this.ItemTable.ColumnWidths.Sum() + (this.ItemTable.ColumnWidths.Count - 1) * 10;
            string title = "ULTIMATE STORAGE SYSTEM";
            float scale = 0.8f;
            Vector2 titleSize = Game1.dialogueFont.MeasureString(title) * scale;
            Vector2 titlePosition = new(this.xPositionOnScreen + tableWidth - titleSize.X + 75f, this.yPositionOnScreen + 30);
            Color titleColor = Color.Orange;
            Color titleShadowColor = Color.Brown;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    b.DrawString(Game1.dialogueFont, title, titlePosition + new Vector2(dx + 3, dy + 3), titleShadowColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0.86f);
                }
            }
            b.DrawString(Game1.dialogueFont, title, titlePosition, titleColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0.86f);
            this.ItemTable.Draw(b);
            IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen + upperFrameHeight - 60, fixedWidth, inventoryFrameHeight - 30, Color.White);
            this.playerInventoryMenu.draw(b);
            this.drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            bool shiftPressed = Game1.oldKBState.IsKeyDown(Keys.LeftShift) || Game1.oldKBState.IsKeyDown(Keys.RightShift);
            var clickedItem = this.ItemTable.GetClickedItem(x, y);
            if (clickedItem != null)
            {
                this.itemTransferManager.HandleLeftClick(clickedItem, isInInventory: false, shiftPressed);
                return;
            }
            foreach (ClickableComponent slot in this.playerInventoryMenu.inventory)
            {
                if (slot.containsPoint(x, y) && this.playerInventoryMenu.actualInventory.Count > slot.myID)
                {
                    Item inventoryItem = this.playerInventoryMenu.actualInventory[slot.myID];
                    if (inventoryItem != null)
                    {
                        this.itemTransferManager.HandleLeftClick(inventoryItem, isInInventory: true, shiftPressed);
                    }
                    break;
                }
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            base.receiveRightClick(x, y, playSound);
            bool shiftPressed = Game1.oldKBState.IsKeyDown(Keys.LeftShift) || Game1.oldKBState.IsKeyDown(Keys.RightShift);
            var clickedItem = this.ItemTable.GetClickedItem(x, y);
            if (clickedItem != null)
            {
                this.itemTransferManager.HandleRightClick(clickedItem, isInInventory: false, shiftPressed);
                return;
            }
            foreach (ClickableComponent slot in this.playerInventoryMenu.inventory)
            {
                if (slot.containsPoint(x, y) && this.playerInventoryMenu.actualInventory.Count > slot.myID)
                {
                    Item inventoryItem = this.playerInventoryMenu.actualInventory[slot.myID];
                    if (inventoryItem != null)
                    {
                        this.itemTransferManager.HandleRightClick(inventoryItem, isInInventory: true, shiftPressed);
                    }
                    break;
                }
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            this.ItemTable.ReceiveScrollWheelAction(direction);
            this.scrollbar.ReceiveScrollWheelAction(direction);
            this.scrollbar.UpdateScrollBarPosition();
        }

        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);
        }

        public void ResetSort()
        {
            this.ItemTable?.ResetSort();
        }

        public override void update(GameTime time)
        {
            base.update(time);
            this.ItemTable.Update();
            this.scrollbar.UpdateScrollBarPosition();
        }

        public void RefreshItems()
        {
            List<Chest> chests = this.terminalMenu.GetAllStorageObjects();
            Dictionary<string, ItemEntry> itemDict = new();
            foreach (Chest chest in chests)
            {
                foreach (Item item in chest.Items)
                {
                    if (item != null)
                    {
                        string name = item.DisplayName;
                        int price = Math.Max(0, item.sellToStorePrice(-1L));
                        int stack = item.Stack;
                        if (itemDict.TryGetValue(name, out var entry))
                        {
                            entry.Quantity += stack;
                            entry.TotalValue += price * stack;
                        }
                        else
                        {
                            itemDict[name] = new ItemEntry(name, stack, price, price * stack, item);
                        }
                    }
                }
            }
            this.ItemTable.ClearItems();
            foreach (ItemEntry entry2 in itemDict.Values)
            {
                this.ItemTable.AddItem(entry2);
            }
            this.ItemTable.Refresh();
            this.scrollbar.UpdateScrollBarPosition();
        }
    }
}
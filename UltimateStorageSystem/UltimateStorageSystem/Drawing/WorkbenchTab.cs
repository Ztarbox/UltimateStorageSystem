#define DEBUG
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Objects;
using System.Diagnostics;
using UltimateStorageSystem.Tools;
using UltimateStorageSystem.Utilities;

namespace UltimateStorageSystem.Drawing
{
    public class WorkbenchTab : IClickableMenu
    {
        private InventoryMenu playerInventoryMenu;

        private ItemTransferManager transferManager;

        private Scrollbar scrollbar;

        private int containerWidth;

        private int containerHeight;

        private int computerMenuHeight;

        private int inventoryMenuWidth;

        private int inventoryMenuHeight = 280;

        private List<CraftingRecipe> craftingRecipes;

        private Dictionary<string, string> objectInformation;

        public InputHandler inputHandler;

        public bool craftMode = false;

        private CraftingRecipe currentRecipe = null;

        private int maxCraftable = 0;

        private int craftAmount = 1;

        private const string BlockerQualifiedId = "(BC)holybananapants.UltimateStorageSystemContentPack_BlockTerminal";

        public DynamicTable CraftingTable { get; private set; }

        public FarmLinkTerminalMenu TerminalMenu { get; set; }

        public WorkbenchTab(int xPositionOnScreen, int yPositionOnScreen, int containerWidth, int containerHeight, FarmLinkTerminalMenu terminalMenu, Dictionary<string, string> objectInformation = null)
            : base(xPositionOnScreen, yPositionOnScreen, containerWidth, containerHeight)
        {
            TerminalMenu = terminalMenu;
            this.containerWidth = containerWidth;
            this.containerHeight = containerHeight;
            computerMenuHeight = containerHeight - inventoryMenuHeight;
            int slotsPerRow = 12;
            int slotSize = 64;
            inventoryMenuWidth = slotsPerRow * slotSize;
            int inventoryMenuX = base.xPositionOnScreen + (containerWidth - inventoryMenuWidth) / 2 - 90;
            int inventoryMenuY = base.yPositionOnScreen + computerMenuHeight + 70;
            playerInventoryMenu = new InventoryMenu(inventoryMenuX, inventoryMenuY, playerInventory: false);
            craftingRecipes = (from name in CraftingRecipe.craftingRecipes.Keys
                               where Game1.player.craftingRecipes.ContainsKey(name)
                               select new CraftingRecipe(name) into recipe
                               where recipe != null
                               select recipe).ToList();
            this.objectInformation = objectInformation;
            craftingRecipes.Add(new CraftingRecipeForBlockTerminal());
            List<string> columnHeaders = new()
            {
                ModHelper.Helper.Translation.Get("column.craft_item"),
                ModHelper.Helper.Translation.Get("column.type"),
                ModHelper.Helper.Translation.Get("column.material"),
                ModHelper.Helper.Translation.Get("column.max")
            };
            List<int> columnWidths = new() { 300, 160, 300, 100 };
            List<bool> columnAlignments = new() { false, false, false, true };
            List<TableRowWithIcon> tableRows = GenerateCraftingData();
            CraftingTable = new DynamicTable(xPositionOnScreen + 30, yPositionOnScreen + 40, columnHeaders, columnWidths, columnAlignments, tableRows, scrollbar, showCraftingQuantity: true);
            scrollbar = new Scrollbar(xPositionOnScreen + containerWidth - 50, yPositionOnScreen + 103, CraftingTable);
            CraftingTable.scrollbar = scrollbar;
            transferManager = new ItemTransferManager(terminalMenu.GetAllStorageObjects(), CraftingTable);
        }

        private List<TableRowWithIcon> GenerateCraftingData()
        {
            List<TableRowWithIcon> rows = new();
            foreach (CraftingRecipe recipe in craftingRecipes)
            {
                Item item = recipe.createItem();
                string itemName = item.DisplayName;
                string itemType = item.getCategoryName();
                string ingredients = GetIngredientsString(recipe);
                string maxQuantity = CalculateMaxCraftable(recipe).ToString();
                Debug.WriteLine("Item: " + itemName + ", Ingredients: " + ingredients);
                rows.Add(new TableRowWithIcon(item, new List<string> { itemName, itemType, ingredients, maxQuantity }));
            }
            return rows;
        }

        public void ResetSort()
        {
            CraftingTable?.ResetSort();
        }

        private string GetIngredientsString(CraftingRecipe recipe)
        {
            List<string> ingredientList = new();
            foreach (KeyValuePair<string, int> ingredient in recipe.recipeList)
            {
                string ingredientName = recipe.getNameFromIndex(ingredient.Key);
                int requiredAmount = ingredient.Value;
                ingredientList.Add($"{ingredientName} ({requiredAmount})");
            }
            return string.Join(", ", ingredientList);
        }

        private int CalculateMaxCraftable(CraftingRecipe recipe)
        {
            List<Chest> chests = TerminalMenu?.GetAllStorageObjects();
            return recipe.getCraftableCount(chests);
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            int fixedWidth = 1000;
            int upperFrameHeight = 620;
            int inventoryFrameHeight = 280;
            int tableWidth = CraftingTable.ColumnWidths.Sum() + (CraftingTable.ColumnWidths.Count - 1) * 10;
            string title = "ULTIMATE CRAFTING SYSTEM";
            float scale = 0.8f;
            Vector2 titleSize = Game1.dialogueFont.MeasureString(title) * scale;
            Vector2 titlePosition = new(xPositionOnScreen + tableWidth - titleSize.X + 75f, yPositionOnScreen + 30);
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
            CraftingTable.Draw(b);
            IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen + upperFrameHeight - 60, fixedWidth, inventoryFrameHeight - 30, Color.White);
            playerInventoryMenu.draw(b);
            if (craftMode && currentRecipe != null)
            {
                Vector2 mousePos = new(Game1.getMouseX(), Game1.getMouseY());
                Item icon = currentRecipe.createItem();
                icon.Stack = craftAmount;
                icon.drawInMenu(b, mousePos, 1f, 1f, 0.9f, StackDrawType.Draw, Color.White, drawShadow: false);
            }
            drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (craftMode && currentRecipe != null)
            {
                foreach (ClickableComponent slot in playerInventoryMenu.inventory)
                {
                    if (slot.containsPoint(x, y) && playerInventoryMenu.actualInventory.Count > slot.myID)
                    {
                        Item existingItem = playerInventoryMenu.actualInventory[slot.myID];
                        Item result;
                        if (currentRecipe is CraftingRecipeForBlockTerminal)
                        {
                            result = ItemRegistry.Create("(BC)holybananapants.UltimateStorageSystemContentPack_BlockTerminal");
                            result.Stack = craftAmount;
                        }
                        else
                        {
                            result = currentRecipe.createItem();
                            if (result == null)
                            {
                                Game1.playSound("cancel");
                                break;
                            }
                            result.Stack = craftAmount;
                        }
                        if (existingItem == null)
                        {
                            playerInventoryMenu.actualInventory[slot.myID] = result;
                            Game1.playSound("coin");
                        }
                        else
                        {
                            if (!existingItem.canStackWith(result))
                            {
                                Game1.playSound("cancel");
                                break;
                            }
                            int combinedStack = existingItem.Stack + result.Stack;
                            int maxStackSize = existingItem.maximumStackSize();
                            if (combinedStack <= maxStackSize)
                            {
                                existingItem.Stack = combinedStack;
                                Game1.playSound("coin");
                            }
                            else
                            {
                                int availableSpace = maxStackSize - existingItem.Stack;
                                if (availableSpace <= 0)
                                {
                                    Game1.playSound("cancel");
                                    break;
                                }
                                existingItem.Stack += availableSpace;
                                result.Stack -= availableSpace;
                                Game1.playSound("coin");
                            }
                        }
                        List<IInventory> inventories = TerminalMenu.GetAllStorageObjects().Select((Func<Chest, IInventory>)((Chest ch) => ch.Items)).ToList();
                        for (int i = 0; i < craftAmount; i++)
                        {
                            currentRecipe.consumeIngredients(inventories);
                        }
                        UpdateCraftingTable();
                        craftMode = false;
                        break;
                    }
                }
                return;
            }
            base.receiveLeftClick(x, y, playSound);
            scrollbar.ReceiveLeftClick(x, y);
            Item clicked = CraftingTable.GetClickedItem(x, y);
            if (clicked == null || TerminalMenu == null)
            {
                return;
            }
            CraftingRecipe recipe = null;
            recipe = ((!(clicked.QualifiedItemId == "(BC)holybananapants.UltimateStorageSystemContentPack_BlockTerminal")) ? craftingRecipes.FirstOrDefault((CraftingRecipe r) => r.name == clicked.Name || r.DisplayName == clicked.DisplayName) : craftingRecipes.OfType<CraftingRecipeForBlockTerminal>().FirstOrDefault());
            if (recipe == null)
            {
                return;
            }
            int maxCanCraft = recipe.getCraftableCount(TerminalMenu.GetAllStorageObjects());
            if (maxCanCraft <= 0)
            {
                Game1.addHUDMessage(new HUDMessage(ModHelper.Helper.Translation.Get("not_enough_ingredients"), 3));
                return;
            }
            bool shift = Game1.oldKBState.IsKeyDown(Keys.LeftShift) || Game1.oldKBState.IsKeyDown(Keys.RightShift);
            craftAmount = ((!shift) ? 1 : Math.Min(5, maxCanCraft));
            List<IInventory> chestInventories = TerminalMenu.GetAllStorageObjects().Select((Func<Chest, IInventory>)((Chest chest) => chest.Items)).ToList();
            for (int i2 = 0; i2 < craftAmount; i2++)
            {
                recipe.consumeIngredients(chestInventories);
            }
            for (int i3 = 0; i3 < craftAmount; i3++)
            {
                Item result2 = ((!(clicked.QualifiedItemId == "(BC)holybananapants.UltimateStorageSystemContentPack_BlockTerminal")) ? recipe.createItem() : ItemRegistry.Create("(BC)holybananapants.UltimateStorageSystemContentPack_BlockTerminal"));
                if (result2 != null)
                {
                    Game1.player.addItemToInventory(result2);
                }
                else
                {
                    Game1.playSound("cancel");
                }
            }
            Game1.playSound("coin");
            UpdateCraftingTable();
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (!craftMode)
            {
                Item clicked = CraftingTable.GetClickedItem(x, y);
                if (clicked != null)
                {
                    if (clicked.QualifiedItemId == "(BC)holybananapants.UltimateStorageSystemContentPack_BlockTerminal")
                    {
                        currentRecipe = craftingRecipes.OfType<CraftingRecipeForBlockTerminal>().FirstOrDefault();
                    }
                    else
                    {
                        currentRecipe = craftingRecipes.FirstOrDefault((CraftingRecipe r) => r.name == clicked.Name || r.DisplayName == clicked.DisplayName);
                    }
                    if (currentRecipe != null)
                    {
                        int max = currentRecipe.getCraftableCount(TerminalMenu.GetAllStorageObjects());
                        if (max > 0)
                        {
                            craftMode = true;
                            maxCraftable = max;
                            craftAmount = 1;
                            Game1.playSound("shiny4");
                        }
                        else
                        {
                            Game1.addHUDMessage(new HUDMessage(ModHelper.Helper.Translation.Get("not_enough_ingredients"), 3));
                        }
                        return;
                    }
                }
            }
            base.receiveRightClick(x, y, playSound);
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (craftMode)
            {
                craftAmount = Math.Clamp(craftAmount + ((direction > 0) ? 1 : (-1)), 1, maxCraftable);
                return;
            }
            CraftingTable.ReceiveScrollWheelAction(direction);
            scrollbar.UpdateScrollBarPosition();
        }

        public void LeftClickHeld(int x, int y)
        {
            CraftingTable.PerformHoverAction(x, y);
            scrollbar.LeftClickHeld(x, y);
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            CraftingTable.PerformHoverAction(x, y);
        }

        public override void receiveKeyPress(Keys key)
        {
            if (craftMode && key == Keys.Escape)
            {
                craftMode = false;
                Game1.playSound("cancel");
            }
            else
            {
                base.receiveKeyPress(key);
            }
        }

        public void CancelCraftMode()
        {
            craftMode = false;
            currentRecipe = null;
            craftAmount = 1;
            maxCraftable = 0;
        }

        private void UpdateCraftingTable()
        {
            string sortCol = CraftingTable.sortedColumn;
            bool asc = CraftingTable.isAscending;
            int scroll = CraftingTable.ScrollIndex;
            List<TableRowWithIcon> newRows = GenerateCraftingData();
            CraftingTable.ClearItems();
            foreach (TableRowWithIcon row in newRows)
            {
                CraftingTable.AllRows.Add(row);
            }
            CraftingTable.Refresh();
            CraftingTable.SortItemsBy(sortCol, asc);
            CraftingTable.ScrollIndex = Math.Clamp(scroll, 0, Math.Max(0, CraftingTable.GetItemEntriesCount() - CraftingTable.GetVisibleRows()));
            CraftingTable.scrollbar.UpdateScrollBarPosition();
        }

        public override void update(GameTime time)
        {
            base.update(time);
            CraftingTable.Update();
            scrollbar.UpdateScrollBarPosition();
        }
    }
}
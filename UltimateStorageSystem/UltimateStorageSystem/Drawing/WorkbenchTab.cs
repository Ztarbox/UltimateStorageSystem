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
        private readonly InventoryMenu playerInventoryMenu;

        private readonly ItemTransferManager transferManager;

        private readonly Scrollbar scrollbar;

        private readonly List<CraftingRecipe> craftingRecipes;

        public InputHandler? inputHandler;

        public bool craftMode = false;

        private CraftingRecipe? currentRecipe = null;

        private int maxCraftable = 0;

        private int craftAmount = 1;

        private const string BlockerQualifiedId = "(BC)holybananapants.UltimateStorageSystemContentPack_BlockTerminal";

        public DynamicTable CraftingTable { get; private set; }

        public FarmLinkTerminalMenu TerminalMenu { get; set; }

        public WorkbenchTab(int xPositionOnScreen, int yPositionOnScreen, int containerWidth, int containerHeight, FarmLinkTerminalMenu terminalMenu, InventoryMenu inventoryMenu)
            : base(xPositionOnScreen, yPositionOnScreen, containerWidth, containerHeight)
        {
            this.TerminalMenu = terminalMenu;
            this.playerInventoryMenu = inventoryMenu;
            this.craftingRecipes = (from name in CraftingRecipe.craftingRecipes.Keys
                               where Game1.player.craftingRecipes.ContainsKey(name)
                               select new CraftingRecipe(name) into recipe
                               where recipe != null
                               select recipe).ToList();
            this.craftingRecipes.Add(new CraftingRecipeForBlockTerminal());
            List<string> columnHeaders = new()
            {
                ModHelper.Helper.Translation.Get("column.craft_item"),
                ModHelper.Helper.Translation.Get("column.type"),
                ModHelper.Helper.Translation.Get("column.material"),
                ModHelper.Helper.Translation.Get("column.max")
            };
            List<int> columnWidths = new() { 300, 160, 300, 100 };
            List<bool> columnAlignments = new() { false, false, false, true };
            List<TableRowWithIcon> tableRows = this.GenerateCraftingData();
            this.CraftingTable = new DynamicTable(xPositionOnScreen + 30, yPositionOnScreen + 40, columnHeaders, columnWidths, columnAlignments, tableRows, this.scrollbar, showCraftingQuantity: true);
            this.scrollbar = new Scrollbar(xPositionOnScreen + containerWidth - 50, yPositionOnScreen + 103, this.CraftingTable);
            this.CraftingTable.Scrollbar = this.scrollbar;
            this.transferManager = new ItemTransferManager(FarmLinkTerminalMenu.GetAllStorageObjects(), this.CraftingTable);
        }

        private List<TableRowWithIcon> GenerateCraftingData()
        {
            List<TableRowWithIcon> rows = new();
            foreach (CraftingRecipe recipe in this.craftingRecipes)
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
            this.CraftingTable?.ResetSort();
        }

        private static string GetIngredientsString(CraftingRecipe recipe)
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

        private static int CalculateMaxCraftable(CraftingRecipe recipe)
        {
            var chests = FarmLinkTerminalMenu.GetAllStorageObjects();
            return recipe.getCraftableCount(chests);
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            int fixedWidth = 1000;
            int upperFrameHeight = 620;
            int inventoryFrameHeight = 280;
            int tableWidth = this.CraftingTable.ColumnWidths.Sum() + (this.CraftingTable.ColumnWidths.Count - 1) * 10;
            string title = "ULTIMATE CRAFTING SYSTEM";
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
            this.CraftingTable.Draw(b);
            drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen + upperFrameHeight - 60, fixedWidth, inventoryFrameHeight - 30, Color.White);
            this.playerInventoryMenu.draw(b);
            if (this.craftMode && this.currentRecipe != null)
            {
                Vector2 mousePos = new(Game1.getMouseX(), Game1.getMouseY());
                Item icon = this.currentRecipe.createItem();
                icon.Stack = this.craftAmount;
                icon.drawInMenu(b, mousePos, 1f, 1f, 0.9f, StackDrawType.Draw, Color.White, drawShadow: false);
            }
            this.drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.craftMode && this.currentRecipe != null)
            {
                foreach (ClickableComponent slot in this.playerInventoryMenu.inventory)
                {
                    if (slot.containsPoint(x, y) && this.playerInventoryMenu.actualInventory.Count > slot.myID)
                    {
                        Item existingItem = this.playerInventoryMenu.actualInventory[slot.myID];
                        Item result;
                        if (this.currentRecipe is CraftingRecipeForBlockTerminal)
                        {
                            result = ItemRegistry.Create("(BC)holybananapants.UltimateStorageSystemContentPack_BlockTerminal");
                            result.Stack = this.craftAmount;
                        }
                        else
                        {
                            result = this.currentRecipe.createItem();
                            if (result == null)
                            {
                                Game1.playSound("cancel");
                                break;
                            }
                            result.Stack = this.craftAmount;
                        }
                        if (existingItem == null)
                        {
                            this.playerInventoryMenu.actualInventory[slot.myID] = result;
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
                        List<IInventory> inventories = FarmLinkTerminalMenu.GetAllStorageObjects().Select((Func<Chest, IInventory>)(ch => ch.Items)).ToList();
                        for (int i = 0; i < this.craftAmount; i++)
                        {
                            this.currentRecipe.consumeIngredients(inventories);
                        }
                        this.UpdateCraftingTable();
                        if (Game1.player.craftingRecipes.ContainsKey(this.currentRecipe.name))
                        {
                            Game1.player.craftingRecipes[this.currentRecipe.name] += this.craftAmount;
                            Game1.stats.checkForCraftingAchievements();
                        }
                        this.craftMode = false;
                        break;
                    }
                }
                return;
            }
            base.receiveLeftClick(x, y, playSound);
            this.scrollbar.ReceiveLeftClick(x, y);
            var clicked = this.CraftingTable.GetClickedItem(x, y);
            if (clicked == null || this.TerminalMenu == null)
            {
                return;
            }
            var recipe = ((!(clicked.QualifiedItemId == "(BC)holybananapants.UltimateStorageSystemContentPack_BlockTerminal")) ? this.craftingRecipes.FirstOrDefault(r => r.name == clicked.Name || r.DisplayName == clicked.DisplayName) : this.craftingRecipes.OfType<CraftingRecipeForBlockTerminal>().FirstOrDefault());
            if (recipe == null)
            {
                return;
            }
            int maxCanCraft = recipe.getCraftableCount(FarmLinkTerminalMenu.GetAllStorageObjects());
            if (maxCanCraft <= 0)
            {
                Game1.addHUDMessage(new HUDMessage(ModHelper.Helper.Translation.Get("not_enough_ingredients"), 3));
                return;
            }
            bool shift = Game1.oldKBState.IsKeyDown(Keys.LeftShift) || Game1.oldKBState.IsKeyDown(Keys.RightShift);
            this.craftAmount = ((!shift) ? 1 : Math.Min(5, maxCanCraft));
            List<IInventory> chestInventories = FarmLinkTerminalMenu.GetAllStorageObjects().Select((Func<Chest, IInventory>)(chest => chest.Items)).ToList();
            for (int i2 = 0; i2 < this.craftAmount; i2++)
            {
                recipe.consumeIngredients(chestInventories);
            }
            for (int i3 = 0; i3 < this.craftAmount; i3++)
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
            this.UpdateCraftingTable();
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (!this.craftMode)
            {
                var clicked = this.CraftingTable.GetClickedItem(x, y);
                if (clicked != null)
                {
                    if (clicked.QualifiedItemId == "(BC)holybananapants.UltimateStorageSystemContentPack_BlockTerminal")
                    {
                        this.currentRecipe = this.craftingRecipes.OfType<CraftingRecipeForBlockTerminal>().FirstOrDefault();
                    }
                    else
                    {
                        this.currentRecipe = this.craftingRecipes.FirstOrDefault(r => r.name == clicked.Name || r.DisplayName == clicked.DisplayName);
                    }
                    if (this.currentRecipe != null)
                    {
                        int max = this.currentRecipe.getCraftableCount(FarmLinkTerminalMenu.GetAllStorageObjects());
                        if (max > 0)
                        {
                            this.craftMode = true;
                            this.maxCraftable = max;
                            this.craftAmount = 1;
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
            if (this.craftMode)
            {
                this.craftAmount = Math.Clamp(this.craftAmount + ((direction > 0) ? 1 : (-1)), 1, this.maxCraftable);
                return;
            }
            this.CraftingTable.ReceiveScrollWheelAction(direction);
            this.scrollbar.UpdateScrollBarPosition();
        }

        public void LeftClickHeld(int x, int y)
        {
            this.scrollbar.LeftClickHeld(x, y);
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
        }

        public override void receiveKeyPress(Keys key)
        {
            if (this.craftMode && key == Keys.Escape)
            {
                this.craftMode = false;
                Game1.playSound("cancel");
            }
            else
            {
                base.receiveKeyPress(key);
            }
        }

        public void CancelCraftMode()
        {
            this.craftMode = false;
            this.currentRecipe = null;
            this.craftAmount = 1;
            this.maxCraftable = 0;
        }

        private void UpdateCraftingTable()
        {
            var sortCol = this.CraftingTable.SortedColumn;
            bool asc = this.CraftingTable.isAscending;
            int scroll = this.CraftingTable.ScrollIndex;
            List<TableRowWithIcon> newRows = this.GenerateCraftingData();
            this.CraftingTable.ClearItems();
            foreach (TableRowWithIcon row in newRows)
            {
                this.CraftingTable.AllRows.Add(row);
            }
            this.CraftingTable.Refresh();
            this.CraftingTable.SortItemsBy(sortCol, asc);
            this.CraftingTable.ScrollIndex = Math.Clamp(scroll, 0, Math.Max(0, this.CraftingTable.GetItemEntriesCount() - this.CraftingTable.GetVisibleRows()));
            this.CraftingTable.Scrollbar?.UpdateScrollBarPosition();
        }

        public override void update(GameTime time)
        {
            base.update(time);
            this.CraftingTable.Update();
            this.scrollbar.UpdateScrollBarPosition();
        }
    }
}
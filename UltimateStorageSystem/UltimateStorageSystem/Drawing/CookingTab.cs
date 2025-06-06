using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Objects;
using UltimateStorageSystem.Tools;
using UltimateStorageSystem.Utilities;

namespace UltimateStorageSystem.Drawing
{
    public class CookingTab : IClickableMenu
    {
        private InventoryMenu playerInventoryMenu;

        private Scrollbar scrollbar;

        private int containerWidth;

        private int containerHeight;

        private int computerMenuHeight;

        private int inventoryMenuWidth;

        private int inventoryMenuHeight = 280;

        private List<CraftingRecipe> cookingRecipes;

        public InputHandler inputHandler;

        public bool cookMode = false;

        private CraftingRecipe currentRecipe = null;

        private int maxCookable = 0;

        private int cookAmount = 1;

        public DynamicTable CookingTable { get; private set; }

        public FarmLinkTerminalMenu TerminalMenu { get; set; }

        public CookingTab(int xPositionOnScreen, int yPositionOnScreen, int containerWidth, int containerHeight, FarmLinkTerminalMenu terminalMenu)
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
            cookingRecipes = Game1.player.cookingRecipes.Keys.Select((string name) => new CraftingRecipe(name, isCookingRecipe: true)).ToList();
            List<string> columnHeaders = new()
            {

                ModHelper.Helper.Translation.Get("column.recipe"),
                ModHelper.Helper.Translation.Get("column.buffs"),
                ModHelper.Helper.Translation.Get("column.ingredients"),
                ModHelper.Helper.Translation.Get("column.max")
            };
            List<int> columnWidths = new() { 250, 230, 280, 100 };
            List<bool> columnAlignments = new() { false, false, false, true };
            List<TableRowWithIcon> tableRows = GenerateRecipeData();
            CookingTable = new DynamicTable(xPositionOnScreen + 30, yPositionOnScreen + 40, columnHeaders, columnWidths, columnAlignments, tableRows, scrollbar);
            scrollbar = new Scrollbar(xPositionOnScreen + containerWidth - 50, yPositionOnScreen + 103, CookingTable);
            CookingTable.scrollbar = scrollbar;
        }

        public void ResetSort()
        {
            CookingTable?.ResetSort();
        }

        private List<TableRowWithIcon> GenerateRecipeData()
        {
            List<TableRowWithIcon> rows = new();
            foreach (string recipeKey in Game1.player.cookingRecipes.Keys)
            {
                CraftingRecipe craftingRecipe = new(recipeKey, isCookingRecipe: true);
                if (craftingRecipe.createItem() is StardewValley.Object { QualifiedItemId: "0" } dish)
                {
                    string recipeName = craftingRecipe.DisplayName;
                    string buffs = GetRecipeBuffs(craftingRecipe);
                    string ingredients = GetRecipeIngredients(craftingRecipe);
                    string maxQuantity = craftingRecipe.getCraftableCount(TerminalMenu.GetAllStorageObjects()).ToString();
                    rows.Add(new TableRowWithIcon(dish, new List<string> { recipeName, buffs, ingredients, maxQuantity }));
                }
            }
            return rows;
        }

        private string GetRecipeBuffs(CraftingRecipe craftingRecipe)
        {
            if (!(craftingRecipe.createItem() is StardewValley.Object cookedDish))
            {
                return "No Buffs";
            }
            List<string> buffs = new();
            int health = cookedDish.healthRecoveredOnConsumption();
            int energy = cookedDish.staminaRecoveredOnConsumption();
            if (health > 0)
            {
                buffs.Add($"Health +{health}");
            }
            if (energy > 0)
            {
                buffs.Add($"Energy +{energy}");
            }
            return (buffs.Count > 0) ? string.Join(", ", buffs) : "No Buffs";
        }

        private string GetRecipeIngredients(CraftingRecipe craftingRecipe)
        {
            IEnumerable<string> names = craftingRecipe.recipeList.Select((KeyValuePair<string, int> kv) => $"{craftingRecipe.getNameFromIndex(kv.Key)} ({kv.Value})");
            return string.Join(", ", names);
        }

        private string GetCategoryName(int categoryId)
        {
            if (1 == 0)
            {
            }
            string text = categoryId switch
            {
                -5 => "Category_Egg",
                -4 => "Category_Fish",
                -6 => "Category_Milk",
                _ => null,
            };
            if (1 == 0)
            {
            }
            string key = text;
            if (key != null)
            {
                return Game1.content.LoadString("Strings\\UI:" + key);
            }
            return Game1.content.LoadString("Strings\\UI:Category_Unknown");
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            int fixedWidth = 1000;
            int upperFrameHeight = 620;
            int inventoryFrameHeight = 280;
            int tableWidth = CookingTable.ColumnWidths.Sum() + (CookingTable.ColumnWidths.Count - 1) * 10;
            string title = "ULTIMATE COOKING SYSTEM";
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
            CookingTable.Draw(b);
            IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen + upperFrameHeight - 60, fixedWidth, inventoryFrameHeight - 30, Color.White);
            playerInventoryMenu.draw(b);
            if (cookMode && currentRecipe != null)
            {
                Vector2 mousePos = new(Game1.getMouseX(), Game1.getMouseY());
                Item icon = currentRecipe.createItem();
                icon.Stack = cookAmount;
                icon.drawInMenu(b, mousePos, 1f, 1f, 0.9f, StackDrawType.Draw, Color.White, drawShadow: false);
            }
            drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (cookMode && currentRecipe != null)
            {
                foreach (ClickableComponent slot in playerInventoryMenu.inventory)
                {
                    if (slot.containsPoint(x, y) && playerInventoryMenu.actualInventory.Count > slot.myID)
                    {
                        Item existingItem = playerInventoryMenu.actualInventory[slot.myID];
                        Item result = currentRecipe.createItem();
                        if (result == null)
                        {
                            Game1.playSound("cancel");
                        }
                        else
                        {
                            result.Stack = cookAmount;
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
                            for (int i = 0; i < cookAmount; i++)
                            {
                                ConsumeIngredientsPrioritized(currentRecipe, 1);
                            }
                            UpdateCookingTable();
                            cookMode = false;
                        }
                        break;
                    }
                }
                return;
            }
            base.receiveLeftClick(x, y, playSound);
            scrollbar.ReceiveLeftClick(x, y);
            Item clickedItem = CookingTable.GetClickedItem(x, y);
            if (clickedItem == null || TerminalMenu == null)
            {
                return;
            }
            CraftingRecipe recipe = cookingRecipes.FirstOrDefault((CraftingRecipe r) => r.name == clickedItem.Name || r.DisplayName == clickedItem.DisplayName);
            if (recipe == null)
            {
                return;
            }
            int maxCanCook = recipe.getCraftableCount(TerminalMenu.GetAllStorageObjects());
            if (maxCanCook <= 0)
            {
                Game1.addHUDMessage(new HUDMessage(ModHelper.Helper.Translation.Get("not_enough_ingredients"), 3));
                return;
            }
            int toCook = ((!Game1.oldKBState.IsKeyDown(Keys.LeftShift) && !Game1.oldKBState.IsKeyDown(Keys.RightShift)) ? 1 : Math.Min(5, maxCanCook));
            List<IInventory> inventories2 = TerminalMenu.GetAllStorageObjects().Select((Func<Chest, IInventory>)((Chest ch) => ch.Items)).ToList();
            for (int i2 = 0; i2 < toCook; i2++)
            {
                ConsumeIngredientsPrioritized(recipe, 1);
            }
            for (int i3 = 0; i3 < toCook; i3++)
            {
                Item result2 = recipe.createItem();
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
            UpdateCookingTable();
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (!cookMode)
            {
                Item clickedItem = CookingTable.GetClickedItem(x, y);
                if (clickedItem != null)
                {
                    CraftingRecipe recipe = cookingRecipes.FirstOrDefault((CraftingRecipe r) => r.name == clickedItem.Name || r.DisplayName == clickedItem.DisplayName);
                    if (recipe != null)
                    {
                        int max = recipe.getCraftableCount(TerminalMenu.GetAllStorageObjects());
                        if (max > 0)
                        {
                            cookMode = true;
                            currentRecipe = recipe;
                            maxCookable = max;
                            cookAmount = 1;
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

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            CookingTable.PerformHoverAction(x, y);
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (cookMode)
            {
                cookAmount = Math.Clamp(cookAmount + ((direction > 0) ? 1 : (-1)), 1, maxCookable);
                return;
            }
            CookingTable.ReceiveScrollWheelAction(direction);
            scrollbar.UpdateScrollBarPosition();
        }

        public void LeftClickHeld(int x, int y)
        {
            CookingTable.PerformHoverAction(x, y);
            scrollbar.LeftClickHeld(x, y);
        }

        public override void receiveKeyPress(Keys key)
        {
            if (cookMode && key == Keys.Escape)
            {
                cookMode = false;
                Game1.playSound("cancel");
            }
            else
            {
                base.receiveKeyPress(key);
            }
        }

        private void UpdateCookingTable()
        {
            string sortCol = CookingTable.sortedColumn;
            bool asc = CookingTable.isAscending;
            int scroll = CookingTable.ScrollIndex;
            foreach (TableRowWithIcon row in CookingTable.AllRows)
            {
                int idx = (row.ItemIcon as StardewValley.Object)?.ParentSheetIndex ?? (-1);
                CraftingRecipe recipe = cookingRecipes.FirstOrDefault(delegate (CraftingRecipe r)
                {
                    StardewValley.Object obj = r.createItem() as StardewValley.Object;
                    return obj != null && obj.ParentSheetIndex == idx;
                });
                if (recipe != null)
                {
                    int maxQty = recipe.getCraftableCount(TerminalMenu.GetAllStorageObjects());
                    row.Cells[3] = maxQty.ToString();
                }
            }
            CookingTable.SortItemsBy(sortCol, asc);
            CookingTable.ScrollIndex = Math.Clamp(scroll, 0, Math.Max(0, CookingTable.GetItemEntriesCount() - CookingTable.GetVisibleRows()));
            CookingTable.scrollbar.UpdateScrollBarPosition();
        }

        private void ConsumeIngredientsPrioritized(CraftingRecipe recipe, int amount)
        {
            foreach (KeyValuePair<string, int> kv in recipe.recipeList)
            {
                string rawId = kv.Key;
                int required = kv.Value * amount;

                var sources = Game1.player.Items
                    .Select((item, idx) => new { item, idx })
                    .Where(x => x.item is StardewValley.Object obj && CraftingRecipe.ItemMatchesForCrafting(obj, rawId))
                    .Select(x => new
                    {
                        x.item,
                        action = (Action<int>)(qty =>
                        {
                            Game1.player.Items[x.idx] = ((StardewValley.Object)x.item).ConsumeStack(qty);
                        }),
                        isPlayer = true
                    })
                    .Concat(
                        TerminalMenu.GetAllStorageObjects()
                            .SelectMany(chest => chest.Items
                                .Select((item, idx) => new { item, idx, chest })
                                .Where(x => x.item is StardewValley.Object obj && CraftingRecipe.ItemMatchesForCrafting(obj, rawId))
                                .Select(x => new
                                {
                                    x.item,
                                    action = (Action<int>)(qty =>
                                    {
                                        ((StardewValley.Object)x.item).Stack -= qty;
                                        if (((StardewValley.Object)x.item).Stack <= 0)
                                        {
                                            x.chest.Items.RemoveAt(x.idx);
                                        }
                                    }),
                                    isPlayer = false
                                })
                            )
                    )
                    .ToList();

                sources = (from s in sources
                           orderby s.item.Quality, s.isPlayer descending
                           select s).ToList();
                int toRemove = required;
                foreach (var src in sources)
                {
                    if (toRemove <= 0)
                    {
                        break;
                    }
                    int take = Math.Min(src.item.Stack, toRemove);
                    src.action(take);
                    toRemove -= take;
                }
            }
        }

        public override void update(GameTime time)
        {
            base.update(time);
            CookingTable.Update();
            scrollbar.UpdateScrollBarPosition();
        }
    }
}
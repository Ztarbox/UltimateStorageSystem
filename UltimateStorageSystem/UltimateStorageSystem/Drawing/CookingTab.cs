using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Objects;
using System.Runtime.CompilerServices;
using UltimateStorageSystem.Tools;
using UltimateStorageSystem.Utilities;

namespace UltimateStorageSystem.Drawing
{
    public class CookingTab : IClickableMenu
    {
        private readonly InventoryMenu playerInventoryMenu;

        private readonly Scrollbar Scrollbar;

        private readonly List<CraftingRecipe> cookingRecipes;

        public InputHandler? inputHandler;

        public bool cookMode = false;

        private CraftingRecipe? currentRecipe = null;

        private int maxCookable = 0;

        private int cookAmount = 1;

        public DynamicTable CookingTable { get; private set; }

        public FarmLinkTerminalMenu TerminalMenu { get; set; }

        public CookingTab(int xPositionOnScreen, int yPositionOnScreen, int containerWidth, int containerHeight, FarmLinkTerminalMenu terminalMenu, InventoryMenu inventoryMenu)
            : base(xPositionOnScreen, yPositionOnScreen, containerWidth, containerHeight)
        {
            this.TerminalMenu = terminalMenu;
            this.playerInventoryMenu = inventoryMenu;
            this.cookingRecipes = Game1.player.cookingRecipes.Keys.Select(name => new CraftingRecipe(name, isCookingRecipe: true)).ToList();
            List<string> columnHeaders = new()
            {

                ModHelper.Helper.Translation.Get("column.recipe"),
                ModHelper.Helper.Translation.Get("column.buffs"),
                ModHelper.Helper.Translation.Get("column.ingredients"),
                ModHelper.Helper.Translation.Get("column.max")
            };
            List<int> columnWidths = new() { 250, 230, 280, 100 };
            List<bool> columnAlignments = new() { false, false, false, true };
            List<TableRowWithIcon> tableRows = this.GenerateRecipeData();
            this.CookingTable = new DynamicTable(xPositionOnScreen + 30, yPositionOnScreen + 40, columnHeaders, columnWidths, columnAlignments, tableRows, this.Scrollbar);
            this.Scrollbar = new Scrollbar(xPositionOnScreen + containerWidth - 50, yPositionOnScreen + 103, this.CookingTable);
            this.CookingTable.Scrollbar = this.Scrollbar;
        }

        public void ResetSort()
        {
            this.CookingTable.ResetSort();
        }

        private List<TableRowWithIcon> GenerateRecipeData()
        {
            List<TableRowWithIcon> rows = new();
            foreach (string recipeKey in Game1.player.cookingRecipes.Keys)
            {
                CraftingRecipe craftingRecipe = new(recipeKey, isCookingRecipe: true);
                if (craftingRecipe.createItem() is StardewValley.Object dish && dish.QualifiedItemId.StartsWith("(O)"))
                {
                    string recipeName = craftingRecipe.DisplayName;
                    string buffs = GetRecipeBuffs(craftingRecipe);
                    string ingredients = GetRecipeIngredients(craftingRecipe);
                    string maxQuantity = craftingRecipe.getCraftableCount(FarmLinkTerminalMenu.GetAllStorageObjects()).ToString();
                    rows.Add(new TableRowWithIcon(dish, new List<string> { recipeName, buffs, ingredients, maxQuantity }));
                }
            }
            return rows;
        }

        private static string GetRecipeBuffs(CraftingRecipe craftingRecipe)
        {
            if (craftingRecipe.createItem() is not StardewValley.Object cookedDish)
            {
                return "No Buffs";
            }
            List<string> buffs = new();
            int health = cookedDish.healthRecoveredOnConsumption();
            int energy = cookedDish.staminaRecoveredOnConsumption();
            
            if (health > 0)
            {
                buffs.Add($"+{health} Health");
            }
            if (energy > 0)
            {
                buffs.Add($"+{energy} Energy");
            }
            var itemBuffs = cookedDish.GetFoodOrDrinkBuffs();
            buffs.AddRange(itemBuffs.SelectMany(buff => BuffsDisplay.displayAttributes.Select(attrib => GetBuffDescription(buff, attrib)).Where(desc => !String.IsNullOrWhiteSpace(desc))));
            return (buffs.Count > 0) ? string.Join(", ", buffs) : "No Buffs";
        }

        private static string GetBuffDescription(Buff buff, BuffAttributeDisplay attribute)
        {
            float value = attribute.Value(buff);
            if (value == 0f)
            {
                return "";
            }
            string description = attribute.Description(value);
            return description;
        }


        private static string GetRecipeIngredients(CraftingRecipe craftingRecipe)
        {
            IEnumerable<string> names = craftingRecipe.recipeList.Select(kv => $"{craftingRecipe.getNameFromIndex(kv.Key)} ({kv.Value})");
            return string.Join(", ", names);
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            int fixedWidth = 1000;
            int upperFrameHeight = 620;
            int inventoryFrameHeight = 280;
            int tableWidth = this.CookingTable.ColumnWidths.Sum() + (this.CookingTable.ColumnWidths.Count - 1) * 10;
            string title = "ULTIMATE COOKING SYSTEM";
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
            this.CookingTable.Draw(b);
            drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen + upperFrameHeight - 60, fixedWidth, inventoryFrameHeight - 30, Color.White);
            this.playerInventoryMenu.draw(b);
            if (this.cookMode && this.currentRecipe != null)
            {
                Vector2 mousePos = new(Game1.getMouseX(), Game1.getMouseY());
                Item icon = this.currentRecipe.createItem();
                icon.Stack = this.cookAmount;
                icon.drawInMenu(b, mousePos, 1f, 1f, 0.9f, StackDrawType.Draw, Color.White, drawShadow: false);
            }
            this.drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.cookMode && this.currentRecipe != null)
            {
                var slotID = this.playerInventoryMenu.getInventoryPositionOfClick(x, y);
                Item existingItem = this.playerInventoryMenu.actualInventory[slotID];
                Item result = this.currentRecipe.createItem();
                if (result == null)
                {
                    Game1.playSound("cancel");
                }
                else
                {
                    result.Stack = this.cookAmount;
                    if (existingItem == null)
                    {
                        this.playerInventoryMenu.actualInventory[slotID] = result;
                        Game1.playSound("coin");
                    }
                    else
                    {
                        if (!existingItem.canStackWith(result))
                        {
                            Game1.playSound("cancel");
                            return;
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
                                return;
                            }
                            existingItem.Stack += availableSpace;
                            result.Stack -= availableSpace;
                            Game1.playSound("coin");
                        }
                    }
                    List<IInventory> inventories = FarmLinkTerminalMenu.GetAllStorageObjects().Select((Func<Chest, IInventory>)(ch => ch.Items)).ToList();
                    for (int i = 0; i < this.cookAmount; i++)
                    {
                        this.ConsumeIngredientsPrioritized(this.currentRecipe, 1);
                    }
                    this.UpdateCookingTable();

                    Game1.player.cookedRecipe(result.ItemId);
                    Game1.stats.checkForCookingAchievements();

                    this.cookMode = false;
                }
            }
            else
            {
                base.receiveLeftClick(x, y, playSound);
                this.Scrollbar.ReceiveLeftClick(x, y);
                var clickedItem = this.CookingTable.GetClickedItem(x, y);
                if (clickedItem == null || this.TerminalMenu == null)
                {
                    return;
                }
                var recipe = this.cookingRecipes.FirstOrDefault(r => r.name == clickedItem.Name || r.DisplayName == clickedItem.DisplayName);
                if (recipe == null)
                {
                    return;
                }
                int maxCanCook = recipe.getCraftableCount(FarmLinkTerminalMenu.GetAllStorageObjects());
                if (maxCanCook <= 0)
                {
                    Game1.addHUDMessage(new HUDMessage(ModHelper.Helper.Translation.Get("not_enough_ingredients"), 3));
                    return;
                }
                int toCook = ((!Game1.oldKBState.IsKeyDown(Keys.LeftShift) && !Game1.oldKBState.IsKeyDown(Keys.RightShift)) ? 1 : Math.Min(5, maxCanCook));
                List<IInventory> inventories2 = FarmLinkTerminalMenu.GetAllStorageObjects().Select((Func<Chest, IInventory>)(ch => ch.Items)).ToList();
                for (int i2 = 0; i2 < toCook; i2++)
                {
                    this.ConsumeIngredientsPrioritized(recipe, 1);
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
                this.UpdateCookingTable();
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (!this.cookMode)
            {
                var clickedItem = this.CookingTable.GetClickedItem(x, y);
                if (clickedItem != null)
                {
                    var recipe = this.cookingRecipes.FirstOrDefault(r => r.name == clickedItem.Name || r.DisplayName == clickedItem.DisplayName);
                    if (recipe != null)
                    {
                        int max = recipe.getCraftableCount(FarmLinkTerminalMenu.GetAllStorageObjects());
                        if (max > 0)
                        {
                            this.cookMode = true;
                            this.currentRecipe = recipe;
                            this.maxCookable = max;
                            this.cookAmount = 1;
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
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (this.cookMode)
            {
                this.cookAmount = Math.Clamp(this.cookAmount + ((direction > 0) ? 1 : (-1)), 1, this.maxCookable);
                return;
            }
            this.CookingTable.ReceiveScrollWheelAction(direction);
            this.Scrollbar.UpdateScrollBarPosition();
        }

        public void LeftClickHeld(int x, int y)
        {
            this.Scrollbar.LeftClickHeld(x, y);
        }

        public override void receiveKeyPress(Keys key)
        {
            if (this.cookMode && key == Keys.Escape)
            {
                this.cookMode = false;
                Game1.playSound("cancel");
            }
            else
            {
                base.receiveKeyPress(key);
            }
        }

        private void UpdateCookingTable()
        {
            var sortCol = this.CookingTable.SortedColumn;
            bool asc = this.CookingTable.isAscending;
            int scroll = this.CookingTable.ScrollIndex;
            foreach (TableRowWithIcon row in this.CookingTable.AllRows)
            {
                int idx = (row.ItemIcon as StardewValley.Object)?.ParentSheetIndex ?? (-1);
                var recipe = this.cookingRecipes.FirstOrDefault(delegate (CraftingRecipe r)
                {
                    return r.createItem() is StardewValley.Object obj && obj.ParentSheetIndex == idx;
                });
                if (recipe != null)
                {
                    int maxQty = recipe.getCraftableCount(FarmLinkTerminalMenu.GetAllStorageObjects());
                    row.Cells[3] = maxQty.ToString();
                }
            }
            this.CookingTable.SortItemsBy(sortCol, asc);
            this.CookingTable.ScrollIndex = Math.Clamp(scroll, 0, Math.Max(0, this.CookingTable.GetItemEntriesCount() - this.CookingTable.GetVisibleRows()));
            this.CookingTable.Scrollbar?.UpdateScrollBarPosition();
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
                        FarmLinkTerminalMenu.GetAllStorageObjects()
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
            this.CookingTable.Update();
            this.Scrollbar.UpdateScrollBarPosition();
        }
    }
}
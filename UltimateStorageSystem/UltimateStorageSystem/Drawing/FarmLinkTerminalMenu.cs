using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using UltimateStorageSystem.Tools;
using UltimateStorageSystem.Utilities;

namespace UltimateStorageSystem.Drawing
{
    public class FarmLinkTerminalMenu : IClickableMenu
    {
        private readonly string searchLabel;

        public StorageTab StorageTab;

        private readonly WorkbenchTab workbenchTab;

        private readonly CookingTab cookingTab;

        private readonly ShoppingTab shoppingTab;

        private readonly CommandInputField commandInputField;

        private readonly List<ClickableTextureComponent> tabs;

        private int selectedTab;

        private readonly InputHandler inputHandler;

        public FarmLinkTerminalMenu(List<Chest> chests)
            : base(0, 0, 1000, 900)
        {
            this.searchLabel = ModHelper.Helper.Translation.Get("Search");
            this.width = 1000;
            this.height = 800;
            float scaleFactor = Game1.options.uiScale;
            int screenWidth = Game1.viewport.Width;
            int screenHeight = Game1.viewport.Height;
            int containerWidthScaled = (int)(this.width * scaleFactor);
            int containerHeightScaled = (int)(this.height * scaleFactor);
            this.xPositionOnScreen = (int)((Game1.uiViewport.Width - this.width) / 2f);
            this.yPositionOnScreen = (int)((Game1.uiViewport.Height - this.height) / 2f);
            DynamicTable itemTable = new(this.xPositionOnScreen, this.yPositionOnScreen, new List<string>(), new List<int>(), new List<bool>(), new List<TableRowWithIcon>(), null);
            Scrollbar scrollbar = new(this.xPositionOnScreen + 790, this.yPositionOnScreen + 120, itemTable);
            InventoryMenu playerInventoryMenu = new(this.xPositionOnScreen + 15, this.yPositionOnScreen + 680, playerInventory: true, rows: 6);
            ItemTransferManager itemTransferManager = new(chests, itemTable);
            itemTransferManager.UpdateChestItemsAndSort();
            this.inputHandler = new InputHandler(playerInventoryMenu, scrollbar, this);
            this.StorageTab = new StorageTab(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, this);
            this.StorageTab.inputHandler = this.inputHandler;
            this.workbenchTab = new WorkbenchTab(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, this);
            this.workbenchTab.inputHandler = this.inputHandler;
            this.workbenchTab.TerminalMenu = this;
            this.cookingTab = new CookingTab(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, this);
            this.cookingTab.inputHandler = this.inputHandler;
            this.shoppingTab = new ShoppingTab(this.xPositionOnScreen, this.yPositionOnScreen);
            this.commandInputField = new CommandInputField(this.xPositionOnScreen + 30, this.yPositionOnScreen + 20, this.GetActiveTable(), this.searchLabel);
            this.StorageTab.ResetSort();
            this.workbenchTab.ResetSort();
            this.cookingTab.ResetSort();
            this.tabs = new List<ClickableTextureComponent>
            {
                new("Storage", new Rectangle(this.xPositionOnScreen + 20, this.yPositionOnScreen - 64, 64, 64), null, null, Game1.mouseCursors, new Rectangle(16, 368, 16, 16), 4f),
                new("Workbench", new Rectangle(this.xPositionOnScreen + 88, this.yPositionOnScreen - 64, 64, 64), null, null, Game1.mouseCursors, new Rectangle(16, 368, 16, 16), 4f),
                new("Cooking", new Rectangle(this.xPositionOnScreen + 156, this.yPositionOnScreen - 64, 64, 64), null, null, Game1.mouseCursors, new Rectangle(16, 368, 16, 16), 4f)
            };
            this.selectedTab = 0;
        }

        private DynamicTable GetActiveTable()
        {
            return this.selectedTab switch
            {
                0 => this.StorageTab.ItemTable,
                1 => this.workbenchTab.CraftingTable,
                2 => this.cookingTab.CookingTable,
                _ => this.StorageTab.ItemTable,
            };
        }

        public List<Chest> GetAllStorageObjects()
        {
            List<Chest> storageObjects = new();
            HashSet<GameLocation> visitedLocations = new();
            foreach (GameLocation location in ModEntry.LocationTracker.GetVisitedLocations())
            {
                AddStorageFromLocation(location);
            }
            return storageObjects;
            void AddStorageFromLocation(GameLocation gameLocation)
            {
                if (visitedLocations.Add(gameLocation))
                {
                    foreach (StardewValley.Object obj in gameLocation.Objects.Values)
                    {
                        if (obj is Chest chest && IsValidStorage(chest))
                        {
                            storageObjects.Add(chest);
                        }
                    }
                    if (gameLocation is FarmHouse house)
                    {
                        var fridgeChest = house.fridge?.Value;
                        if (fridgeChest != null && !IsBlockedChest(fridgeChest))
                        {
                            storageObjects.Add(fridgeChest);
                        }
                    }
                    if (gameLocation is IslandFarmHouse islandhouse)
                    {
                        var islandfridgeChest = islandhouse.fridge?.Value;
                        if (islandfridgeChest != null && !IsBlockedChest(islandfridgeChest))
                        {
                            storageObjects.Add(islandfridgeChest);
                        }
                    }
                    if (gameLocation is Cabin cabin)
                    {
                        var cabinFridge = cabin.fridge?.Value;
                        if (cabinFridge != null && !IsBlockedChest(cabinFridge))
                        {
                            storageObjects.Add(cabinFridge);
                        }
                    }
                    if (gameLocation != null)
                    {
                        if (true)
                        {
                            foreach (Building building in gameLocation.buildings)
                            {
                                var indoors = building.indoors?.Value;
                                if (indoors != null)
                                {
                                    AddStorageFromLocation(indoors);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool IsBlockedChest(Chest chest)
        {
            return chest.Items.Any(item => item is StardewValley.Object obj && obj.QualifiedItemId == "(BC)holybananapants.UltimateStorageSystemContentPack_BlockTerminal");
        }

        private static bool IsValidStorage(Chest chest)
        {
            return chest.playerChest.Value && !IsBlockedChest(chest);
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            this.commandInputField.Draw(b);
            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, (int)(Game1.viewport.Width / Game1.options.uiScale), (int)(Game1.viewport.Height / Game1.options.uiScale)), Color.Black * 0.8f);
            int fixedWidth = 1000;
            IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen, fixedWidth, this.height, Color.White);
            b.Draw(Game1.staminaRect, new Rectangle(this.xPositionOnScreen + 12, this.yPositionOnScreen + 12, fixedWidth - 24, this.height - 24), Color.Black);
            if (this.selectedTab == 0)
            {
                this.StorageTab.draw(b);
            }
            else if (this.selectedTab == 1)
            {
                this.workbenchTab.draw(b);
            }
            else if (this.selectedTab == 2)
            {
                this.cookingTab.draw(b);
            }
            else if (this.selectedTab == 3)
            {
                this.shoppingTab.draw(b);
            }
            this.commandInputField.Draw(b);
            foreach (ClickableTextureComponent tab in this.tabs)
            {
                int yOffset = ((this.tabs.IndexOf(tab) == this.selectedTab) ? 8 : 0);
                tab.bounds.Y += yOffset;
                tab.draw(b, Color.White, 0.86f);
                tab.bounds.Y -= yOffset;
                Texture2D chestIcon = Game1.objectSpriteSheet;
                Vector2 tabIconPosition;
                Rectangle sourceRect;
                if (this.tabs.IndexOf(tab) == 0)
                {
                    sourceRect = Game1.getSourceRectForStandardTileSheet(chestIcon, 166, 16, 16);
                    tabIconPosition = new Vector2(this.xPositionOnScreen + 36, this.yPositionOnScreen - 40 + yOffset);
                }
                else
                {
                    if (this.tabs.IndexOf(tab) == 1)
                    {
                        Texture2D mouseCursors = Game1.mouseCursors;
                        Rectangle hammerSourceRect = new(64, 368, 16, 16);
                        Vector2 hammerIconPosition = new(this.xPositionOnScreen + 88, this.yPositionOnScreen - 64 + yOffset);
                        b.Draw(mouseCursors, hammerIconPosition, hammerSourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                        continue;
                    }
                    if (this.tabs.IndexOf(tab) != 2)
                    {
                        Texture2D basketTexture = ModEntry.basketTexture;
                        sourceRect = new Rectangle(0, 0, basketTexture.Width, basketTexture.Height);
                        Vector2 basketIconPosition = new(tab.bounds.X + 16, tab.bounds.Y + 22 + yOffset);
                        b.Draw(basketTexture, basketIconPosition, sourceRect, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 1f);
                        continue;
                    }
                    sourceRect = Game1.getSourceRectForStandardTileSheet(chestIcon, 241, 16, 16);
                    tabIconPosition = new Vector2(this.xPositionOnScreen + 172, this.yPositionOnScreen - 40 + yOffset);
                }
                b.Draw(chestIcon, tabIconPosition, sourceRect, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 1f);
            }
            this.DrawQuickTips(b);
            this.drawMouse(b);
        }

        private void DrawQuickTips(SpriteBatch b)
        {
            ITranslationHelper translation = ModEntry.Instance.Helper.Translation;
            int x = this.xPositionOnScreen + 15;
            int yStart = this.yPositionOnScreen + this.height + 10;
            int lineSpacing = 40;
            float scale = 1f;
            Vector2 pos = new(x, yStart);
            switch (this.selectedTab)
            {
                case 0:
                    DrawLabel("help.storage.leftclick.title", ref pos);
                    DrawText(ModHelper.Helper.Translation.Get("help.storage.leftclick.desc"), ref pos);
                    DrawText("|", ref pos);
                    DrawLabel("help.storage.shiftleftclick.title", ref pos);
                    DrawText(ModHelper.Helper.Translation.Get("help.storage.shiftleftclick.desc"), ref pos);
                    break;
                case 1:
                    DrawLabel("help.crafting.leftclick.title", ref pos);
                    DrawText(ModHelper.Helper.Translation.Get("help.crafting.quick.desc.split1"), ref pos);
                    DrawText("|", ref pos);
                    DrawLabel("help.crafting.shiftleftclick.title", ref pos);
                    DrawText(ModHelper.Helper.Translation.Get("help.crafting.quick.desc.split2"), ref pos);
                    break;
                case 2:
                    DrawLabel("help.cooking.leftclick.title", ref pos);
                    DrawText(ModHelper.Helper.Translation.Get("help.cooking.quick.desc.split1"), ref pos);
                    DrawText("|", ref pos);
                    DrawLabel("help.cooking.shiftleftclick.title", ref pos);
                    DrawText(ModHelper.Helper.Translation.Get("help.cooking.quick.desc.split2"), ref pos);
                    break;
                case 3:
                    DrawLabel("help.shopping.leftclick.title", ref pos);
                    DrawText(ModHelper.Helper.Translation.Get("help.shopping.leftclick.desc"), ref pos);
                    break;
            }
            pos = new Vector2(x, yStart + lineSpacing);
            switch (this.selectedTab)
            {
                case 0:
                    DrawLabel("help.storage.rightclick.title", ref pos);
                    DrawText(ModHelper.Helper.Translation.Get("help.storage.rightclick.desc"), ref pos);
                    DrawText("|", ref pos);
                    DrawLabel("help.storage.shiftrightclick.title", ref pos);
                    DrawText(ModHelper.Helper.Translation.Get("help.storage.shiftrightclick.desc"), ref pos);
                    break;
                case 1:
                    DrawLabel("help.crafting.rightclick.title", ref pos);
                    DrawText(ModHelper.Helper.Translation.Get("help.crafting.multi.desc"), ref pos);
                    break;
                case 2:
                    DrawLabel("help.cooking.rightclick.title", ref pos);
                    DrawText(ModHelper.Helper.Translation.Get("help.cooking.multi.desc"), ref pos);
                    break;
                case 3:
                    break;
            }
            void DrawLabel(string key, ref Vector2 reference)
            {
                b.DrawString(Game1.smallFont, ModHelper.Helper.Translation.Get(key), reference, Color.Orange, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
                reference.X += Game1.smallFont.MeasureString(ModHelper.Helper.Translation.Get(key)).X + 10f;
            }
            void DrawText(string text, ref Vector2 reference)
            {
                b.DrawString(Game1.smallFont, text, reference, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
                reference.X += Game1.smallFont.MeasureString(text).X + 20f;
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            bool inMenu = x >= this.xPositionOnScreen && x <= this.xPositionOnScreen + this.width && y >= this.yPositionOnScreen && y <= this.yPositionOnScreen + this.height;
            bool onTab = this.tabs.Exists(tab => tab.containsPoint(x, y));
            if (!inMenu && !onTab)
            {
                return;
            }
            base.receiveLeftClick(x, y, playSound);
            for (int i = 0; i < this.tabs.Count; i++)
            {
                if (this.tabs[i].containsPoint(x, y))
                {
                    this.selectedTab = i;
                    this.commandInputField.UpdateTable(this.GetActiveTable());
                    this.commandInputField.Reset();
                    this.StorageTab.ResetSort();
                    this.workbenchTab.ResetSort();
                    this.cookingTab.ResetSort();
                    if (this.selectedTab == 0)
                    {
                        this.StorageTab.RefreshItems();
                    }
                    return;
                }
            }
            DynamicTable activeTable = this.GetActiveTable();
            int prevScroll = activeTable.ScrollIndex;
            switch (this.selectedTab)
            {
                case 0:
                    this.StorageTab.receiveLeftClick(x, y, playSound);
                    break;
                case 1:
                    this.workbenchTab.receiveLeftClick(x, y, playSound);
                    break;
                case 2:
                    this.cookingTab.receiveLeftClick(x, y, playSound);
                    break;
                case 3:
                    this.shoppingTab.receiveLeftClick(x, y, playSound);
                    break;
            }
            this.commandInputField.UpdateTable(activeTable);
            activeTable.SortItemsBy(activeTable.SortedColumn, activeTable.isAscending);
            activeTable.ScrollIndex = Math.Clamp(prevScroll, 0, Math.Max(0, activeTable.GetItemEntriesCount() - activeTable.GetVisibleRows()));
            activeTable.Scrollbar?.UpdateScrollBarPosition();
            activeTable.Scrollbar?.ReceiveLeftClick(x, y);
            activeTable.Scrollbar?.UpdateScrollBarPosition();
        }

        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);
            if (this.GetActiveTable() is DynamicTable activeTable)
            {
                activeTable.Scrollbar?.LeftClickHeld(x, y);
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
            if (this.GetActiveTable() is DynamicTable activeTable)
            {
                activeTable.Scrollbar?.ReleaseLeftClick(x, y);
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (ModEntry.Instance.ignoreNextRightClick)
            {
                ModEntry.Instance.ignoreNextRightClick = false;
                return;
            }
            bool inMenu = x >= this.xPositionOnScreen && x <= this.xPositionOnScreen + this.width && y >= this.yPositionOnScreen && y <= this.yPositionOnScreen + this.height;
            bool onTab = this.tabs.Exists(tab => tab.containsPoint(x, y));
            if (!inMenu && !onTab)
            {
                return;
            }
            base.receiveRightClick(x, y, playSound);
            for (int i = 0; i < this.tabs.Count; i++)
            {
                if (this.tabs[i].containsPoint(x, y))
                {
                    this.selectedTab = i;
                    this.commandInputField.UpdateTable(this.GetActiveTable());
                    this.commandInputField.Reset();
                    this.StorageTab.ResetSort();
                    this.workbenchTab.ResetSort();
                    this.cookingTab.ResetSort();
                    return;
                }
            }
            var activeTable = this.GetActiveTable();
            int prevScroll = activeTable.ScrollIndex;
            switch (this.selectedTab)
            {
                case 0:
                    this.StorageTab.receiveRightClick(x, y, playSound);
                    break;
                case 1:
                    this.workbenchTab.receiveRightClick(x, y, playSound);
                    break;
                case 2:
                    this.cookingTab.receiveRightClick(x, y, playSound);
                    break;
                case 3:
                    this.shoppingTab.receiveRightClick(x, y, playSound);
                    break;
            }
            this.commandInputField.UpdateTable(activeTable);
            activeTable.SortItemsBy(activeTable.SortedColumn, activeTable.isAscending);
            activeTable.ScrollIndex = Math.Clamp(prevScroll, 0, Math.Max(0, activeTable.GetItemEntriesCount() - activeTable.GetVisibleRows()));
            activeTable.Scrollbar?.UpdateScrollBarPosition();
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (this.selectedTab == 0)
            {
                this.StorageTab.receiveScrollWheelAction(direction);
            }
            else if (this.selectedTab == 1)
            {
                this.workbenchTab.receiveScrollWheelAction(direction);
            }
            else if (this.selectedTab == 2)
            {
                this.cookingTab.receiveScrollWheelAction(direction);
            }
            else if (this.selectedTab != 3)
            {
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Escape && this.selectedTab == 1 && this.workbenchTab != null && this.workbenchTab.craftMode)
            {
                this.workbenchTab.CancelCraftMode();
            }
            else if (key == Keys.Escape)
            {
                this.exitThisMenu();
                Game1.playSound("bigDeSelect");
            }
            else
            {
                this.commandInputField.ReceiveKeyPress(key);
            }
        }

        public override void performHoverAction(int x, int y)
        {
            if (this.selectedTab == 0)
            {
                this.StorageTab.performHoverAction(x, y);
            }
            else if (this.selectedTab == 1)
            {
                this.workbenchTab.performHoverAction(x, y);
            }
            else if (this.selectedTab == 2)
            {
                this.cookingTab.performHoverAction(x, y);
            }
            else if (this.selectedTab != 3)
            {
            }
        }

        public override void update(GameTime time)
        {
            base.update(time);
            this.commandInputField.Update(time);
        }
    }
}
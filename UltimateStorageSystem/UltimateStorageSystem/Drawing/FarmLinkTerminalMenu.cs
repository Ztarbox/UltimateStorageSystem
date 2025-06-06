using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using UltimateStorageSystem.Interfaces;
using UltimateStorageSystem.Tools;
using UltimateStorageSystem.Utilities;

namespace UltimateStorageSystem.Drawing
{
    public class FarmLinkTerminalMenu : IClickableMenu
    {
        private IModHelper helper;

        private readonly string searchLabel;

        public StorageTab StorageTab;

        private WorkbenchTab workbenchTab;

        private CookingTab cookingTab;

        private ShoppingTab shoppingTab;

        private CommandInputField commandInputField;

        private List<ClickableTextureComponent> tabs;

        private int selectedTab;

        private InputHandler inputHandler;

        private const int ScreenEdgePadding = 100;

        public FarmLinkTerminalMenu(List<Chest> chests, IModHelper helper)
            : base(0, 0, 1000, 900)
        {
            this.helper = helper;
            searchLabel = ModHelper.Helper.Translation.Get("Search");
            width = 1000;
            height = 800;
            float scaleFactor = Game1.options.uiScale;
            int screenWidth = Game1.viewport.Width;
            int screenHeight = Game1.viewport.Height;
            int containerWidthScaled = (int)(width * scaleFactor);
            int containerHeightScaled = (int)(height * scaleFactor);
            xPositionOnScreen = (int)((Game1.uiViewport.Width - width) / 2f);
            yPositionOnScreen = (int)((Game1.uiViewport.Height - height) / 2f);
            DynamicTable itemTable = new(xPositionOnScreen, yPositionOnScreen, new List<string>(), new List<int>(), new List<bool>(), new List<TableRowWithIcon>(), null);
            Scrollbar scrollbar = new(xPositionOnScreen + 790, yPositionOnScreen + 120, itemTable);
            InventoryMenu playerInventoryMenu = new(xPositionOnScreen + 15, yPositionOnScreen + 680, playerInventory: true);
            ItemTransferManager itemTransferManager = new(chests, itemTable);
            itemTransferManager.UpdateChestItemsAndSort();
            inputHandler = new InputHandler(playerInventoryMenu, scrollbar, this);
            StorageTab = new StorageTab(xPositionOnScreen, yPositionOnScreen, width, height, this);
            StorageTab.inputHandler = inputHandler;
            workbenchTab = new WorkbenchTab(xPositionOnScreen, yPositionOnScreen, width, height, this);
            workbenchTab.inputHandler = inputHandler;
            workbenchTab.TerminalMenu = this;
            cookingTab = new CookingTab(xPositionOnScreen, yPositionOnScreen, width, height, this);
            cookingTab.inputHandler = inputHandler;
            shoppingTab = new ShoppingTab(xPositionOnScreen, yPositionOnScreen);
            commandInputField = new CommandInputField(xPositionOnScreen + 30, yPositionOnScreen + 20, GetActiveTable(), searchLabel);
            StorageTab.ResetSort();
            workbenchTab.ResetSort();
            cookingTab.ResetSort();
            tabs = new List<ClickableTextureComponent>
        {
            new("Storage", new Rectangle(xPositionOnScreen + 20, yPositionOnScreen - 64, 64, 64), null, null, Game1.mouseCursors, new Rectangle(16, 368, 16, 16), 4f),
            new("Workbench", new Rectangle(xPositionOnScreen + 88, yPositionOnScreen - 64, 64, 64), null, null, Game1.mouseCursors, new Rectangle(16, 368, 16, 16), 4f),
            new("Cooking", new Rectangle(xPositionOnScreen + 156, yPositionOnScreen - 64, 64, 64), null, null, Game1.mouseCursors, new Rectangle(16, 368, 16, 16), 4f)
        };
            selectedTab = 0;
        }

        private IFilterableTable GetActiveTable()
        {
            return selectedTab switch
            {
                0 => StorageTab.ItemTable,
                1 => workbenchTab.CraftingTable,
                2 => cookingTab.CookingTable,
                _ => StorageTab.ItemTable,
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
                        Chest fridgeChest = house.fridge?.Value;
                        if (fridgeChest != null && !IsBlockedChest(fridgeChest))
                        {
                            storageObjects.Add(fridgeChest);
                        }
                    }
                    if (gameLocation is IslandFarmHouse islandhouse)
                    {
                        Chest islandfridgeChest = islandhouse.fridge?.Value;
                        if (islandfridgeChest != null && !IsBlockedChest(islandfridgeChest))
                        {
                            storageObjects.Add(islandfridgeChest);
                        }
                    }
                    if (gameLocation is Cabin cabin)
                    {
                        Chest cabinFridge = cabin.fridge?.Value;
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
                                GameLocation indoors = building.indoors?.Value;
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

        private bool IsBlockedChest(Chest chest)
        {
            return chest.Items.Any((Item item) => item is StardewValley.Object obj && obj.QualifiedItemId == "(BC)holybananapants.UltimateStorageSystemContentPack_BlockTerminal");
        }

        private bool IsValidStorage(Chest chest)
        {
            return chest.playerChest.Value && !IsBlockedChest(chest);
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            commandInputField.Draw(b);
            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, (int)(Game1.viewport.Width / Game1.options.uiScale), (int)(Game1.viewport.Height / Game1.options.uiScale)), Color.Black * 0.8f);
            int fixedWidth = 1000;
            IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, fixedWidth, height, Color.White);
            b.Draw(Game1.staminaRect, new Rectangle(xPositionOnScreen + 12, yPositionOnScreen + 12, fixedWidth - 24, height - 24), Color.Black);
            if (selectedTab == 0)
            {
                StorageTab.draw(b);
            }
            else if (selectedTab == 1)
            {
                workbenchTab.draw(b);
            }
            else if (selectedTab == 2)
            {
                cookingTab.draw(b);
            }
            else if (selectedTab == 3)
            {
                shoppingTab.draw(b);
            }
            commandInputField.Draw(b);
            foreach (ClickableTextureComponent tab in tabs)
            {
                int yOffset = ((tabs.IndexOf(tab) == selectedTab) ? 8 : 0);
                tab.bounds.Y += yOffset;
                tab.draw(b, Color.White, 0.86f);
                tab.bounds.Y -= yOffset;
                Texture2D chestIcon = Game1.objectSpriteSheet;
                Vector2 tabIconPosition;
                Rectangle sourceRect;
                if (tabs.IndexOf(tab) == 0)
                {
                    sourceRect = Game1.getSourceRectForStandardTileSheet(chestIcon, 166, 16, 16);
                    tabIconPosition = new Vector2(xPositionOnScreen + 36, yPositionOnScreen - 40 + yOffset);
                }
                else
                {
                    if (tabs.IndexOf(tab) == 1)
                    {
                        Texture2D mouseCursors = Game1.mouseCursors;
                        Rectangle hammerSourceRect = new(64, 368, 16, 16);
                        Vector2 hammerIconPosition = new(xPositionOnScreen + 88, yPositionOnScreen - 64 + yOffset);
                        b.Draw(mouseCursors, hammerIconPosition, hammerSourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                        continue;
                    }
                    if (tabs.IndexOf(tab) != 2)
                    {
                        Texture2D basketTexture = ModEntry.basketTexture;
                        sourceRect = new Rectangle(0, 0, basketTexture.Width, basketTexture.Height);
                        Vector2 basketIconPosition = new(tab.bounds.X + 16, tab.bounds.Y + 22 + yOffset);
                        b.Draw(basketTexture, basketIconPosition, sourceRect, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 1f);
                        continue;
                    }
                    sourceRect = Game1.getSourceRectForStandardTileSheet(chestIcon, 241, 16, 16);
                    tabIconPosition = new Vector2(xPositionOnScreen + 172, yPositionOnScreen - 40 + yOffset);
                }
                b.Draw(chestIcon, tabIconPosition, sourceRect, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 1f);
            }
            DrawQuickTips(b);
            drawMouse(b);
        }

        private void DrawQuickTips(SpriteBatch b)
        {
            ITranslationHelper translation = ModEntry.Instance.Helper.Translation;
            int x = xPositionOnScreen + 15;
            int yStart = yPositionOnScreen + height + 10;
            int lineSpacing = 40;
            float scale = 1f;
            Vector2 pos = new(x, yStart);
            switch (selectedTab)
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
            switch (selectedTab)
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
            bool inMenu = x >= xPositionOnScreen && x <= xPositionOnScreen + width && y >= yPositionOnScreen && y <= yPositionOnScreen + height;
            bool onTab = tabs.Exists((ClickableTextureComponent tab) => tab.containsPoint(x, y));
            if (!inMenu && !onTab)
            {
                return;
            }
            base.receiveLeftClick(x, y, playSound);
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].containsPoint(x, y))
                {
                    selectedTab = i;
                    commandInputField.UpdateTable(GetActiveTable());
                    commandInputField.Reset();
                    StorageTab.ResetSort();
                    workbenchTab.ResetSort();
                    cookingTab.ResetSort();
                    if (selectedTab == 0)
                    {
                        StorageTab.RefreshItems();
                    }
                    return;
                }
            }
            DynamicTable activeTable = GetActiveTable() as DynamicTable;
            int prevScroll = activeTable?.ScrollIndex ?? 0;
            switch (selectedTab)
            {
                case 0:
                    StorageTab.receiveLeftClick(x, y, playSound);
                    break;
                case 1:
                    workbenchTab.receiveLeftClick(x, y, playSound);
                    break;
                case 2:
                    cookingTab.receiveLeftClick(x, y, playSound);
                    break;
                case 3:
                    shoppingTab.receiveLeftClick(x, y, playSound);
                    break;
            }
            commandInputField.UpdateTable(activeTable);
            activeTable.SortItemsBy(activeTable.sortedColumn, activeTable.isAscending);
            activeTable.ScrollIndex = Math.Clamp(prevScroll, 0, Math.Max(0, activeTable.GetItemEntriesCount() - activeTable.GetVisibleRows()));
            activeTable.scrollbar.UpdateScrollBarPosition();
            activeTable?.scrollbar.ReceiveLeftClick(x, y);
            activeTable.scrollbar.UpdateScrollBarPosition();
        }

        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);
            if (GetActiveTable() is DynamicTable activeTable)
            {
                activeTable.scrollbar.LeftClickHeld(x, y);
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
            if (GetActiveTable() is DynamicTable activeTable)
            {
                activeTable.scrollbar.ReleaseLeftClick(x, y);
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (ModEntry.Instance.ignoreNextRightClick)
            {
                ModEntry.Instance.ignoreNextRightClick = false;
                return;
            }
            bool inMenu = x >= xPositionOnScreen && x <= xPositionOnScreen + width && y >= yPositionOnScreen && y <= yPositionOnScreen + height;
            bool onTab = tabs.Exists((ClickableTextureComponent tab) => tab.containsPoint(x, y));
            if (!inMenu && !onTab)
            {
                return;
            }
            base.receiveRightClick(x, y, playSound);
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].containsPoint(x, y))
                {
                    selectedTab = i;
                    commandInputField.UpdateTable(GetActiveTable());
                    commandInputField.Reset();
                    StorageTab.ResetSort();
                    workbenchTab.ResetSort();
                    cookingTab.ResetSort();
                    return;
                }
            }
            DynamicTable activeTable = GetActiveTable() as DynamicTable;
            int prevScroll = activeTable?.ScrollIndex ?? 0;
            switch (selectedTab)
            {
                case 0:
                    StorageTab.receiveRightClick(x, y, playSound);
                    break;
                case 1:
                    workbenchTab.receiveRightClick(x, y, playSound);
                    break;
                case 2:
                    cookingTab.receiveRightClick(x, y, playSound);
                    break;
                case 3:
                    shoppingTab.receiveRightClick(x, y, playSound);
                    break;
            }
            commandInputField.UpdateTable(activeTable);
            activeTable.SortItemsBy(activeTable.sortedColumn, activeTable.isAscending);
            activeTable.ScrollIndex = Math.Clamp(prevScroll, 0, Math.Max(0, activeTable.GetItemEntriesCount() - activeTable.GetVisibleRows()));
            activeTable.scrollbar.UpdateScrollBarPosition();
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (selectedTab == 0)
            {
                StorageTab.receiveScrollWheelAction(direction);
            }
            else if (selectedTab == 1)
            {
                workbenchTab.receiveScrollWheelAction(direction);
            }
            else if (selectedTab == 2)
            {
                cookingTab.receiveScrollWheelAction(direction);
            }
            else if (selectedTab != 3)
            {
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Escape && selectedTab == 1 && workbenchTab != null && workbenchTab.craftMode)
            {
                workbenchTab.CancelCraftMode();
            }
            else if (key == Keys.Escape)
            {
                exitThisMenu();
                Game1.playSound("bigDeSelect");
            }
            else
            {
                commandInputField.ReceiveKeyPress(key);
            }
        }

        public override void performHoverAction(int x, int y)
        {
            if (selectedTab == 0)
            {
                StorageTab.performHoverAction(x, y);
            }
            else if (selectedTab == 1)
            {
                workbenchTab.performHoverAction(x, y);
            }
            else if (selectedTab == 2)
            {
                cookingTab.performHoverAction(x, y);
            }
            else if (selectedTab != 3)
            {
            }
        }

        public override void update(GameTime time)
        {
            base.update(time);
            commandInputField.Update(time);
        }
    }
}
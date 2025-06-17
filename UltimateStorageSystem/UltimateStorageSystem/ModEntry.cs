using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using UltimateStorageSystem.Drawing;
using UltimateStorageSystem.Interfaces;
using UltimateStorageSystem.Tools;
using UltimateStorageSystem.Utilities;

namespace UltimateStorageSystem
{
    public class ModEntry : Mod
    {
        [HarmonyPatch(typeof(GameLocation), "draw")]
        public static class GameLocationDrawPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GameLocation __instance, SpriteBatch b)
            {
                if (BlockerOverlayTexture == null || __instance?.Objects == null)
                {
                    return;
                }
                float scale = 1f;
                foreach (KeyValuePair<Vector2, StardewValley.Object> pair in __instance.Objects.Pairs)
                {
                    Vector2 tile = pair.Key;
                    if (pair.Value is Chest chest && ChestHasBlocker(chest))
                    {
                        float drawPosX = tile.X * 64f + 32f - BlockerOverlayTexture.Width * scale / 2f - 15f;
                        float drawPosY = tile.Y * 64f - BlockerOverlayTexture.Height * scale + 50f;
                        Vector2 drawPos = Game1.GlobalToLocal(Game1.viewport, new Vector2(drawPosX, drawPosY));
                        float sortPixelY_Bottom = (tile.Y + 1f) * 64f;
                        float layerDepth = Math.Clamp((sortPixelY_Bottom - 1f) / 10000f, 0f, 1f);
                        b.Draw(BlockerOverlayTexture, drawPos, new Rectangle(0, 0, BlockerOverlayTexture.Width, BlockerOverlayTexture.Height), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
                    }
                }
                if (__instance.GetType() == typeof(FarmHouse) && __instance is FarmHouse fh)
                {
                    Chest fridge = fh.fridge.Value;
                    if (fridge != null && ChestHasBlocker(fridge))
                    {
                        Vector2 fridgeTile = fh.fridgePosition.ToVector2();
                        float overlayScale = 1f;
                        float drawPosX2 = fridgeTile.X * 64f + 32f - BlockerOverlayTexture.Width * overlayScale / 2f - 15f;
                        float drawPosY2 = (fridgeTile.Y + 1f) * 64f - BlockerOverlayTexture.Height * overlayScale - 10f;
                        Vector2 drawPos2 = Game1.GlobalToLocal(Game1.viewport, new Vector2(drawPosX2, drawPosY2));
                        float layerDepth2 = ((fridgeTile.Y + 1f) * 64f - 1f) / 10000f;
                        b.Draw(BlockerOverlayTexture, drawPos2, null, Color.White, 0f, Vector2.Zero, overlayScale, SpriteEffects.None, layerDepth2);
                    }
                }
                if (__instance is IslandFarmHouse islandHouse)
                {
                    Chest islandFridge = islandHouse.fridge.Value;
                    if (islandFridge != null && ChestHasBlocker(islandFridge))
                    {
                        Vector2 fridgeTile2 = islandHouse.fridgePosition.ToVector2();
                        float overlayScale2 = 1f;
                        float drawPosX3 = fridgeTile2.X * 64f + 32f - BlockerOverlayTexture.Width * overlayScale2 / 2f - 15f;
                        float drawPosY3 = (fridgeTile2.Y + 1f) * 64f - BlockerOverlayTexture.Height * overlayScale2 - 10f;
                        Vector2 drawPos3 = Game1.GlobalToLocal(Game1.viewport, new Vector2(drawPosX3, drawPosY3));
                        float layerDepth3 = ((fridgeTile2.Y + 1f) * 64f - 1f) / 10000f;
                        b.Draw(BlockerOverlayTexture, drawPos3, null, Color.White, 0f, Vector2.Zero, overlayScale2, SpriteEffects.None, layerDepth3);
                    }
                }
            }
        }

        public static ModEntry Instance = null!;

        public static Texture2D basketTexture = null!;

        public static Texture2D BlockerOverlayTexture = null!;

        public static VisitedLocationManager LocationTracker = null!;

        public bool ignoreNextRightClick = true;

        private ModConfig config = null!;

        private SButton? openTerminalHotkey;

        private readonly string farmLinkTerminalName = "holybananapants.UltimateStorageSystemContentPack_FarmLinkTerminal";

        private readonly Dictionary<string, string> vendorMapping = new()
        {
            { "Pierre", "SeedShop" },
            { "Marnie", "AnimalShop" },
            { "Robin", "Carpenter" },
            { "Dwarf", "Dwarf" }
        };

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            ModHelper.Init(helper);
            this.LoadConfig();
            basketTexture = helper.ModContent.Load<Texture2D>("assets/basket.png");
            BlockerOverlayTexture = helper.ModContent.Load<Texture2D>("assets/blockTerminal.png");
            LocationTracker = new VisitedLocationManager(helper);
            Harmony harmony = new(this.ModManifest.UniqueID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        }

        private void LoadConfig()
        {
            try
            {
                this.config = this.Helper.ReadConfig<ModConfig>();
                this.openTerminalHotkey = this.config.OpenFarmLinkTerminalHotkey.GetValueOrDefault();
                if (this.openTerminalHotkey == (SButton?)0)
                {
                    this.Monitor.Log("No hotkey is set for opening the FarmLink Terminal. You can set a hotkey in GenericModConfigMenu if desired.", (LogLevel)2);
                }
                this.OverrideShopData();
            }
            catch
            {
                this.openTerminalHotkey = 0;
            }
        }

        private void OverrideShopData()
        {
            ShopDataManager.UpdateShopData(this.Helper, this.Monitor, this.config.TerminalPrice, this.config.Vendor);
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu == null)
            {
                return;
            }
            configMenu.Register(this.ModManifest, delegate
            {
                this.config = new ModConfig();
            }, delegate
            {
                //IL_004d: Unknown result type (might be due to invalid IL or missing references)
                if (this.vendorMapping.TryGetValue(this.config.Vendor, out var value))
                {
                    this.config.Vendor = value;
                }
                this.Helper.WriteConfig<ModConfig>(this.config);
                this.openTerminalHotkey = this.config.OpenFarmLinkTerminalHotkey.GetValueOrDefault();
                this.OverrideShopData();
            });
            configMenu.AddSectionTitle(this.ModManifest, () => ModHelper.Helper.Translation.Get("menu.shopSettings"), () => ModHelper.Helper.Translation.Get("menu.shopSettingsTooltip"));
            configMenu.AddTextOption(this.ModManifest, () => this.vendorMapping.FirstOrDefault(x => x.Value == this.config.Vendor).Key ?? "Dwarf", delegate (string value)
            {
                if (this.vendorMapping.TryGetValue(value, out var value2))
                {
                    this.config.Vendor = value2;
                }
            }, () => ModHelper.Helper.Translation.Get("menu.vendor"), () => ModHelper.Helper.Translation.Get("menu.vendorTooltip"), this.vendorMapping.Keys.ToArray());
            configMenu.AddNumberOption(this.ModManifest, () => this.config.TerminalPrice, delegate (int value)
            {
                this.config.TerminalPrice = value;
            }, () => ModHelper.Helper.Translation.Get("menu.price"), () => ModHelper.Helper.Translation.Get("menu.priceTooltip"), 1, 100000, 1000);
            configMenu.AddParagraph(this.ModManifest, () => "");
            configMenu.AddParagraph(this.ModManifest, () => "");
            configMenu.AddParagraph(this.ModManifest, () => ModHelper.Helper.Translation.Get("menu.noticeTitle"));
            configMenu.AddParagraph(this.ModManifest, () => ModHelper.Helper.Translation.Get("menu.noticeContent"));
            configMenu.AddParagraph(this.ModManifest, () => ModHelper.Helper.Translation.Get("menu.command"));
            configMenu.AddSectionTitle(this.ModManifest, () => ModHelper.Helper.Translation.Get("menu.hotkeys"), () => ModHelper.Helper.Translation.Get("menu.hotkeysTooltip"));
            configMenu.AddKeybind(this.ModManifest, () => this.config.OpenFarmLinkTerminalHotkey.GetValueOrDefault(), delegate (SButton value)
            {
                //IL_0007: Unknown result type (might be due to invalid IL or missing references)
                //IL_0014: Unknown result type (might be due to invalid IL or missing references)
                this.config.OpenFarmLinkTerminalHotkey = value;
                this.openTerminalHotkey = value;
            }, () => ModHelper.Helper.Translation.Get("menu.hotkeyOpen"), () => ModHelper.Helper.Translation.Get("menu.hotkeyOpenTooltip"));
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            this.LoadConfig();
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (this.openTerminalHotkey.HasValue && Context.IsPlayerFree && (SButton?)e.Button == this.openTerminalHotkey)
            {
                this.ignoreNextRightClick = false;
                if (this.IsFarmLinkTerminalPlaced())
                {
                    this.OpenFarmLinkTerminalMenu();
                }
            }
            if (Context.IsPlayerFree && SButtonExtensions.IsActionButton(e.Button))
            {
                Vector2 tile = e.Cursor.Tile;
                if (this.IsFarmLinkTerminalOnTile(tile, out var terminalObject) && FarmLinkTerminal.IsPlayerBelowTileAndFacingUp(Game1.player, terminalObject.TileLocation))
                {
                    this.ignoreNextRightClick = true;
                    this.OpenFarmLinkTerminalMenu();
                }
            }
        }

        private bool IsFarmLinkTerminalPlaced()
        {
            if (Game1.locations.OfType<FarmHouse>().Any(location => location.objects.Values.Any(obj => obj.Name == this.farmLinkTerminalName)))
            {
                return true;
            }
            if (Game1.locations.OfType<Farm>().Any(location => location.objects.Values.Any(obj => obj.Name == this.farmLinkTerminalName)))
            {
                return true;
            }
            return false;
        }

        private bool IsFarmLinkTerminalOnTile(Vector2 tile, out StardewValley.Object terminalObject)
        {
            return (Game1.currentLocation.objects.TryGetValue(tile, out terminalObject) && terminalObject.Name == this.farmLinkTerminalName) || (Game1.currentLocation.objects.TryGetValue(tile + new Vector2(0f, 1f), out terminalObject) && terminalObject.Name == this.farmLinkTerminalName);
        }

        private void OpenFarmLinkTerminalMenu()
        {
            FarmLinkTerminalMenu farmLinkTerminalMenu = new(new List<Chest>(), this.Helper);
            ItemTransferManager itemTransferManager = new(farmLinkTerminalMenu.GetAllStorageObjects(), new DynamicTable(0, 0, new List<string>(), new List<int>(), new List<bool>(), new List<TableRowWithIcon>(), null));
            itemTransferManager.UpdateChestItemsAndSort();
            Game1.activeClickableMenu = farmLinkTerminalMenu;
        }

        public static bool ChestHasBlocker(Chest chest)
        {
            if (chest?.Items == null)
            {
                return false;
            }
            return chest.Items.Any(item => item is StardewValley.Object obj && obj.QualifiedItemId == "(BC)holybananapants.UltimateStorageSystemContentPack_BlockTerminal");
        }
    }
}
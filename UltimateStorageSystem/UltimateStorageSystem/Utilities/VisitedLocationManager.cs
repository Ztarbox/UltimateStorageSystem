using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace UltimateStorageSystem.Utilities
{
    public class VisitedLocationManager
    {
        private readonly IModHelper helper;

        private readonly HashSet<string> visitedLocationNames = new();

        /// <summary>
        /// IsDirty indicates whether the visited locations have been modified since the last read of <see cref="GetVisitedLocations"/>.
        /// </summary>
        public bool IsDirty { get; private set; } = true;

        public VisitedLocationManager(IModHelper helper)
        {
            this.helper = helper;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.Player.Warped += this.OnWarped;
            helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
        }

        private void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
        {
            // if removed or added item is a chest or big chest, we need to update the visited locations, cause available chests changed
            //  also if a BlockTerminal-Item is placed in a chest, we need to get rid of the chest from the "GetAllStorageObjects", so we mark it as dirty
            static bool predicate(Item item) => item is StardewValley.Objects.Chest || (item is StardewValley.Object obj && obj.QualifiedItemId == "(BC)holybananapants.UltimateStorageSystemContentPack_BlockTerminal");
            if (e.Removed.Any(predicate))
            {
                this.IsDirty = true;
            }
            else if (e.Added.Any(predicate))
            {
                this.IsDirty = true;
            }
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            var saved = this.helper.Data.ReadSaveData<List<string>>("visitedLocations");
            if (saved != null)
            {
                this.visitedLocationNames.Clear();
                foreach (string name in saved)
                {
                    this.visitedLocationNames.Add(name);
                }
            }
            foreach (GameLocation loc in Game1.locations)
            {
                this.AddAndSave(loc.NameOrUniqueName);
            }
            string[] knownExtraVanillaLocations = new string[14]
            {
                "IslandFarm", "IslandFarmHouse", "IslandNorth", "IslandSouth", "IslandWest", "IslandShrine", "MermaidHouse", "LeoTreeHouse", "BugLand", "VolcanoDungeon",
                "Cellar", "Cabin1", "Cabin2", "Cabin3"
            };
            string[] array = knownExtraVanillaLocations;
            foreach (string name2 in array)
            {
                GameLocation loc2 = Game1.getLocationFromName(name2);
                if (loc2 != null)
                {
                    this.AddAndSave(name2);
                }
            }
            if (Game1.player?.currentLocation != null)
            {
                this.AddAndSave(Game1.player.currentLocation.NameOrUniqueName);
            }
        }

        private void OnWarped(object? sender, WarpedEventArgs e)
        {
            if (e.NewLocation != null && !string.IsNullOrEmpty(e.NewLocation.NameOrUniqueName))
            {
                this.AddAndSave(e.NewLocation.NameOrUniqueName);
            }
        }

        private void AddAndSave(string name)
        {
            if (!string.IsNullOrWhiteSpace(name) && this.visitedLocationNames.Add(name))
            {
                this.helper.Data.WriteSaveData("visitedLocations", new List<string>(this.visitedLocationNames));
                this.IsDirty = true;
            }
        }

        public IEnumerable<GameLocation> GetVisitedLocations()
        {
            foreach (string name in this.visitedLocationNames)
            {
                GameLocation loc = Game1.getLocationFromName(name);
                if (loc != null)
                {
                    yield return loc;
                }
            }
            this.IsDirty = false;
        }
    }
}
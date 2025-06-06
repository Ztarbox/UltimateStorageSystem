using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace UltimateStorageSystem.Utilities
{
    public class VisitedLocationManager
    {
        private readonly IModHelper helper;

        private readonly IMonitor monitor;

        private readonly HashSet<string> visitedLocationNames = new();

        public VisitedLocationManager(IModHelper helper)
        {
            this.helper = helper;
            monitor = monitor;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.Player.Warped += OnWarped;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            List<string> saved = helper.Data.ReadSaveData<List<string>>("visitedLocations");
            if (saved != null)
            {
                visitedLocationNames.Clear();
                foreach (string name in saved)
                {
                    visitedLocationNames.Add(name);
                }
            }
            foreach (GameLocation loc in Game1.locations)
            {
                AddAndSave(loc.NameOrUniqueName);
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
                    AddAndSave(name2);
                }
            }
            if (Game1.player?.currentLocation != null)
            {
                AddAndSave(Game1.player.currentLocation.NameOrUniqueName);
            }
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.NewLocation != null && !string.IsNullOrEmpty(e.NewLocation.NameOrUniqueName))
            {
                AddAndSave(e.NewLocation.NameOrUniqueName);
            }
        }

        private void AddAndSave(string name)
        {
            if (!string.IsNullOrWhiteSpace(name) && visitedLocationNames.Add(name))
            {
                helper.Data.WriteSaveData<List<string>>("visitedLocations", new List<string>(visitedLocationNames));
            }
        }

        public IEnumerable<GameLocation> GetVisitedLocations()
        {
            foreach (string name in visitedLocationNames)
            {
                GameLocation loc = Game1.getLocationFromName(name);
                if (loc != null)
                {
                    yield return loc;
                }
            }
        }
    }
}
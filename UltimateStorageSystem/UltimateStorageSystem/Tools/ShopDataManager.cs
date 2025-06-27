using System.Text.Json;
using StardewModdingAPI;

namespace UltimateStorageSystem.Tools
{
    public static class ShopDataManager
    {
        public static void UpdateShopData(IModHelper helper, IMonitor monitor, int terminalPrice, string vendor)
        {
            var modDirectoryName = Path.GetDirectoryName(helper.DirectoryPath);
            if (modDirectoryName == null)
            {
                monitor.Log("Failed to get mod directory name.", LogLevel.Error);
                return;
            }

            string contentPackPath = Path.Combine(modDirectoryName, "[CP]UltimateStorageSystem", "data", "Shop.json");
            string jsonText = File.ReadAllText(contentPackPath);
            var shopData = JsonSerializer.Deserialize<ShopData>(jsonText);
            if (shopData != null && shopData.Changes != null && shopData.Changes.Count != 0)
            {
                if (1 == 0)
                {
                }
                string text = vendor switch
                {
                    "SeedShop" => "Items",
                    "Carpenter" => "Items",
                    "AnimalShop" => "Items",
                    "Dwarf" => "Items",
                    _ => "Items",
                };
                if (1 == 0)
                {
                }
                string itemCategory = text;
                Change firstChange = shopData.Changes[0];
                firstChange.TargetField[0] = vendor;
                firstChange.TargetField[1] = itemCategory;
                if (firstChange.LogName != null)
                {
                    firstChange.LogName = firstChange.LogName.Replace("{{TargetField[0]}}", vendor);
                }
                if (firstChange.Entries != null && firstChange.Entries.ContainsKey("{{ModID}}_FarmLinkTerminal"))
                {
                    Entry terminalEntry = firstChange.Entries["{{ModID}}_FarmLinkTerminal"];
                    terminalEntry.Price = terminalPrice;
                }
                string updatedJson = JsonSerializer.Serialize(shopData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(contentPackPath, updatedJson);
            }
        }
    }
}
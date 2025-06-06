using StardewValley;

namespace UltimateStorageSystem.Tools
{
    public class CraftingRecipeForBlockTerminal : CraftingRecipe
    {
        public CraftingRecipeForBlockTerminal()
            : base("Custom_BlockTerminal", isCookingRecipe: false)
        {
            recipeList = new Dictionary<string, int> { { "388", 1 } };
            bigCraftable = true;
        }

        public override Item createItem()
        {
            return ItemRegistry.Create("(BC)holybananapants.UltimateStorageSystemContentPack_BlockTerminal");
        }
    }
}
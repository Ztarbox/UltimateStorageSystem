using StardewModdingAPI;

namespace UltimateStorageSystem.Utilities
{
    internal static class ModHelper
    {
        private static IModHelper? _helper;

        public static IModHelper Helper
        {
            get
            {
                if (_helper == null)
                {
                    throw new InvalidOperationException("ModHelper has not been initialized. Call Init() before accessing the Helper property.");
                }
                return _helper;
            }
        }

        internal static void Init(IModHelper helper)
        {
            _helper = helper;
        }
    }
}

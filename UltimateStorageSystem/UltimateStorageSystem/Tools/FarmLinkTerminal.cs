using Microsoft.Xna.Framework;
using StardewValley;

namespace UltimateStorageSystem.Tools
{
    public static class FarmLinkTerminal
    {
        public static bool IsPlayerBelowTileAndFacingUp(Farmer player, Vector2 tile)
        {
            int playerTileX = (int)((player.Position.X + 32f + player.Sprite.SpriteWidth / 2) / 64f);
            int playerTileY = (int)((player.Position.Y + player.Sprite.SpriteHeight / 2) / 64f);
            Vector2 playerTile = new(playerTileX, playerTileY);
            bool isFacingUp = player.FacingDirection == 0;
            bool isBelowTile = playerTile == tile + new Vector2(0f, 1f);
            return isFacingUp && isBelowTile;
        }
    }
}
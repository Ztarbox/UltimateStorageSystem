using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace UltimateStorageSystem.Drawing
{
    public static class SpriteBatchExtensions
    {
        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness)
        {
            float length = Vector2.Distance(start, end);
            float rotation = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);
            spriteBatch.Draw(Game1.staminaRect, start, null, color, rotation, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0f);
        }
    }
}
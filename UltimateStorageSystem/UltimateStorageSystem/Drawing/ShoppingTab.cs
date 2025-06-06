using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace UltimateStorageSystem.Drawing
{
    public class ShoppingTab : IClickableMenu
    {
        private InventoryMenu playerInventoryMenu;

        private int containerWidth = 830;

        private int containerHeight = 900;

        private int computerMenuHeight;

        private int inventoryMenuWidth;

        private int inventoryMenuHeight = 280;

        public ShoppingTab(int xPositionOnScreen, int yPositionOnScreen)
            : base(xPositionOnScreen, yPositionOnScreen, 800, 1000)
        {
            computerMenuHeight = containerHeight - inventoryMenuHeight;
            int slotsPerRow = 12;
            int slotSize = 64;
            inventoryMenuWidth = slotsPerRow * slotSize;
            int inventoryMenuX = base.xPositionOnScreen + (containerWidth - inventoryMenuWidth) / 2;
            int inventoryMenuY = base.yPositionOnScreen + computerMenuHeight + 55;
            playerInventoryMenu = new InventoryMenu(inventoryMenuX, inventoryMenuY, playerInventory: true);
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, containerWidth, computerMenuHeight, Color.White);
            b.Draw(Game1.staminaRect, new Rectangle(xPositionOnScreen + 12, yPositionOnScreen + 12, containerWidth - 24, computerMenuHeight - 24), Color.Black);
            IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen + computerMenuHeight, containerWidth, inventoryMenuHeight, Color.White);
            playerInventoryMenu.draw(b);
            drawMouse(b);
        }
    }
}
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
            this.computerMenuHeight = this.containerHeight - this.inventoryMenuHeight;
            int slotsPerRow = 12;
            int slotSize = 64;
            this.inventoryMenuWidth = slotsPerRow * slotSize;
            int inventoryMenuX = base.xPositionOnScreen + (this.containerWidth - this.inventoryMenuWidth) / 2;
            int inventoryMenuY = base.yPositionOnScreen + this.computerMenuHeight + 55;
            this.playerInventoryMenu = new InventoryMenu(inventoryMenuX, inventoryMenuY, playerInventory: true);
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen, this.containerWidth, this.computerMenuHeight, Color.White);
            b.Draw(Game1.staminaRect, new Rectangle(this.xPositionOnScreen + 12, this.yPositionOnScreen + 12, this.containerWidth - 24, this.computerMenuHeight - 24), Color.Black);
            IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen + this.computerMenuHeight, this.containerWidth, this.inventoryMenuHeight, Color.White);
            this.playerInventoryMenu.draw(b);
            this.drawMouse(b);
        }
    }
}
using Microsoft.Xna.Framework.Input;
using StardewValley.Menus;
using UltimateStorageSystem.Drawing;

namespace UltimateStorageSystem.Tools
{
    public class InputHandler
    {
        private readonly InventoryMenu playerInventoryMenu;

        private readonly Scrollbar scrollbar;

        private readonly FarmLinkTerminalMenu terminalMenu;

        public InputHandler(InventoryMenu playerInventoryMenu, Scrollbar scrollbar, FarmLinkTerminalMenu terminalMenu)
        {
            this.playerInventoryMenu = playerInventoryMenu;
            this.scrollbar = scrollbar;
            this.terminalMenu = terminalMenu;
        }

        public void ReceiveLeftClick(int x, int y, bool playSound = true)
        {
            this.playerInventoryMenu.receiveLeftClick(x, y, playSound);
            this.scrollbar.ReceiveLeftClick(x, y);
        }

        public void LeftClickHeld(int x, int y)
        {
            this.scrollbar.LeftClickHeld(x, y);
        }

        public void ReleaseLeftClick(int x, int y)
        {
            this.scrollbar.ReleaseLeftClick(x, y);
        }

        public void PerformHoverAction(int x, int y)
        {
            this.playerInventoryMenu.performHoverAction(x, y);
        }

        public void ReceiveKeyPress(Keys key)
        {
            if (key == Keys.Escape)
            {
                this.terminalMenu.exitThisMenu();
            }
        }

        public void ReceiveScrollWheelAction(int direction)
        {
            this.scrollbar.ReceiveScrollWheelAction(direction);
        }
    }
}
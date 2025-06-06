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
            playerInventoryMenu.receiveLeftClick(x, y, playSound);
            scrollbar.ReceiveLeftClick(x, y);
        }

        public void LeftClickHeld(int x, int y)
        {
            scrollbar.LeftClickHeld(x, y);
        }

        public void ReleaseLeftClick(int x, int y)
        {
            scrollbar.ReleaseLeftClick(x, y);
        }

        public void PerformHoverAction(int x, int y)
        {
            playerInventoryMenu.performHoverAction(x, y);
        }

        public void ReceiveKeyPress(Keys key)
        {
            if (key == Keys.Escape)
            {
                terminalMenu.exitThisMenu();
            }
        }

        public void ReceiveScrollWheelAction(int direction)
        {
            scrollbar.ReceiveScrollWheelAction(direction);
        }
    }
}
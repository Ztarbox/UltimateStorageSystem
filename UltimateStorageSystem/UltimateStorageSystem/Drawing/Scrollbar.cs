using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using UltimateStorageSystem.Interfaces;

namespace UltimateStorageSystem.Drawing
{
    public class Scrollbar
    {
        private readonly Rectangle scrollBarRunner;

        private Rectangle scrollBar;

        private bool isScrolling = false;

        private int dragOffset = 0;

        private readonly IScrollableTable table;

        public Scrollbar(int x, int y, IScrollableTable table)
        {
            this.table = table;
            scrollBarRunner = new Rectangle(x - 5, y + 40, 20, 400);
            scrollBar = new Rectangle(scrollBarRunner.X, scrollBarRunner.Y, 20, 20);
        }

        public void Draw(SpriteBatch b)
        {
            b.Draw(Game1.staminaRect, new Rectangle(scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height), Color.Gray);
            b.Draw(Game1.staminaRect, scrollBar, Color.White);
        }

        public void ReceiveLeftClick(int x, int y)
        {
            if (scrollBar.Contains(x, y))
            {
                isScrolling = true;
                dragOffset = y - scrollBar.Y;
            }
            else if (scrollBarRunner.Contains(x, y))
            {
                isScrolling = true;
                dragOffset = scrollBar.Height / 2;
                HandleDrag(y);
            }
        }

        public void LeftClickHeld(int x, int y)
        {
            if (isScrolling)
            {
                HandleDrag(y);
            }
        }

        public void ReleaseLeftClick(int _x, int _y)
        {
            isScrolling = false;
        }

        public void ReceiveScrollWheelAction(int direction)
        {
            int scrollAmount = ((direction <= 0) ? 1 : (-1));
            int itemCount = table.GetItemEntriesCount();
            int visibleRows = table.GetVisibleRows();
            table.ScrollIndex = Math.Clamp(table.ScrollIndex + scrollAmount, 0, Math.Max(0, itemCount - visibleRows));
            UpdateScrollBarPosition();
        }

        public void UpdatePosition(float proportionVisible, int scrollIndex, int maxScrollIndex)
        {
            scrollBar.Height = (int)(scrollBarRunner.Height * proportionVisible);
            float percent = scrollIndex / (float)maxScrollIndex;
            scrollBar.Y = scrollBarRunner.Y + (int)(percent * (scrollBarRunner.Height - scrollBar.Height));
        }

        public void SetToMaxSize()
        {
            scrollBar.Y = scrollBarRunner.Y;
            scrollBar.Height = scrollBarRunner.Height;
        }

        private void UpdateScrollBar(int y)
        {
            float percent = (y - scrollBarRunner.Y) / (float)(scrollBarRunner.Height - scrollBar.Height);
            int itemCount = table.GetItemEntriesCount();
            int visibleRows = table.GetVisibleRows();
            table.ScrollIndex = (int)(percent * Math.Max(0, itemCount - visibleRows));
            table.ScrollIndex = Math.Clamp(table.ScrollIndex, 0, Math.Max(0, itemCount - visibleRows));
            UpdateScrollBarPosition();
        }

        public void UpdateScrollBarPosition()
        {
            int itemCount = table.GetItemEntriesCount();
            int visibleRows = table.GetVisibleRows();
            if (itemCount > visibleRows)
            {
                float proportionVisible = visibleRows / (float)itemCount;
                UpdatePosition(proportionVisible, table.ScrollIndex, itemCount - visibleRows);
            }
            else
            {
                SetToMaxSize();
            }
        }

        private void HandleDrag(int mouseY)
        {
            int runnerHeight = scrollBarRunner.Height;
            int barHeight = scrollBar.Height;
            float relative = mouseY - scrollBarRunner.Y - dragOffset;
            relative = Math.Clamp(relative, 0f, runnerHeight - barHeight);
            int itemCount = table.GetItemEntriesCount();
            int visibleRows = table.GetVisibleRows();
            if (itemCount > visibleRows)
            {
                float pct = relative / (runnerHeight - barHeight);
                int maxScroll = itemCount - visibleRows;
                table.ScrollIndex = Math.Clamp((int)(pct * maxScroll), 0, maxScroll);
                UpdateScrollBarPosition();
            }
        }
    }
}
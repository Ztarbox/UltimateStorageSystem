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
            this.scrollBarRunner = new Rectangle(x - 5, y + 40, 20, 400);
            this.scrollBar = new Rectangle(this.scrollBarRunner.X, this.scrollBarRunner.Y, 20, 20);
        }

        public void Draw(SpriteBatch b)
        {
            b.Draw(Game1.staminaRect, new Rectangle(this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height), Color.Gray);
            b.Draw(Game1.staminaRect, this.scrollBar, Color.White);
        }

        public void ReceiveLeftClick(int x, int y)
        {
            if (this.scrollBar.Contains(x, y))
            {
                this.isScrolling = true;
                this.dragOffset = y - this.scrollBar.Y;
            }
            else if (this.scrollBarRunner.Contains(x, y))
            {
                this.isScrolling = true;
                this.dragOffset = this.scrollBar.Height / 2;
                this.HandleDrag(y);
            }
        }

        public void LeftClickHeld(int x, int y)
        {
            if (this.isScrolling)
            {
                this.HandleDrag(y);
            }
        }

        public void ReleaseLeftClick(int _x, int _y)
        {
            this.isScrolling = false;
        }

        public void ReceiveScrollWheelAction(int direction)
        {
            int scrollAmount = ((direction <= 0) ? 1 : (-1));
            int itemCount = this.table.GetItemEntriesCount();
            int visibleRows = this.table.GetVisibleRows();
            this.table.ScrollIndex = Math.Clamp(this.table.ScrollIndex + scrollAmount, 0, Math.Max(0, itemCount - visibleRows));
            this.UpdateScrollBarPosition();
        }

        public void UpdatePosition(float proportionVisible, int scrollIndex, int maxScrollIndex)
        {
            this.scrollBar.Height = (int)(this.scrollBarRunner.Height * proportionVisible);
            float percent = scrollIndex / (float)maxScrollIndex;
            this.scrollBar.Y = this.scrollBarRunner.Y + (int)(percent * (this.scrollBarRunner.Height - this.scrollBar.Height));
        }

        public void SetToMaxSize()
        {
            this.scrollBar.Y = this.scrollBarRunner.Y;
            this.scrollBar.Height = this.scrollBarRunner.Height;
        }

        private void UpdateScrollBar(int y)
        {
            float percent = (y - this.scrollBarRunner.Y) / (float)(this.scrollBarRunner.Height - this.scrollBar.Height);
            int itemCount = this.table.GetItemEntriesCount();
            int visibleRows = this.table.GetVisibleRows();
            this.table.ScrollIndex = (int)(percent * Math.Max(0, itemCount - visibleRows));
            this.table.ScrollIndex = Math.Clamp(this.table.ScrollIndex, 0, Math.Max(0, itemCount - visibleRows));
            this.UpdateScrollBarPosition();
        }

        public void UpdateScrollBarPosition()
        {
            int itemCount = this.table.GetItemEntriesCount();
            int visibleRows = this.table.GetVisibleRows();
            if (itemCount > visibleRows)
            {
                float proportionVisible = visibleRows / (float)itemCount;
                this.UpdatePosition(proportionVisible, this.table.ScrollIndex, itemCount - visibleRows);
            }
            else
            {
                this.SetToMaxSize();
            }
        }

        private void HandleDrag(int mouseY)
        {
            int runnerHeight = this.scrollBarRunner.Height;
            int barHeight = this.scrollBar.Height;
            float relative = mouseY - this.scrollBarRunner.Y - this.dragOffset;
            relative = Math.Clamp(relative, 0f, runnerHeight - barHeight);
            int itemCount = this.table.GetItemEntriesCount();
            int visibleRows = this.table.GetVisibleRows();
            if (itemCount > visibleRows)
            {
                float pct = relative / (runnerHeight - barHeight);
                int maxScroll = itemCount - visibleRows;
                this.table.ScrollIndex = Math.Clamp((int)(pct * maxScroll), 0, maxScroll);
                this.UpdateScrollBarPosition();
            }
        }
    }
}
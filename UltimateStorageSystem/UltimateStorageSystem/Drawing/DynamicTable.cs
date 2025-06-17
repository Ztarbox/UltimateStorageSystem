using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using UltimateStorageSystem.Interfaces;
using UltimateStorageSystem.Utilities;

namespace UltimateStorageSystem.Drawing
{
    public class DynamicTable : IScrollableTable, IFilterableTable
    {
        private readonly List<string> ColumnHeaders;

        public List<int> ColumnWidths;

        public List<bool> ColumnAlignments;

        private readonly bool showCraftingQuantity;

        public List<TableRowWithIcon> AllRows;

        private List<TableRowWithIcon> FilteredRows;

        public Scrollbar? Scrollbar;

        private bool previousMousePressed = false;

        public int StartX { get; }

        public int StartY { get; }

        public int ScrollIndex { get; set; } = 0;

        public string SortedColumn { get; private set; } = "";

        public bool isAscending { get; private set; }

        private List<int> ColumnPositions { get; set; }

        public DynamicTable(int startX, int startY, List<string> columnHeaders, List<int> columnWidths, List<bool> columnAlignments, List<TableRowWithIcon> rows, Scrollbar? scrollbar = null, bool showCraftingQuantity = false)
        {
            this.StartX = startX;
            this.StartY = startY;
            this.ColumnHeaders = columnHeaders;
            this.ColumnWidths = columnWidths;
            this.ColumnAlignments = columnAlignments;
            this.AllRows = new List<TableRowWithIcon>(rows);
            this.FilteredRows = new List<TableRowWithIcon>(rows);
            this.Scrollbar = scrollbar;
            this.showCraftingQuantity = showCraftingQuantity;
            if (this.ColumnHeaders.Count != this.ColumnWidths.Count || this.ColumnHeaders.Count != this.ColumnAlignments.Count)
            {
                throw new ArgumentException("Die Anzahl der Spaltenüberschriften, Spaltenbreiten und Ausrichtungen muss übereinstimmen.");
            }
            this.ColumnPositions = new List<int>();
            int currentX = this.StartX;
            for (int i = 0; i < columnWidths.Count; i++)
            {
                this.ColumnPositions.Add(currentX);
                currentX += columnWidths[i];
                if (i < columnWidths.Count - 1)
                {
                    currentX += 10;
                }
            }
        }

        public int GetItemEntriesCount()
        {
            return this.FilteredRows.Count;
        }

        public int GetVisibleRows()
        {
            return 13;
        }

        public int TotalTableWidth()
        {
            return this.ColumnWidths.Sum() + (this.ColumnHeaders.Count - 1) * 10;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            this.DrawHeaders(spriteBatch);
            this.DrawRows(spriteBatch);
            this.Scrollbar?.Draw(spriteBatch);
        }

        private void DrawHeaders(SpriteBatch spriteBatch)
        {
            int headerY = this.StartY + 40;
            bool isMousePressed = Game1.input.GetMouseState().LeftButton == ButtonState.Pressed;
            for (int i = 0; i < this.ColumnHeaders.Count; i++)
            {
                string header = this.ColumnHeaders[i];
                int columnWidth = this.ColumnWidths[i];
                bool isSortedColumn = header == this.SortedColumn;
                bool alignRight = this.ColumnAlignments[i];
                Vector2 headerPosition = new(this.ColumnPositions[i], headerY);
                this.DrawHeaderWithSortIcon(spriteBatch, header, headerPosition, Color.White, isSortedColumn, this.isAscending, alignRight);
                if (new Rectangle((int)headerPosition.X, (int)headerPosition.Y, columnWidth, 35).Contains(Game1.getMouseX(), Game1.getMouseY()) && !isMousePressed && this.previousMousePressed)
                {
                    this.ToggleSort(header);
                }
            }
            this.previousMousePressed = isMousePressed;
            spriteBatch.DrawLine(new Vector2(this.StartX, headerY + 35), new Vector2(this.StartX + this.TotalTableWidth(), headerY + 35), Color.White, 1f);
        }

        private void DrawHeaderWithSortIcon(SpriteBatch spriteBatch, string text, Vector2 position, Color color, bool isSortedColumn, bool isAscending, bool alignRight)
        {
            int columnIndex = this.ColumnHeaders.IndexOf(text);
            int columnWidth = this.ColumnWidths[columnIndex];
            Vector2 textSize = Game1.smallFont.MeasureString(text);
            Vector2 iconPosition = position;
            Rectangle sourceRect = (isAscending ? new Rectangle(421, 472, 12, 12) : new Rectangle(421, 459, 12, 12));
            float iconScale = 2f;
            float iconWidth = 12f * iconScale;
            float spacing = 5f;
            if (alignRight)
            {
                float textXPosition = (position.X = position.X + columnWidth - textSize.X);
                if (isSortedColumn)
                {
                    iconPosition.X = textXPosition - iconWidth - spacing;
                    iconPosition.Y += 3f;
                    spriteBatch.Draw(Game1.mouseCursors, iconPosition, sourceRect, Color.White, 0f, Vector2.Zero, iconScale, SpriteEffects.None, 0f);
                }
                spriteBatch.DrawString(Game1.smallFont, text, position, color);
            }
            else
            {
                spriteBatch.DrawString(Game1.smallFont, text, position, color);
                if (isSortedColumn)
                {
                    iconPosition.X = position.X + textSize.X + spacing;
                    iconPosition.Y += 3f;
                    spriteBatch.Draw(Game1.mouseCursors, iconPosition, sourceRect, Color.White, 0f, Vector2.Zero, iconScale, SpriteEffects.None, 0f);
                }
            }
        }

        private void DrawRows(SpriteBatch spriteBatch)
        {
            int contentStartY = this.StartY + 35 + 60;
            for (int i = 0; i < this.GetVisibleRows(); i++)
            {
                int index = this.ScrollIndex + i;
                if (index >= this.FilteredRows.Count)
                {
                    break;
                }
                int rowY = contentStartY + 32 * i;
                TableRowWithIcon rowContent = this.FilteredRows[index];
                bool isHovered = new Rectangle(this.StartX, rowY, this.TotalTableWidth(), 32).Contains(Game1.getMouseX(), Game1.getMouseY());
                if (isHovered)
                {
                    spriteBatch.Draw(Game1.staminaRect, new Rectangle(this.StartX, rowY, this.TotalTableWidth(), 32), Color.White * 0.15f);
                }
                this.DrawRowContent(spriteBatch, this.StartX, rowY, rowContent, isHovered);
            }
        }

        private void DrawRowContent(SpriteBatch spriteBatch, int startX, int rowY, TableRowWithIcon row, bool isHovered)
        {
            for (int i = 0; i < row.Cells.Count; i++)
            {
                string cellText = row.Cells[i];
                int columnWidth = this.ColumnWidths[i];
                bool alignRight = this.ColumnAlignments[i];
                Color textColor = Color.White;
                Vector2 position = new(this.ColumnPositions[i], rowY);
                if (i == 0 && row.ItemIcon != null)
                {
                    if (row.ItemIcon is StardewValley.Object obj)
                    {
                        textColor = obj.Quality switch
                        {
                            1 => Color.Gray,
                            2 => Color.Gold,
                            4 => Color.MediumPurple,
                            _ => Color.White,
                        };
                    }
                    float iconScale = 0.375f;
                    int iconWidth = (int)(64f * iconScale);
                    Vector2 iconPosition = new(position.X - 16f, rowY - 16);
                    var itemObj = row.ItemIcon as StardewValley.Object;
                    if (row.ItemIcon is Ring || row.ItemIcon is Boots || (itemObj != null && itemObj.preserve.Value.HasValue && this.IsPreservedItemToShift(itemObj.preserve.Value.Value)))
                    {
                        iconPosition.Y += 10f;
                        iconPosition.X += 10f;
                    }
                    else if (row.ItemIcon is WateringCan)
                    {
                        iconPosition.Y += 8f;
                    }
                    else if (row.ItemIcon.ParentSheetIndex == 597 || row.ItemIcon.ParentSheetIndex == 593 || row.ItemIcon.ParentSheetIndex == 376 || row.ItemIcon.ParentSheetIndex == 595)
                    {
                        iconPosition.Y += 10f;
                        iconPosition.X += 10f;
                    }
                    row.ItemIcon.drawInMenu(spriteBatch, iconPosition, iconScale, 1f, 0.9f, StackDrawType.Hide, Color.White, drawShadow: false);
                    position.X += iconWidth + 15;
                    columnWidth -= iconWidth + 5;
                }
                if (i == 0 && this.showCraftingQuantity && row.ItemIcon?.maximumStackSize() > 1 && row.ItemIcon.Stack > 1)
                {
                    cellText += $" ({row.ItemIcon.Stack})";
                }
                this.DrawCellText(spriteBatch, cellText, position, columnWidth, alignRight, isHovered, textColor);
            }
        }

        private bool IsPreservedItemToShift(StardewValley.Object.PreserveType preserveType)
        {
            return preserveType == StardewValley.Object.PreserveType.Wine || preserveType == StardewValley.Object.PreserveType.Jelly || preserveType == StardewValley.Object.PreserveType.Pickle || preserveType == StardewValley.Object.PreserveType.Juice || preserveType == StardewValley.Object.PreserveType.Roe || preserveType == StardewValley.Object.PreserveType.AgedRoe || preserveType == StardewValley.Object.PreserveType.Honey || preserveType == StardewValley.Object.PreserveType.Bait || preserveType == StardewValley.Object.PreserveType.DriedFruit || preserveType == StardewValley.Object.PreserveType.DriedMushroom || preserveType == StardewValley.Object.PreserveType.SmokedFish;
        }

        private void DrawCellText(SpriteBatch spriteBatch, string cellText, Vector2 position, int columnWidth, bool alignRight, bool isHovered, Color textColor)
        {
            float textWidth = Game1.smallFont.MeasureString(cellText).X;
            bool needsScrolling = textWidth > columnWidth - 10;
            if (isHovered && needsScrolling)
            {
                this.DrawScrollingText(spriteBatch, cellText, position, textColor, columnWidth - 10);
                return;
            }
            string displayText = cellText;
            if (needsScrolling)
            {
                while (Game1.smallFont.MeasureString(displayText + "...").X > columnWidth - 10 && displayText.Length > 0)
                {
                    displayText = displayText.Substring(0, displayText.Length - 1);
                }
                displayText += "...";
            }
            this.DrawText(spriteBatch, displayText, position, textColor, alignRight, columnWidth);
        }

        private void DrawText(SpriteBatch spriteBatch, string text, Vector2 position, Color color, bool alignRight, int columnWidth)
        {
            Vector2 textSize = Game1.smallFont.MeasureString(text);
            if (alignRight)
            {
                position.X += columnWidth - textSize.X - 5f;
            }
            else
            {
                position.X += 5f;
            }
            spriteBatch.DrawString(Game1.smallFont, text, position, color);
        }

        private void DrawScrollingText(SpriteBatch spriteBatch, string fullText, Vector2 position, Color color, int maxVisibleWidth)
        {
            Rectangle oldScissorRect = spriteBatch.GraphicsDevice.ScissorRectangle;
            Rectangle scissor = new((int)position.X, (int)position.Y, maxVisibleWidth, 32);
            scissor = Rectangle.Intersect(scissor, spriteBatch.GraphicsDevice.Viewport.Bounds);
            RasterizerState rasterizerStateScissor = new()
            {
                ScissorTestEnable = true
            };
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, rasterizerStateScissor, null, Matrix.Identity);
            spriteBatch.GraphicsDevice.ScissorRectangle = scissor;
            float textWidth = Game1.smallFont.MeasureString(fullText).X;
            float scrollSpeed = 50f;
            float time = (float)Game1.currentGameTime.TotalGameTime.TotalSeconds;
            float scrollArea = Math.Max(0f, textWidth - maxVisibleWidth);
            float scrollOffset = 0f;
            if (scrollArea > 0f)
            {
                float t = time * scrollSpeed % (scrollArea * 2f);
                scrollOffset = ((t <= scrollArea) ? t : (2f * scrollArea - t));
            }
            spriteBatch.DrawString(position: new Vector2(position.X - scrollOffset, position.Y), spriteFont: Game1.smallFont, text: fullText, color: color);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            spriteBatch.GraphicsDevice.ScissorRectangle = oldScissorRect;
        }

        private void ToggleSort(string column)
        {
            if (this.SortedColumn == column)
            {
                this.isAscending = !this.isAscending;
            }
            else
            {
                this.SortedColumn = column;
                this.isAscending = true;
            }
            this.SortItemsBy(this.SortedColumn, this.isAscending);
        }

        public void SortItemsBy(string? column, bool ascending)
        {
            if (column == null)
            {
                return;
            }
            int colIndex = this.ColumnHeaders.IndexOf(column);
            if (colIndex == -1)
            {
                return;
            }
            bool isNumeric = this.FilteredRows.All(row => int.TryParse(row.Cells[colIndex], out int result));
            this.FilteredRows.Sort(delegate (TableRowWithIcon a, TableRowWithIcon b)
            {
                string text = a.Cells[colIndex] ?? string.Empty;
                string text2 = b.Cells[colIndex] ?? string.Empty;
                if (isNumeric)
                {
                    int value = int.Parse(text);
                    int value2 = int.Parse(text2);
                    return ascending ? value.CompareTo(value2) : value2.CompareTo(value);
                }
                int num = string.Compare(text, text2, StringComparison.OrdinalIgnoreCase);
                return ascending ? num : (-num);
            });
        }

        public void FilterItems(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                this.FilteredRows = new List<TableRowWithIcon>(this.AllRows);
            }
            else
            {
                this.FilteredRows = this.AllRows.FindAll(row => row.Cells[0].IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            this.FilteredRows = (from row in this.FilteredRows
                            orderby row.ItemIcon?.DisplayName ?? "", (row.ItemIcon as StardewValley.Object)?.Quality ?? 0 descending
                            select row).ToList();
            this.ScrollIndex = 0;
            this.Scrollbar?.UpdateScrollBarPosition();
        }

        public void HandleHeaderClick(int x, int y)
        {
            for (int i = 0; i < this.ColumnHeaders.Count; i++)
            {
                int offsetX = this.StartX + this.ColumnWidths.Take(i).Sum() + 10 * i;
                if (new Rectangle(offsetX, this.StartY + 40, this.ColumnWidths[i], 35).Contains(x, y))
                {
                    this.ToggleSort(this.ColumnHeaders[i]);
                    break;
                }
            }
        }

        public Item? GetClickedItem(int x, int y)
        {
            int contentStartY = this.StartY + 35 + 60;
            for (int i = 0; i < this.GetVisibleRows(); i++)
            {
                int index = this.ScrollIndex + i;
                if (index >= this.FilteredRows.Count)
                {
                    break;
                }
                int rowY = contentStartY + 32 * i;
                if (new Rectangle(this.StartX, rowY, this.TotalTableWidth(), 32).Contains(x, y))
                {
                    return this.FilteredRows[index].ItemIcon;
                }
            }
            return null;
        }

        public void Update()
        {
            this.Scrollbar?.UpdateScrollBarPosition();
        }

        public void ReceiveScrollWheelAction(int direction)
        {
            int scrollAmount = ((direction <= 0) ? 1 : (-1));
            this.ScrollIndex = Math.Clamp(this.ScrollIndex + scrollAmount, 0, Math.Max(0, this.FilteredRows.Count - this.GetVisibleRows()));
            this.Scrollbar?.UpdateScrollBarPosition();
        }

        public void ResetSort()
        {
            if (this.ColumnHeaders.Count > 0)
            {
                this.SortedColumn = this.ColumnHeaders[0];
                this.isAscending = true;
                this.SortItemsBy(this.SortedColumn, this.isAscending);
            }
        }

        public void OpenTerminal()
        {
            this.ResetSort();
        }

        public void CloseTerminal()
        {
            this.ResetSort();
        }

        public void AddItem(ItemEntry item)
        {
            this.AllRows.Add(new TableRowWithIcon(item.Item, new List<string>
            {
                item.Name,
                item.Quantity.ToString(),
                item.SingleValue.ToString(),
                item.TotalValue.ToString()
            }));
            this.FilteredRows.Add(new TableRowWithIcon(item.Item, new List<string>
            {
                item.Name,
                item.Quantity.ToString(),
                item.SingleValue.ToString(),
                item.TotalValue.ToString()
            }));
        }

        public List<ItemEntry> GetItemEntries()
        {
            return this.FilteredRows.Select(row => new ItemEntry(row.Cells[0], int.Parse(row.Cells[1]), int.Parse(row.Cells[2]), int.Parse(row.Cells[3]), row.ItemIcon)).ToList();
        }

        public void Refresh()
        {
            this.FilteredRows = new List<TableRowWithIcon>(this.AllRows);
        }

        public void ClearItems()
        {
            this.AllRows.Clear();
            this.FilteredRows.Clear();
        }

        public void UpdateItemList(Item item, int remainingAmount)
        {
            var entry = this.AllRows.Find(row => row.ItemIcon == item);
            if (entry != null)
            {
                if (remainingAmount <= 0)
                {
                    this.AllRows.Remove(entry);
                }
                else
                {
                    entry.Cells[1] = remainingAmount.ToString();
                    entry.Cells[3] = (int.Parse(entry.Cells[2]) * remainingAmount).ToString();
                }
            }
            else if (remainingAmount > 0)
            {
                this.AddItem(new ItemEntry(item.DisplayName, remainingAmount, item.salePrice(), item.salePrice() * remainingAmount, item));
            }
            this.Refresh();
        }
    }
}
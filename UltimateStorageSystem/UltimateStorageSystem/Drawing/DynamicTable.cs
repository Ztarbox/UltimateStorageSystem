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
        private List<string> ColumnHeaders;

        public List<int> ColumnWidths;

        public List<bool> ColumnAlignments;

        private bool showCraftingQuantity;

        public List<TableRowWithIcon> AllRows;

        private List<TableRowWithIcon> FilteredRows;

        public Scrollbar scrollbar;

        private int hoverRowIndex = -1;

        private const int HeaderHeight = 35;

        private const int RowHeight = 32;

        private const int MaxTextLength = 15;

        private bool previousMousePressed = false;

        public int StartX { get; }

        public int StartY { get; }

        public int ScrollIndex { get; set; } = 0;

        public string sortedColumn { get; private set; }

        public bool isAscending { get; private set; }

        private List<int> ColumnPositions { get; set; }

        public DynamicTable(int startX, int startY, List<string> columnHeaders, List<int> columnWidths, List<bool> columnAlignments, List<TableRowWithIcon> rows, Scrollbar scrollbar, bool showCraftingQuantity = false)
        {
            StartX = startX;
            StartY = startY;
            ColumnHeaders = columnHeaders;
            ColumnWidths = columnWidths;
            ColumnAlignments = columnAlignments;
            AllRows = new List<TableRowWithIcon>(rows);
            FilteredRows = new List<TableRowWithIcon>(rows);
            this.scrollbar = scrollbar;
            this.showCraftingQuantity = showCraftingQuantity;
            if (ColumnHeaders.Count != ColumnWidths.Count || ColumnHeaders.Count != ColumnAlignments.Count)
            {
                throw new ArgumentException("Die Anzahl der Spaltenüberschriften, Spaltenbreiten und Ausrichtungen muss übereinstimmen.");
            }
            ColumnPositions = new List<int>();
            int currentX = StartX;
            for (int i = 0; i < columnWidths.Count; i++)
            {
                ColumnPositions.Add(currentX);
                currentX += columnWidths[i];
                if (i < columnWidths.Count - 1)
                {
                    currentX += 10;
                }
            }
        }

        public int GetItemEntriesCount()
        {
            return FilteredRows.Count;
        }

        public int GetVisibleRows()
        {
            return 13;
        }

        public int TotalTableWidth()
        {
            return ColumnWidths.Sum() + (ColumnHeaders.Count - 1) * 10;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawHeaders(spriteBatch);
            DrawRows(spriteBatch);
            scrollbar?.Draw(spriteBatch);
        }

        private void DrawHeaders(SpriteBatch spriteBatch)
        {
            int headerY = StartY + 40;
            bool isMousePressed = Game1.input.GetMouseState().LeftButton == ButtonState.Pressed;
            for (int i = 0; i < ColumnHeaders.Count; i++)
            {
                string header = ColumnHeaders[i];
                int columnWidth = ColumnWidths[i];
                bool isSortedColumn = header == sortedColumn;
                bool alignRight = ColumnAlignments[i];
                Vector2 headerPosition = new(ColumnPositions[i], headerY);
                DrawHeaderWithSortIcon(spriteBatch, header, headerPosition, Color.White, isSortedColumn, isAscending, alignRight);
                if (new Rectangle((int)headerPosition.X, (int)headerPosition.Y, columnWidth, 35).Contains(Game1.getMouseX(), Game1.getMouseY()) && !isMousePressed && previousMousePressed)
                {
                    ToggleSort(header);
                }
            }
            previousMousePressed = isMousePressed;
            spriteBatch.DrawLine(new Vector2(StartX, headerY + 35), new Vector2(StartX + TotalTableWidth(), headerY + 35), Color.White, 1f);
        }

        private void DrawHeaderWithSortIcon(SpriteBatch spriteBatch, string text, Vector2 position, Color color, bool isSortedColumn, bool isAscending, bool alignRight)
        {
            int columnIndex = ColumnHeaders.IndexOf(text);
            int columnWidth = ColumnWidths[columnIndex];
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
            int contentStartY = StartY + 35 + 60;
            for (int i = 0; i < GetVisibleRows(); i++)
            {
                int index = ScrollIndex + i;
                if (index >= FilteredRows.Count)
                {
                    break;
                }
                int rowY = contentStartY + 32 * i;
                TableRowWithIcon rowContent = FilteredRows[index];
                bool isHovered = new Rectangle(StartX, rowY, TotalTableWidth(), 32).Contains(Game1.getMouseX(), Game1.getMouseY());
                if (isHovered)
                {
                    hoverRowIndex = i;
                    spriteBatch.Draw(Game1.staminaRect, new Rectangle(StartX, rowY, TotalTableWidth(), 32), Color.White * 0.15f);
                }
                DrawRowContent(spriteBatch, StartX, rowY, rowContent, isHovered);
            }
        }

        private void DrawRowContent(SpriteBatch spriteBatch, int startX, int rowY, TableRowWithIcon row, bool isHovered)
        {
            for (int i = 0; i < row.Cells.Count; i++)
            {
                string cellText = row.Cells[i];
                int columnWidth = ColumnWidths[i];
                bool alignRight = ColumnAlignments[i];
                Color textColor = Color.White;
                Vector2 position = new(ColumnPositions[i], rowY);
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
                    StardewValley.Object itemObj = row.ItemIcon as StardewValley.Object;
                    if (row.ItemIcon is Ring || row.ItemIcon is Boots || (itemObj != null && itemObj.preserve.Value.HasValue && IsPreservedItemToShift(itemObj.preserve.Value.Value)))
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
                if (i == 0 && showCraftingQuantity && row.ItemIcon.maximumStackSize() > 1 && row.ItemIcon.Stack > 1)
                {
                    cellText += $" ({row.ItemIcon.Stack})";
                }
                DrawCellText(spriteBatch, cellText, position, columnWidth, alignRight, isHovered, textColor);
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
                DrawScrollingText(spriteBatch, cellText, position, textColor, columnWidth - 10);
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
            DrawText(spriteBatch, displayText, position, textColor, alignRight, columnWidth);
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
            if (sortedColumn == column)
            {
                isAscending = !isAscending;
            }
            else
            {
                sortedColumn = column;
                isAscending = true;
            }
            SortItemsBy(sortedColumn, isAscending);
        }

        public void SortItemsBy(string column, bool ascending)
        {
            int colIndex = ColumnHeaders.IndexOf(column);
            if (colIndex == -1)
            {
                return;
            }
            int result;
            bool isNumeric = FilteredRows.All((TableRowWithIcon row) => int.TryParse(row.Cells[colIndex], out result));
            FilteredRows.Sort(delegate (TableRowWithIcon a, TableRowWithIcon b)
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
                FilteredRows = new List<TableRowWithIcon>(AllRows);
            }
            else
            {
                FilteredRows = AllRows.FindAll((TableRowWithIcon row) => row.Cells[0].IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            FilteredRows = (from row in FilteredRows
                            orderby row.ItemIcon?.DisplayName ?? "", (row.ItemIcon as StardewValley.Object)?.Quality ?? 0 descending
                            select row).ToList();
            ScrollIndex = 0;
            scrollbar?.UpdateScrollBarPosition();
        }

        public void HandleHeaderClick(int x, int y)
        {
            for (int i = 0; i < ColumnHeaders.Count; i++)
            {
                int offsetX = StartX + ColumnWidths.Take(i).Sum() + 10 * i;
                if (new Rectangle(offsetX, StartY + 40, ColumnWidths[i], 35).Contains(x, y))
                {
                    ToggleSort(ColumnHeaders[i]);
                    break;
                }
            }
        }

        public void PerformHoverAction(int x, int y)
        {
            hoverRowIndex = -1;
            for (int i = 0; i < GetVisibleRows(); i++)
            {
                int index = ScrollIndex + i;
                if (index >= FilteredRows.Count)
                {
                    break;
                }
                int rowY = StartY + 35 + 32 * i + 10;
                if (new Rectangle(StartX, rowY, TotalTableWidth(), 32).Contains(x, y))
                {
                    hoverRowIndex = i;
                    break;
                }
            }
        }

        public Item GetClickedItem(int x, int y)
        {
            int contentStartY = StartY + 35 + 60;
            for (int i = 0; i < GetVisibleRows(); i++)
            {
                int index = ScrollIndex + i;
                if (index >= FilteredRows.Count)
                {
                    break;
                }
                int rowY = contentStartY + 32 * i;
                if (new Rectangle(StartX, rowY, TotalTableWidth(), 32).Contains(x, y))
                {
                    return FilteredRows[index].ItemIcon;
                }
            }
            return null;
        }

        public void Update()
        {
            scrollbar.UpdateScrollBarPosition();
        }

        public void ReceiveScrollWheelAction(int direction)
        {
            int scrollAmount = ((direction <= 0) ? 1 : (-1));
            ScrollIndex = Math.Clamp(ScrollIndex + scrollAmount, 0, Math.Max(0, FilteredRows.Count - GetVisibleRows()));
            scrollbar.UpdateScrollBarPosition();
        }

        public void ResetSort()
        {
            if (ColumnHeaders.Count > 0)
            {
                sortedColumn = ColumnHeaders[0];
                isAscending = true;
                SortItemsBy(sortedColumn, isAscending);
            }
        }

        public void OpenTerminal()
        {
            ResetSort();
        }

        public void CloseTerminal()
        {
            ResetSort();
        }

        public void AddItem(ItemEntry item)
        {
            AllRows.Add(new TableRowWithIcon(item.Item, new List<string>
        {
            item.Name,
            item.Quantity.ToString(),
            item.SingleValue.ToString(),
            item.TotalValue.ToString()
        }));
            FilteredRows.Add(new TableRowWithIcon(item.Item, new List<string>
        {
            item.Name,
            item.Quantity.ToString(),
            item.SingleValue.ToString(),
            item.TotalValue.ToString()
        }));
        }

        public List<ItemEntry> GetItemEntries()
        {
            return FilteredRows.Select((TableRowWithIcon row) => new ItemEntry(row.Cells[0], int.Parse(row.Cells[1]), int.Parse(row.Cells[2]), int.Parse(row.Cells[3]), row.ItemIcon)).ToList();
        }

        public void Refresh()
        {
            FilteredRows = new List<TableRowWithIcon>(AllRows);
        }

        public void ClearItems()
        {
            AllRows.Clear();
            FilteredRows.Clear();
        }

        public void UpdateItemList(Item item, int remainingAmount)
        {
            TableRowWithIcon entry = AllRows.Find((TableRowWithIcon row) => row.ItemIcon == item);
            if (entry != null)
            {
                if (remainingAmount <= 0)
                {
                    AllRows.Remove(entry);
                }
                else
                {
                    entry.Cells[1] = remainingAmount.ToString();
                    entry.Cells[3] = (int.Parse(entry.Cells[2]) * remainingAmount).ToString();
                }
            }
            else if (remainingAmount > 0)
            {
                AddItem(new ItemEntry(item.DisplayName, remainingAmount, item.salePrice(), item.salePrice() * remainingAmount, item));
            }
            Refresh();
        }
    }
}
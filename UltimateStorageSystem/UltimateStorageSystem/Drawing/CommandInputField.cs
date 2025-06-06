using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using UltimateStorageSystem.Interfaces;

namespace UltimateStorageSystem.Drawing
{
    public class CommandInputField : IKeyboardSubscriber
    {
        private string inputText = "";

        private bool showCursor = true;

        private double cursorBlinkTimer = 0.0;

        private IFilterableTable table;

        private readonly Texture2D whiteTexture;

        private readonly int x;

        private readonly int y;

        private readonly int width = 300;

        private readonly int height = 40;

        private readonly string searchLabel;

        public bool Selected { get; set; }

        public CommandInputField(int x, int y, IFilterableTable table, string searchLabel)
        {
            this.x = x;
            this.y = y;
            this.table = table;
            this.searchLabel = searchLabel;
            whiteTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            whiteTexture.SetData(new Color[1] { Color.White });
            Game1.keyboardDispatcher.Subscriber = this;
        }

        public void Draw(SpriteBatch b)
        {
            b.Draw(destinationRectangle: new Rectangle(x, y, width, height), texture: whiteTexture, color: Color.Black * 0.8f);
            Vector2 searchLabelPosition = new(x, y + 10);
            Vector2 searchLabelSize = Game1.smallFont.MeasureString(searchLabel);
            Vector2 textPosition = new(searchLabelPosition.X + searchLabelSize.X, searchLabelPosition.Y);
            string displayText = inputText;
            if (showCursor)
            {
                displayText += "_";
            }
            b.DrawString(Game1.smallFont, searchLabel, searchLabelPosition, Color.Orange);
            b.DrawString(Game1.smallFont, displayText, textPosition, Color.White);
        }

        public void UpdateTable(IFilterableTable newTable)
        {
            table = newTable;
            table.FilterItems(inputText);
        }

        public void Update(GameTime gameTime)
        {
            cursorBlinkTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (cursorBlinkTimer >= 500.0)
            {
                showCursor = !showCursor;
                cursorBlinkTimer = 0.0;
            }
        }

        public void RecieveTextInput(char inputChar)
        {
            if (!char.IsControl(inputChar))
            {
                inputText += inputChar;
                table.FilterItems(inputText);
            }
        }

        public void ReceiveKeyPress(Keys key)
        {
            if (key == Keys.Back && inputText.Length > 0)
            {
                inputText = inputText.Substring(0, inputText.Length - 1);
                table.FilterItems(inputText);
            }
            else if (key == Keys.Escape)
            {
                inputText = "";
                table.FilterItems("");
            }
        }

        public void RecieveTextInput(string text)
        {
        }

        public void RecieveCommandInput(char command)
        {
        }

        public void RecieveSpecialInput(Keys key)
        {
        }

        public void Reset()
        {
            inputText = "";
            table.FilterItems("");
        }
    }
}
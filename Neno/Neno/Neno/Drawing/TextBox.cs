using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Neno
{
    public enum TextOrient
    {
        Left = 0,
        Middle = 1,
        Right = 2
    }
    public class TextBox
    {
        public int X;
        public int Y;
        public string Text;
        float Scale;
        SpriteFont Font;
        TextOrient Orient = TextOrient.Left;
        public Color textColor = Color.White;
        public Color boxColor = new Color(0, 0, 0, 100);
        Color currentTextColor = Color.White;
        Color currentBoxColor = new Color(0, 0, 0, 100);
        public bool Select = false;
        Rectangle Rect;
        int Padding = 4;
        Texture2D Texture = Main.pix;
        public string typeText = "";
        bool typebox = false;
        Keys[] buffer;
        Keys[] bufferAlt = Key.keyboard.GetPressedKeys();
        public bool clicked = false;
        public bool Visible = true;
        int holdTimer = 0;
        Timer holdInterval = new Timer(5, true);
        public bool dontDraw = false;
        public string Description = "";
        public int maxWidth = 0;
        public int tag = 0; //Extra tag

        public TextBox(int x, int y, string text, float scale, SpriteFont font)
        {
            X = x;
            Y = y;
            Text = text;
            Scale = scale;
            Font = font;
            Init();
        }
        public TextBox(int x, int y, string text, float scale, SpriteFont font, TextOrient orient)
        {
            X = x;
            Y = y;
            Text = text;
            Scale = scale;
            Font = font;
            Orient = orient;
            Init();
        }
        public TextBox(int x, int y, string text, float scale, SpriteFont font, TextOrient orient, bool typeable)
        {
            X = x;
            Y = y;
            Text = text;
            Scale = scale;
            Font = font;
            Orient = orient;
            typebox = typeable;
            Init();
        }
        public TextBox(int x, int y, string text, float scale, SpriteFont font, TextOrient orient, bool typeable, string typetext)
        {
            X = x;
            Y = y;
            Text = text;
            Scale = scale;
            Font = font;
            Orient = orient;
            typebox = typeable;
            typeText = typetext;
            Init();
        }
        public TextBox(int x, int y, string text, float scale, SpriteFont font, TextOrient orient, Color textcolor, Color boxcolor)
        {
            X = x;
            Y = y;
            Text = text;
            Scale = scale;
            Font = font;
            Orient = orient;
            Init();
        }
        public TextBox(int x, int y, string text, float scale, SpriteFont font, TextOrient orient, Color textcolor, Texture2D texture)
        {
            X = x;
            Y = y;
            Text = text;
            Scale = scale;
            Font = font;
            Orient = orient;
            Texture = texture;
            Init();
        }

        private string WrapText(SpriteFont spriteFont, string text, float maxLineWidth)
        {
            string[] words = text.Split(' ');
            StringBuilder sb = new StringBuilder();
            float lineWidth = 0f;
            float spaceWidth = spriteFont.MeasureString(" ").X;

            foreach (string word in words)
            {
                Vector2 size = spriteFont.MeasureString(word);

                if (lineWidth + size.X < maxLineWidth)
                {
                    sb.Append(word + " ");
                    lineWidth += size.X + spaceWidth;
                }
                else
                {
                    sb.Append("\n" + word + " ");
                    lineWidth = size.X + spaceWidth;
                }
            }

            return sb.ToString();
        }

        private void Type()
        {
            buffer = Key.keyboard.GetPressedKeys();
            StringBuilder builder = new StringBuilder(typeText);
            bool Caps = false;
            if (buffer.Contains(Keys.LeftShift)
                || buffer.Contains(Keys.RightShift))
                Caps = true;

            if (Key.keyboard.GetPressedKeys().Length > 0)
                holdTimer++;
            else
                holdTimer = 0;

            if (Key.down(Keys.LeftControl))
            {
                if (buffer.Contains(Keys.V)
                    && !bufferAlt.Contains(Keys.V))
                {
                    builder.Insert(builder.Length, System.Windows.Forms.Clipboard.GetText());
                }
            }
            else
                foreach (Keys key in buffer)
                {
                    if (!bufferAlt.Contains(key)
                        || (holdTimer > 30 & holdInterval.tick))
                    {
                        switch (key)
                        {
                            case Keys.Back:
                                if (builder.Length > 0)
                                    builder.Remove(builder.Length - 1, 1);
                                break;
                            case Keys.Delete:
                                if (builder.Length > 0)
                                    builder.Remove(builder.Length - 1, 1);
                                break;
                            case Keys.Escape:
                                clicked = false;
                                break;
                            case Keys.F1:
                                break;
                            case Keys.F2:
                                break;
                            case Keys.F3:
                                break;
                            case Keys.F4:
                                break;
                            case Keys.F5:
                                break;
                            case Keys.F6:
                                break;
                            case Keys.F7:
                                break;
                            case Keys.F8:
                                break;
                            case Keys.F9:
                                break;
                            case Keys.F11:
                                break;
                            case Keys.F12:
                                break;
                            case Keys.Tab:
                                break;
                            case Keys.CapsLock:
                                break;
                            case Keys.LeftShift:
                                break;
                            case Keys.RightShift:
                                break;
                            case Keys.LeftWindows:
                                break;
                            case Keys.RightWindows:
                                break;
                            case Keys.LeftControl:
                                break;
                            case Keys.RightControl:
                                break;
                            case Keys.LeftAlt:
                                break;
                            case Keys.RightAlt:
                                break;
                            case Keys.Space:
                                builder.Insert(builder.Length, " ");
                                break;
                            case Keys.Insert:
                                break;
                            case Keys.End:
                                break;
                            case Keys.Home:
                                break;
                            case Keys.PageUp:
                                break;
                            case Keys.PageDown:
                                break;
                            case Keys.PrintScreen:
                                break;
                            case Keys.Enter:
                                break;
                            case Keys.Up:
                                break;
                            case Keys.Down:
                                break;
                            case Keys.Left:
                                break;
                            case Keys.Right:
                                break;
                            case Keys.Scroll:
                                break;
                            case Keys.Pause:
                                break;
                            case Keys.OemQuotes:
                                if (!Caps) builder.Insert(builder.Length, "'");
                                else builder.Insert(builder.Length, "\"");
                                break;
                            case Keys.OemComma:
                                if (!Caps) builder.Insert(builder.Length, ",");
                                else builder.Insert(builder.Length, "<");
                                break;
                            case Keys.OemPeriod:
                                if (!Caps) builder.Insert(builder.Length, ".");
                                else builder.Insert(builder.Length, ">");
                                break;
                            case Keys.OemQuestion:
                                if (!Caps) builder.Insert(builder.Length, "/");
                                else builder.Insert(builder.Length, "?");
                                break;
                            case Keys.Subtract:
                                if (!Caps) builder.Insert(builder.Length, "-");
                                else builder.Insert(builder.Length, "_");
                                break;
                            case Keys.Add:
                                if (!Caps) builder.Insert(builder.Length, "=");
                                else builder.Insert(builder.Length, "+");
                                break;
                            case Keys.OemMinus:
                                if (!Caps) builder.Insert(builder.Length, "-");
                                else builder.Insert(builder.Length, "_");
                                break;
                            case Keys.OemPlus:
                                if (!Caps) builder.Insert(builder.Length, "=");
                                else builder.Insert(builder.Length, "+");
                                break;
                            case Keys.D0:
                                if (!Caps) builder.Insert(builder.Length, "0");
                                else builder.Insert(builder.Length, ")");
                                break;
                            case Keys.D1:
                                if (!Caps) builder.Insert(builder.Length, "1");
                                else builder.Insert(builder.Length, "!");
                                break;
                            case Keys.D2:
                                if (!Caps) builder.Insert(builder.Length, "2");
                                else builder.Insert(builder.Length, "@");
                                break;
                            case Keys.D3:
                                if (!Caps) builder.Insert(builder.Length, "3");
                                else builder.Insert(builder.Length, "#");
                                break;
                            case Keys.D4:
                                if (!Caps) builder.Insert(builder.Length, "4");
                                else builder.Insert(builder.Length, "$");
                                break;
                            case Keys.D5:
                                if (!Caps) builder.Insert(builder.Length, "5");
                                else builder.Insert(builder.Length, "%");
                                break;
                            case Keys.D6:
                                if (!Caps) builder.Insert(builder.Length, "6");
                                else builder.Insert(builder.Length, "^");
                                break;
                            case Keys.D7:
                                if (!Caps) builder.Insert(builder.Length, "7");
                                else builder.Insert(builder.Length, "&");
                                break;
                            case Keys.D8:
                                if (!Caps) builder.Insert(builder.Length, "8");
                                else builder.Insert(builder.Length, "*");
                                break;
                            case Keys.D9:
                                if (!Caps) builder.Insert(builder.Length, "9");
                                else builder.Insert(builder.Length, "(");
                                break;
                            case Keys.NumPad0:
                                if (!Caps) builder.Insert(builder.Length, "0");
                                else builder.Insert(builder.Length, ")");
                                break;
                            case Keys.NumPad1:
                                if (!Caps) builder.Insert(builder.Length, "1");
                                else builder.Insert(builder.Length, "!");
                                break;
                            case Keys.NumPad2:
                                if (!Caps) builder.Insert(builder.Length, "2");
                                else builder.Insert(builder.Length, "@");
                                break;
                            case Keys.NumPad3:
                                if (!Caps) builder.Insert(builder.Length, "3");
                                else builder.Insert(builder.Length, "#");
                                break;
                            case Keys.NumPad4:
                                if (!Caps) builder.Insert(builder.Length, "4");
                                else builder.Insert(builder.Length, "$");
                                break;
                            case Keys.NumPad5:
                                if (!Caps) builder.Insert(builder.Length, "5");
                                else builder.Insert(builder.Length, "%");
                                break;
                            case Keys.NumPad6:
                                if (!Caps) builder.Insert(builder.Length, "6");
                                else builder.Insert(builder.Length, "^");
                                break;
                            case Keys.NumPad7:
                                if (!Caps) builder.Insert(builder.Length, "7");
                                else builder.Insert(builder.Length, "&");
                                break;
                            case Keys.NumPad8:
                                if (!Caps) builder.Insert(builder.Length, "8");
                                else builder.Insert(builder.Length, "*");
                                break;
                            case Keys.NumPad9:
                                if (!Caps) builder.Insert(builder.Length, "9");
                                else builder.Insert(builder.Length, "(");
                                break;
                            default:
                                if (!key.ToString().Contains("Oem"))
                                {
                                    if (Caps) builder.Insert(builder.Length, key.ToString().ToUpper());
                                    else builder.Insert(builder.Length, key.ToString().ToLower());
                                }
                                break;
                        }
                        Sound.Play("type");
                    }
                }

            typeText = builder.ToString();

            bufferAlt = Key.keyboard.GetPressedKeys();
        }

        private void Init()
        {
            currentTextColor = textColor;
            currentBoxColor = boxColor;
        }

        public bool CheckSelect()
        {
            Select = false;
            if (Rect.Contains((int)Main.mousePos.X, (int)Main.mousePos.Y) && !dontDraw)
                Select = true;

            return (Select);
        }

        public bool CheckClicked()
        {
            if (Main.mouseLeftPressed && !dontDraw)
            {
                Select = false;
                if (Rect.Contains((int)Main.mousePos.X, (int)Main.mousePos.Y))
                    Select = true;



                return (Select);
            }
            else
                return false;
        }

        public void Draw(string extraText, SpriteBatch spriteBatch, float depth = 0)
        {

            if (typebox)
            {
                if (Main.mouseLeftPressed
                    && Select)
                    clicked = true;
                if (Main.mouseLeftPressed
                    && !Select)
                    clicked = false;
            }


            if (typebox && clicked)
            {
                Type();
            }


            string text = Text;
            if (extraText != null)
                text += extraText + typeText;

            //Word wrap
            if (maxWidth > 0)
            {
                text = WrapText(Font, text, maxWidth);
            }

            int width = (int)(Font.MeasureString(text).X * Scale);
            int height = (int)(Font.MeasureString(text).Y * Scale);

            //Variables
            int x = 0; 
            int y = 0;
            if (Orient == TextOrient.Left) {x = X; y = Y;}
            if (Orient == TextOrient.Right) { x = X - width; y = Y; }
            if (Orient == TextOrient.Middle) { x = X - width / 2; y = Y; }
            Rect = new Rectangle(x - Padding, y - Padding, width + Padding * 2, height);
            Init();
            if (clicked && typebox) { currentBoxColor = new Color(boxColor.R + 100, boxColor.G + 100, boxColor.B + 100, boxColor.A + 100); }
            if (Select && !clicked) { currentBoxColor = new Color(boxColor.R + 50, boxColor.G + 50, boxColor.B + 50, boxColor.A + 50); }

            //Color
            var nextBoxColor = currentBoxColor;
            var nextTextColor = currentTextColor;

            //Box
            float nextDepth = depth;
            if (depth > 0)
                nextDepth = depth - 0.00001f;
            if (!dontDraw)
            {
                spriteBatch.Draw(Texture, Rect, Texture.Bounds, nextBoxColor, 0, Vector2.Zero, SpriteEffects.None, nextDepth);

                //Text
                spriteBatch.DrawString(Font, text, new Vector2(x, y), nextTextColor, 0, new Vector2(0, 0), Scale, SpriteEffects.None, depth);
            }
        }
    }
}

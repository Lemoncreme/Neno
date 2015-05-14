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
    public class SelectMenu
    {
        public List<string> Items;
        public SpriteFont font;
        int select = -1;
        
        public SelectMenu(List<string> items)
        {
            font = Main.consoleFont;
            Items = items;
        }

        public void Update(Vector2 pos)
        {
            int longest = 0;

            for (int i = 0; i < Items.Count; i++)
            {
                if (font.MeasureString(Items[i]).X > longest)
                    longest = (int)font.MeasureString(Items[i]).X;
            }

            select = -1;
            for (int i = 0; i < Items.Count; i++)
            {
                if (
                    Main.mousePos.X < pos.X + longest &&
                    Main.mousePos.X > pos.X &&
                    Main.mousePos.Y < pos.Y + i * font.MeasureString(Items[i]).Y + font.MeasureString(Items[i]).Y &&
                    Main.mousePos.Y > pos.Y + i * font.MeasureString(Items[i]).Y)
                    select = i;
            }
        }

        public string CheckClicked()
        {
            if (Main.mouseLeftPressed && select != -1)
            {
                return Items[select];
            }
            return "";
        }

        public void Draw(Vector2 pos)
        {
            TextOrient orient = TextOrient.Left;
            int down = 1;
            int horiz = 1;
            int longest = 0;
            if (pos.X > Main.windowWidth / 2)
            { orient = TextOrient.Right; horiz = -1; }
            if (pos.Y > Main.windowHeight / 2)
                down = -1;

            for(int i = 0; i < Items.Count; i++)
            {
                if (font.MeasureString(Items[i]).X > longest)
                    longest = (int)font.MeasureString(Items[i]).X;
            }

            int X = (int)pos.X - 2;
            int Y = (int)pos.Y - 2;
            int W = longest + 4;
            int H = (int)(Items.Count * font.MeasureString(Items[0]).Y) + 4;
            if (horiz == -1)
                X = X - W;
            if (down == -1)
                Y = Y - H;

            Main.sb.Draw(Main.pix, new Rectangle(X, Y, W, H), new Color(0, 0, 0, 0.6f));

            for(int i = 0; i < Items.Count; i++)
            {
                Color color = Color.LightGray;
                if (select == i)
                    color = Color.Yellow;
                Main.drawText(font, Items[i], new Vector2(pos.X, pos.Y + (i * font.MeasureString(Items[i]).Y) * down), color, 1f, orient);
            }
        }
    }
}

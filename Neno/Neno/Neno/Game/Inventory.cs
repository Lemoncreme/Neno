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
    public class Inventory
    {
        Item[,] inv;
        public int width = 12;
        public int height = 8;

        public int drawSize = 32;

        public Inventory(int w, int h)
        {
            inv = new Item[w, h];
            width = w;
            height = h;
        }

        public void Add(Item item)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (inv[x, y] == null)
                    {
                        inv[x, y] = item;
                        return;
                    }
                }
            }
        }
        public Item Get(Point pos)
        {
            if (pos.X >= 0 && pos.X < width && pos.Y >= 0 && pos.Y < height)
                return inv[pos.X, pos.Y];
            else
                return null;
        }

        public void Draw(Point pos)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Main.sb.Draw(Main.img("slot"), new Rectangle(pos.X + x * drawSize, pos.Y + y * drawSize, drawSize, drawSize), Color.White);
                    if (inv[x, y] != null)
                    {
                        var item = inv[x, y];
                        Color color = Color.White;
                        if (item.Prop(PropType.DmgMagic) > 0)
                            color = new Color(0.5f, (float)Math.Sin(Main.Time / 6) * (item.Prop(PropType.DmgMagic) / 10f), 0.8f + (float)Math.Sin(Main.Time / 6) * 0.2f, 1f);
                        if (item.isEquipped)
                            color.A = 120;
                        Main.sb.Draw(Main.img("items"), new Rectangle(pos.X + x * drawSize, pos.Y + y * drawSize, drawSize, drawSize),
                            Main.fromSheet(inv[x, y].Frame, 2048, 2048, 256, 256), color);
                    }
                }
            }
        }
    }
}

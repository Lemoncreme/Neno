using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Neno
{
    public class Char
    {
        

        public static void draw(Vector2 pos, Color Hair, Color Skin, Color Pants, Color Shirt, Color Eyes)
        {
            var rect = new Rectangle((int)pos.X, (int)pos.Y, 8, 8);

            Main.sb.Draw(Main.img("Char/body"), rect, Skin);
            Main.sb.Draw(Main.img("Char/hair"), rect, Hair);
            Main.sb.Draw(Main.img("Char/eyes"), rect, Eyes);
            Main.sb.Draw(Main.img("Char/limbs"), rect, Pants);
            Main.sb.Draw(Main.img("Char/shirt"), rect, Shirt);
        }
    }
}

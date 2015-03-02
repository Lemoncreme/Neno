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
    public static class Key
    {
        public static KeyboardState keyboard;
        private static KeyboardState lastkeyboard;




        public static void update() 
        {
            lastkeyboard = keyboard;
            keyboard = Keyboard.GetState();
        }

        public static bool down(Keys key)
        {
            return (keyboard.IsKeyDown(key));
        }
        public static bool pressed(Keys key)
        {
            return (keyboard.IsKeyDown(key) && lastkeyboard.IsKeyUp(key));
        }
        public static bool unpressed(Keys key)
        {
            return (keyboard.IsKeyDown(key));
        }
    }
}

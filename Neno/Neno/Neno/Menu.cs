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
    public class Menu
    {

        #region Variables

        TextBox currentItem;
        TextBox[] currentMenu;
        TextBox[] menuMain;

        #endregion




        public void init()
        {
            //Create Menus
            menuMain = new TextBox[] { 
            new TextBox(4, 60, "Join", 0.5f, Main.font),
            new TextBox(4, 100, "Host", 0.5f, Main.font),
            new TextBox(4, 140, "Exit", 0.5f, Main.font)
            };
            currentMenu = menuMain;
        }

        public void step()
        {
            currentItem = null;
            foreach(TextBox item in currentMenu)
            {
                if (item.CheckSelect())
                {
                    currentItem = item;
                }
                if (item.CheckClicked())
                {
                    switch(item.Text)
                    {
                        case "Join":
                            Main.Switch(Focus.Client);
                            break;
                        case "Host":
                            Main.Switch(Focus.Server);
                            break;
                        case "Exit":
                            Main.self.Exit();
                            break;
                    }
                }
            }
        }

        public void draw()
        {
            Main.sb.Begin();

            Main.sb.DrawString(Main.font, "Neno Test", Vector2.Zero, Color.White);
            foreach (TextBox item in currentMenu)
            {
                item.Draw("", Main.sb);
            }

            Main.sb.End();
        }

        public void end()
        {

        }
    }
}

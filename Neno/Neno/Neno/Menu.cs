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
        TextBox[] menuJoin;
        TextBox[] menuServer;

        #endregion




        public void init()
        {
            //Create Menus
            menuMain = new TextBox[] { 
            new TextBox(4, 50, "Join", 0.5f, Main.font),
            new TextBox(4, 100, "Host", 0.5f, Main.font),
            new TextBox(4, 150, "Exit", 0.5f, Main.font)
            };
            menuJoin = new TextBox[] { 
            new TextBox(4, 50, "IP", 0.5f, Main.font, TextOrient.Left, true, "127.0.0.1"),
            new TextBox(4, 100, "Port", 0.5f, Main.font, TextOrient.Left, true, Settings.defaultPort.ToString()),
            new TextBox(4, 150, "Connect", 0.5f, Main.font),
            new TextBox(4, 200, "Back", 0.5f, Main.font)
            };
            menuServer = new TextBox[] { 
            new TextBox(4, 50, "Name", 0.5f, Main.font, TextOrient.Left, true, "Neno Game Server"),
            new TextBox(4, 100, "Port", 0.5f, Main.font, TextOrient.Left, true, Settings.defaultPort.ToString()),
            new TextBox(4, 150, "Start Server", 0.5f, Main.font),
            new TextBox(4, 200, "Back", 0.5f, Main.font)
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
                            currentMenu = menuJoin;
                            break;
                        case "Host":
                            currentMenu = menuServer;
                            break;
                        case "Exit":
                            Main.self.Exit();
                            break;
                        case "Connect":
                            GameClient.connectIP = menuJoin[0].typeText;
                            GameClient.connectPort = Convert.ToInt32(menuJoin[1].typeText);
                            Main.Switch(Focus.Client);
                            break;
                        case "Start Server":
                            GameServer.serverName = menuServer[0].typeText;
                            GameServer.serverPort = Convert.ToInt32(menuJoin[1].typeText);
                            Main.Switch(Focus.Server);
                            break;
                        case "Back":
                            currentMenu = menuMain;
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

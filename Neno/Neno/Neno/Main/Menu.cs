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
            new TextBox(4, 150, "Join", 0.5f, Main.font),
            new TextBox(4, 200, "Host", 0.5f, Main.font),
            new TextBox(4, 250, "Exit", 0.5f, Main.font)
            };
            menuJoin = new TextBox[] { 
            new TextBox(4, 150, "IP: ", 0.5f, Main.font, TextOrient.Left, true, "127.0.0.1"),
            new TextBox(4, 200, "Port: ", 0.5f, Main.font, TextOrient.Left, true, Settings.defaultPort.ToString()),
            new TextBox(4, 250, "Connect", 0.5f, Main.font),
            new TextBox(4, 300, "Back", 0.5f, Main.font)
            };
            menuServer = new TextBox[] { 
            new TextBox(4, 150, "Name: ", 0.5f, Main.font, TextOrient.Left, true, "Neno Game Server"),
            new TextBox(4, 200, "Port: ", 0.5f, Main.font, TextOrient.Left, true, Settings.defaultPort.ToString()),
            new TextBox(4, 250, "Start Server", 0.5f, Main.font),
            new TextBox(4, 300, "Back", 0.5f, Main.font)
            };
            currentMenu = menuMain;
        }

        public void step()
        {
            if (Main.mouseLeftPressed && (new Rectangle(4, Main.windowHeight - 100, 92, 92).Contains((int)Main.mousePos.X, (int)Main.mousePos.Y)))
            { 
                MediaPlayer.IsMuted = !MediaPlayer.IsMuted;
                Sound.Play("type");
            }

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
                            Main.Client.isOwner = true;
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
            Main.sb.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, DepthStencilState.Default, null);

            Main.sb.Draw(Main.img("Boards/Battle"), new Rectangle(
                (int)(Math.Sin(Main.Time / 100f) * 256f - 256f), 
                (int)(Math.Sin(Main.Time / 250f) * 1000f - 1000f),
                Main.img("Boards/Battle").Bounds.Width * 6,
                Main.img("Boards/Battle").Bounds.Height * 6), Color.White);
            Main.sb.Draw(Main.img("Boards/Battle"), new Rectangle(
                (int)(Math.Sin(Main.Time / 100f) * 256f + 256f),
                (int)(Math.Sin(Main.Time / 250f) * 1000f - 1000f),
                Main.img("Boards/Battle").Bounds.Width * 6,
                Main.img("Boards/Battle").Bounds.Height * 6), Color.White);
            if (MediaPlayer.IsMuted)
                Main.sb.Draw(Main.img("musicOff"), new Vector2(4, Main.windowHeight - 100), Color.Black);
            else
                Main.sb.Draw(Main.img("musicOn"), new Vector2(4, Main.windowHeight - 100), Color.Black);

            Main.sb.Draw(Main.img("logo"), new Vector2(Main.windowWidth / 2, 0), Main.img("logo").Bounds, Color.Black, 0, new Vector2(Main.img("logo").Width / 2, 0), 0.5f, SpriteEffects.None, 0);
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

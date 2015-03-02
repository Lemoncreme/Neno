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
using Lidgren.Network;

namespace Neno
{
    public class GameServer
    {


        #region Variables

        NetServer server;
        NetPeerConfiguration config;

        #endregion


        #region Methods

        void startServer()
        {
            //Config
            config = new NetPeerConfiguration("Neno");
            config.Port = 25565;
            config.MaximumConnections = 8;

            //Start Server
            server = new NetServer(config);
            server.Start(); Console.WriteLine("Server started");
        }

        #endregion




        public void init()
        {
            startServer();
        }

        public void step()
        {
            if (Key.pressed(Keys.Home)) Main.Switch(Focus.Menu);
        }

        public void draw()
        {
            Main.sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            Main.sb.DrawString(Main.font, "Neno Server", Vector2.Zero, Color.White);

            Main.sb.End();
        }

        public void end()
        {
            server.Shutdown("end");
        }
    }
}

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
    public class GameClient
    {


        #region Variables

        NetClient client;
        NetPeerConfiguration config;

        #endregion


        #region Methods

        void startClient()
        {
            //Config
            config = new NetPeerConfiguration("Neno");

            //Start Server
            client = new NetClient(config);
            client.Start(); Console.WriteLine("Client started");
        }

        #endregion




        public void init()
        {
            startClient();
        }

        public void step()
        {
            if (Key.pressed(Keys.Escape)) Main.Switch(Focus.Menu);
        }

        public void draw()
        {
            Main.sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            Main.sb.Draw(Main.img("rock"), new Rectangle(64, 64, 64, 64), Color.White);

            Main.sb.End();
        }

        public void end()
        {
            client.Shutdown("end");
        }
    }
}

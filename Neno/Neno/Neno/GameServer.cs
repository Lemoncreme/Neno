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
    enum ServerMsg
    {
        init
    }
    public class GameServer
    {


        #region Variables

        NetServer server;
        NetPeerConfiguration config;
        public static string serverName;
        public static int serverPort;

        #endregion


        #region Methods

        void startServer()
        {
            //Config
            config = new NetPeerConfiguration("Neno");
            config.Port = serverPort;
            config.MaximumConnections = 6;

            //Start Server
            server = new NetServer(config);
            server.Start(); Console.WriteLine("Server started on port " + server.Port + ", endpoint: " + server.Socket.LocalEndPoint);
        }
        void recMessage()
        {
            NetIncomingMessage inc;

            while ((inc = server.ReadMessage()) != null)
            {
                switch (inc.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        switch ((ServerMsg)inc.ReadByte())
                        {
                            case ServerMsg.init: //Recieve init data request

                                sendInitData(inc.SenderConnection);
                                break;
                        }
                        break;
                }
            }
        }
        void sendInitData(NetConnection recipient)
        {
            NetOutgoingMessage sendMsg = server.CreateMessage();

            sendMsg.Write((byte)ClientMsg.init);
            sendMsg.Write(serverName);

            server.SendMessage(sendMsg, recipient, NetDeliveryMethod.ReliableOrdered);
            Console.WriteLine("Sent init data");
        }

        #endregion




        public void init()
        {
            startServer();
        }

        public void step()
        {
            if (Key.pressed(Keys.Home)) Main.Switch(Focus.Menu);

            recMessage();
        }

        public void draw()
        {
            Main.sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            Main.sb.DrawString(Main.font, "Neno Server", Vector2.Zero, Color.White);
            Main.sb.DrawString(Main.font, "Connections: " + server.ConnectionsCount, new Vector2(4, 64), Color.White, 0, Vector2.Zero, 0.25f, SpriteEffects.None, 0);

            Main.sb.End();
        }

        public void end()
        {
            server.Shutdown("end");
            Console.WriteLine("Server ended");
        }
    }
}

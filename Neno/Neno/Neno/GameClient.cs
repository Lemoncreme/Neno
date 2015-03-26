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
    enum ClientStatus
    {
        Waiting_For_Connection,
        Waiting_For_Data
    }
    enum ClientMsg
    {
        init
    }
    public class GameClient
    {


        #region Variables

        NetClient client;
        NetPeerConfiguration config;
        public static string connectIP;
        public static int connectPort;
        ClientStatus Status = ClientStatus.Waiting_For_Connection;
        string serverName = "";
        string clientName = "";

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
        void recMessage()
        {
            NetIncomingMessage inc;

            while ((inc = client.ReadMessage()) != null)
            {

                switch (inc.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        switch ((ClientMsg)inc.ReadByte())
                        {
                            case ClientMsg.init: //Receive init data from server
                                serverName = inc.ReadString();
                                Console.WriteLine("Server name is " + serverName);
                                break;
                        }
                        break;
                }
            }
        }
        void sendInitRequest()
        {
            NetOutgoingMessage sendMsg = client.CreateMessage();

            sendMsg.Write((byte)ServerMsg.init);
            sendMsg.Write(clientName);

            client.SendMessage(sendMsg, client.ServerConnection, NetDeliveryMethod.ReliableOrdered);
            Console.WriteLine("Sent ready");
        }

        #endregion




        public void init()
        {
            //Get name
            clientName = Environment.UserName;

            startClient();

            //Connect to local
            if (Main.focus == Focus.Server)
                client.Connect("127.0.0.1", Settings.defaultPort);

            //Connect to remote
            if (Main.focus == Focus.Client)
                client.Connect(connectIP, connectPort);
        }

        public void step()
        {
            if (Key.pressed(Keys.Escape)) Main.Switch(Focus.Menu);

            switch (Status)
            {
                case ClientStatus.Waiting_For_Connection:
                    if (client.ServerConnection != null)
                    {
                        Console.WriteLine("Client connected");
                        sendInitRequest();
                        Status = ClientStatus.Waiting_For_Data;
                    }
                    break;
                case ClientStatus.Waiting_For_Data:
                    recMessage();
                    break;
            }
        }

        public void draw()
        {
            Main.sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            Main.sb.Draw(Main.img("rock"), new Rectangle(64, 64, 64, 64), Color.White);
            Main.sb.DrawString(Main.font, "Status: " + client.ConnectionStatus, new Vector2(8, 256), Color.White);

            Main.sb.End();
        }

        public void end()
        {
            client.Shutdown("end");
            Console.WriteLine("Client ended");
        }
    }
}

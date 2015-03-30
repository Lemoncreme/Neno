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
        init, join, ready
    }
    public class GameServer
    {


        #region Variables

        NetServer server;
        NetPeerConfiguration config;
        public static string serverName;
        public static int serverPort;
        List<ServerPlayer> playerList = new List<ServerPlayer>();
        byte turn = 0;

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
            server.Start();
            Console.WriteLine("Server running on port " + server.Port);
        }
        void recMessage()
        {
            NetIncomingMessage inc;
            byte playerID = 0;

            while ((inc = server.ReadMessage()) != null)
            {
                switch (inc.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        switch ((ServerMsg)inc.ReadByte())
                        {
                            case ServerMsg.init: //Recieve init data request

                                //Get name and ID
                                string name = inc.ReadString();
                                playerID = (byte)playerList.Count;
                                Console.WriteLine("Player " + name + " (" + playerID + ") joined");

                                //Search and fix name dupes
                                if (playerList.Count > 0)
                                {
                                    int add = -1;
                                    foreach (ServerPlayer player in playerList)
                                    {
                                        if (player.Name == name)
                                            add = 0;
                                        else
                                            if (player.Name == name + "_" + add)
                                                add += 1;
                                    }
                                    if (add != -1)
                                    {
                                        name += "_" + add;
                                        Console.WriteLine("Name dupe, player " + playerID + " is now called " + name);
                                    }
                                }

                                //Add to playerlist
                                playerList.Add(new ServerPlayer(name, playerID));

                                //Send data
                                sendInitData(inc.SenderConnection, playerID);

                                //Send other players
                                foreach (ServerPlayer player in playerList)
                                {
                                    sendPlayerInfo(inc.SenderConnection, player.ID);
                                }
                                break;
                            case ServerMsg.join:
                                playerID = inc.ReadByte();

                                getPlayer(playerID).Status = PlayerStatus.Lobby;
                                Console.WriteLine(getName(playerID) + " joined lobby");
                                break;
                            case ServerMsg.ready:
                                playerID = inc.ReadByte();

                                getPlayer(playerID).ready = true;
                                Console.WriteLine(getName(playerID) + " is ready");
                                sendPlayerReady(playerID);
                                break;
                        }
                        break;
                }
            }
        }

        #region Sending Messages
        void sendInitData(NetConnection recipient, byte playerID)
        {
            NetOutgoingMessage sendMsg = server.CreateMessage();

            sendMsg.Write((byte)ClientMsg.init);

            //Server Name
            sendMsg.Write(serverName);

            //Player Info
            sendMsg.Write(playerID);
            sendMsg.Write(getName(playerID));

            server.SendMessage(sendMsg, recipient, NetDeliveryMethod.ReliableOrdered);
            Console.WriteLine("Sent init data to " + getName(playerID));
        }
        void sendPlayerInfo(NetConnection recipient, byte playerID)
        {
            NetOutgoingMessage sendMsg = server.CreateMessage();

            //Info about a player
            sendMsg.Write((byte)ClientMsg.playerInfo);

            sendMsg.Write(playerID);
            sendMsg.Write(getName(playerID));
            sendMsg.Write(getPlayer(playerID).ready);

            server.SendMessage(sendMsg, recipient, NetDeliveryMethod.ReliableOrdered);
        }
        void sendPlayerReady(byte playerID)
        {
            NetOutgoingMessage sendMsg = server.CreateMessage();

            //Info about a player
            sendMsg.Write((byte)ClientMsg.playerIsReady);

            sendMsg.Write(playerID);

            server.SendToAll(sendMsg, NetDeliveryMethod.ReliableOrdered);
        }
        #endregion
        string getName(byte playerID)
        {
            return getPlayer(playerID).Name;
        }
        ServerPlayer getPlayer(byte playerID)
        {
            foreach(ServerPlayer player in playerList)
            {
                if (player.ID == playerID)
                    return player;
            }
            return null;
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
            if (Key.down(Keys.F1))
            {
                int i = 4;

                Main.sb.DrawString(Main.consoleFont, "Neno Server", new Vector2(4, i), Color.White); i += 22;

                Main.sb.DrawString(Main.consoleFont, "Connections: " + server.ConnectionsCount, new Vector2(4, i), Color.White); i += 16;
                Main.sb.DrawString(Main.consoleFont, "# Recieved: " + server.Statistics.ReceivedPackets, new Vector2(4, i), Color.White); i += 16;
                Main.sb.DrawString(Main.consoleFont, "# Sent: " + server.Statistics.SentPackets, new Vector2(4, i), Color.White); i += 16;
                Main.sb.DrawString(Main.consoleFont, "Players: " + playerList.Count, new Vector2(4, i), Color.White); i += 16;

                Main.sb.DrawString(Main.consoleFont, "Turn: " + getName(turn), new Vector2(4, i), Color.White); i += 16;
            }
            else
                Main.sb.DrawString(Main.consoleFont, "Hold F1 to view server info", new Vector2(4, 4), new Color(1f, 1f, 1f, 0.1f));
            Main.sb.End();
            
        }

        public void end()
        {
            server.Shutdown("end");
            Console.WriteLine("Server ended");
        }
    }
}

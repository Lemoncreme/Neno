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
        init = 100, 
        join = 101, 
        ready = 102,
        start = 103
    }
    public class GameServer
    {


        #region Variables

        NetServer server;
        NetPeerConfiguration config;
        public static string serverName;
        public static int serverPort;
        List<ServerPlayer> playerList = new List<ServerPlayer>();
        byte turn = 1;
        byte lastID = 0;
        int battleBoardCount;
        List<BattleBoard> battleBoards = new List<BattleBoard>();
        WordBoard wordBoard;

        #endregion


        #region Methods

        void startServer()
        {
            //Config
            config = new NetPeerConfiguration("Neno");
            config.Port = serverPort;
            config.MaximumConnections = 6;
            config.EnableUPnP = true;

            //Start Server
            server = new NetServer(config);
            server.Start();
            Console.WriteLine("<SERVER> " + "Server running on port " + server.Port + " on external IP " + server.UPnP.GetExternalIP());

            //Try and forward port
            var worked = server.UPnP.ForwardPort(serverPort, "Neno Game Server Port");
            if (worked)
                Console.WriteLine("<SERVER> " + "Succesfully port forwarded port " + serverPort);
            else
                Console.WriteLine("<SERVER> " + "Did not succesfully port forward port " + serverPort);
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
                                playerID = (byte)(lastID + 1);
                                lastID++;
                                Console.WriteLine("<SERVER> " + "Player " + name + " (" + playerID + ") joined");

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
                                        Console.WriteLine("<SERVER> " + "Name dupe, player " + playerID + " is now called " + name);
                                    }
                                }

                                //Add to playerlist
                                playerList.Add(new ServerPlayer(name, playerID, inc.SenderConnection));

                                //Send data
                                sendInitData(inc.SenderConnection, playerID);

                                //Send other players
                                foreach (ServerPlayer player in playerList)
                                {
                                    if (player.ID != playerID) sendPlayerInfo(inc.SenderConnection, player.ID);
                                }
                                break;
                            case ServerMsg.join:
                                playerID = inc.ReadByte();

                                getPlayer(playerID).Status = PlayerStatus.Lobby;
                                Console.WriteLine("<SERVER> " + getName(playerID) + " joined lobby");
                                sendPlayerInfoAll(playerID);

                                break;
                            case ServerMsg.ready:
                                playerID = inc.ReadByte();

                                getPlayer(playerID).ready = true;
                                Console.WriteLine("<SERVER> " + getName(playerID) + " is ready");
                                sendPlayerReady(playerID);
                                break;
                            case ServerMsg.start:
                                playerID = inc.ReadByte();

                                if (playerID == 1)
                                {
                                    Console.WriteLine("<SERVER> starting game");
                                    turn = Main.choose<ServerPlayer>(playerList).ID;
                                    sendStarting();
                                }
                                break;
                        }
                        break;
                        case NetIncomingMessageType.StatusChanged:
                            switch ((NetConnectionStatus)inc.ReadByte())
                            {
                                case NetConnectionStatus.Disconnected:
                                    ServerPlayer player = getPlayer(inc.SenderConnection);
                                    if (inc.ReadString() == "end")
                                        Console.WriteLine(player.Name + " exited to menu");
                                    else
                                        Console.WriteLine(player.Name + " disconnected");
                                    sendPlayerRemoved(player.ID);
                                    playerList.Remove(player);
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
            Console.WriteLine("<SERVER> " + "Sent init data to " + getName(playerID));
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

            //Player is ready
            sendMsg.Write((byte)ClientMsg.playerIsReady);
            sendMsg.Write(playerID);

            server.SendToAll(sendMsg, NetDeliveryMethod.ReliableOrdered);
        }
        void sendPlayerInfoAll(byte playerID)
        {
            NetOutgoingMessage sendMsg = server.CreateMessage();

            //Info about a player
            sendMsg.Write((byte)ClientMsg.playerInfo);

            sendMsg.Write(playerID);
            sendMsg.Write(getName(playerID));
            sendMsg.Write(getPlayer(playerID).ready);

            server.SendToAll(sendMsg, NetDeliveryMethod.ReliableOrdered);
        }
        void sendPlayerRemoved(byte playerID)
        {
            NetOutgoingMessage sendMsg = server.CreateMessage();

            //Info about a player
            sendMsg.Write((byte)ClientMsg.playerLeft);
            sendMsg.Write(playerID);

            server.SendToAll(sendMsg, NetDeliveryMethod.ReliableOrdered);
        }
        void sendStarting()
        {
            NetOutgoingMessage sendMsg = server.CreateMessage();

            //Starting game
            sendMsg.Write((byte)ClientMsg.starting);
            sendMsg.Write(turn);

            server.SendToAll(sendMsg, NetDeliveryMethod.ReliableOrdered);
        }
        #endregion

        string getName(byte playerID)
        {
            return getPlayer(playerID).Name;
        }
        public ServerPlayer getPlayer(byte playerID)
        {
            foreach(ServerPlayer player in playerList)
            {
                if (player.ID == playerID)
                    return player;
            }
            return null;
        }
        ServerPlayer getPlayer(NetConnection connection)
        {
            foreach (ServerPlayer player in playerList)
            {
                if (player.Connection == connection)
                    return player;
            }
            return null;
        }

        void Create()
        {
            //Create wordboard
            wordBoard = new WordBoard();

            //Create battleboards
            battleBoardCount = playerList.Count;
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
            
            Main.sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, null, null);
            if (server.ConnectionsCount > 0)
            {
                if (Key.down(Keys.F1))
                {
                    int i = 4;

                    Main.sb.DrawString(Main.consoleFont, "Neno Server", new Vector2(4, i), Color.White); i += 22;

                    Main.sb.DrawString(Main.consoleFont, "Connections: " + server.ConnectionsCount, new Vector2(4, i), Color.White); i += 16;
                    Main.sb.DrawString(Main.consoleFont, "Players: " + playerList.Count, new Vector2(4, i), Color.White); i += 16;
                    Main.sb.DrawString(Main.consoleFont, "UPnP Status: " + server.UPnP.Status, new Vector2(4, i), Color.White); i += 16;

                    Main.sb.DrawString(Main.consoleFont, "Turn: " + getName(turn), new Vector2(4, i), Color.White); i += 16;
                }
            }
            Main.sb.End();
            
        }

        public void end()
        {
            server.Shutdown("end");
            Console.WriteLine("<SERVER> " + "Server ended");
        }
    }
}

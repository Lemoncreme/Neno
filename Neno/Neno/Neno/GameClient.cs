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
        Waiting_For_Data,
        Disconnected,
        Lobby,
        Starting_Game
    }
    enum ClientMsg
    {
        init, playerInfo, playerIsReady, playerLeft, starting
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
        byte playerID;
        List<ClientPlayer> playerList = new List<ClientPlayer>();
        TextBox readyBox;
        TextBox startBox;
        bool ready = false;
        public bool isOwner = false;
        byte turn;
        WordBoard wordBoard;

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
            byte ID;

            while ((inc = client.ReadMessage()) != null)
            {

                switch (inc.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        switch ((ClientMsg)inc.ReadByte())
                        {
                            case ClientMsg.init: //Receive init data from server
                                serverName = inc.ReadString();
                                playerID = inc.ReadByte();
                                clientName = inc.ReadString();
                                Console.WriteLine("Server name is " + serverName + ", I am client #" + playerID);
                                Status = ClientStatus.Lobby;
                                readyBox = new TextBox(Main.windowWidth / 2, 16, "I'm Ready!", 0.5f, Main.font, TextOrient.Middle);
                                if (isOwner)
                                { 
                                    startBox = new TextBox(Main.windowWidth / 2, 80, "Start Game", 1f, Main.font, TextOrient.Middle);
                                    startBox.Description = "Start game, if all players are ready";
                                }
                                sendJoin();
                                break;
                            case ClientMsg.playerInfo: //Information about another player
                                ID = inc.ReadByte();
                                string name = inc.ReadString();
                                bool ready = inc.ReadBoolean();
                                playerList.Add(new ClientPlayer(name, ID, ready));
                                Console.WriteLine(name + " (" + ID + ") info");
                                break;
                            case ClientMsg.playerIsReady: //Another player is ready to play
                                ID = inc.ReadByte();
                                getPlayer(ID).Ready = true;
                                Console.WriteLine(getPlayer(ID).Name + " (" + ID + ") is ready");
                                break;
                            case ClientMsg.playerLeft: //A player left the game
                                ID = inc.ReadByte();
                                playerList.Remove(getPlayer(ID));
                                break;
                            case ClientMsg.starting: //The game has been started
                                Status = ClientStatus.Starting_Game;
                                turn = inc.ReadByte();
                                readyBox = null; startBox = null;
                                Start();
                                break;
                        }
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        switch ((NetConnectionStatus)inc.ReadByte())
                        {
                            case NetConnectionStatus.Disconnected:
                                if (inc.ReadString() == "end")
                                    Console.WriteLine("Server was closed by server owner");
                                else
                                    Console.WriteLine("Server was closed, unknown reason");
                                Status = ClientStatus.Disconnected;
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
            Console.WriteLine("Sent init request");
        }
        void sendJoin()
        {
            NetOutgoingMessage sendMsg = client.CreateMessage();

            sendMsg.Write((byte)ServerMsg.join);
            sendMsg.Write(playerID);

            client.SendMessage(sendMsg, client.ServerConnection, NetDeliveryMethod.ReliableOrdered);
        }
        void sendReady()
        {
            ready = true;
            NetOutgoingMessage sendMsg = client.CreateMessage();

            sendMsg.Write((byte)ServerMsg.ready);
            sendMsg.Write(playerID);

            client.SendMessage(sendMsg, client.ServerConnection, NetDeliveryMethod.ReliableOrdered);
            Console.WriteLine("Sent ready to play");
        }
        ClientPlayer getPlayer(byte ID)
        {
            foreach (ClientPlayer player in playerList)
            {
                if (player.ID == ID)
                    return player;
            }
            return null;
        }
        bool checkAllReady()
        {
            int i = 0;
            foreach (ClientPlayer player in playerList)
            {
                if (player.Ready) i++;
            }
            if (i == playerList.Count - 0)
                return true;
            else return false;
        }
        void sendStart()
        {
            NetOutgoingMessage sendMsg = client.CreateMessage();

            sendMsg.Write((byte)ServerMsg.start);
            sendMsg.Write(playerID);

            client.SendMessage(sendMsg, client.ServerConnection, NetDeliveryMethod.ReliableOrdered);
        }
        void Start()
        {
            //Create WordBoard
            wordBoard = new WordBoard();
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

            if (Status != ClientStatus.Waiting_For_Connection
                && Status != ClientStatus.Disconnected)
            recMessage();

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
                    
                    break;
                case ClientStatus.Lobby:
                    readyBox.X = Main.windowWidth / 2;
                    readyBox.CheckSelect();
                    if (!ready && readyBox.CheckClicked())
                        sendReady();
                    if (isOwner)
                    {
                        startBox.X = Main.windowWidth / 2;
                        startBox.CheckSelect();
                        if (startBox.CheckClicked() && checkAllReady())
                            sendStart();
                    }
                    break;
                case ClientStatus.Starting_Game:

                    break;
            }
        }

        public void draw()
        {
            Main.sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            Color mainColor = Color.Black;

            switch(Status)
            {
                case ClientStatus.Waiting_For_Connection:
                    Main.drawText(Main.consoleFont, "Waiting for connection...", new Vector2(Main.windowWidth / 2, Main.windowHeight / 2), Color.White, 1, TextOrient.Middle);
                    mainColor = Color.White;
                    break;
                case ClientStatus.Waiting_For_Data:
                    Main.drawText(Main.consoleFont, "Waiting for data...", new Vector2(Main.windowWidth / 2, Main.windowHeight / 2), Color.White, 1, TextOrient.Middle);
                    mainColor = Color.White;
                    break;
                case ClientStatus.Disconnected:
                    Main.drawText(Main.consoleFont, "Disconnected! Press ESCAPE to exit.", new Vector2(Main.windowWidth / 2, Main.windowHeight / 2), Color.White, 1, TextOrient.Middle);
                    mainColor = Color.White;
                    break;
                case ClientStatus.Lobby:
                    Main.sb.Draw(Main.img("bg"), 
                        new Vector2(
                            (float)Math.Sin(Main.Time / 200f) * 100 - 100,
                            (float)Math.Sin(Main.Time / 200f + 20) * 100 - 100), Main.img("bg").Bounds, Color.White,
                            (float)(Math.Sin(Main.Time / 2000f)), 
                            new Vector2(Main.img("bg").Bounds.Width / 2, Main.img("bg").Bounds.Height / 2), new Vector2(6, 6), SpriteEffects.None, 0);
                    readyBox.Draw("", Main.sb);
                    startBox.Draw("", Main.sb);
                    int i = 32;
                    foreach (ClientPlayer player in playerList)
                    {
                        if (player.Ready)
                            Main.sb.Draw(Main.img("checked"), new Vector2(4, i - 20), mainColor);
                        else
                            Main.sb.Draw(Main.img("unchecked"), new Vector2(4, i - 20), mainColor);
                        Main.drawText(Main.font, (string)player.Name, new Vector2(80, i), mainColor, 1f, TextOrient.Left);
                        i += 128;
                    }
                    break;
                case ClientStatus.Starting_Game:
                    Main.sb.Draw(Main.img("bg"),
                        new Vector2(
                            (float)Math.Sin(Main.Time / 200f) * 100 - 100,
                            (float)Math.Sin(Main.Time / 200f + 20) * 100 - 100), Main.img("bg").Bounds, Color.DarkOliveGreen,
                            (float)(Math.Sin(Main.Time / 2000f)),
                            new Vector2(Main.img("bg").Bounds.Width / 2, Main.img("bg").Bounds.Height / 2), new Vector2(6, 6), SpriteEffects.None, 0);
                    Main.drawText(Main.font, "Game is starting!", new Vector2(Main.windowWidth / 2, Main.windowHeight / 2), Color.White, 1f, TextOrient.Middle);
                    Main.drawText(Main.font, "Give the server some time to generate battlefields.", new Vector2(Main.windowWidth / 2, Main.windowHeight / 2 + 64), Color.White, 0.5f, TextOrient.Middle);
                    mainColor = Color.White;
                    break;
            }

            if (!Key.down(Keys.F1) && Key.down(Keys.F2))
            {
                int i = 26;

                Main.sb.DrawString(Main.consoleFont, "Neno Client", new Vector2(4, i), mainColor); i += 16;
                Main.sb.DrawString(Main.consoleFont, "Name: " + clientName, new Vector2(4, i), mainColor); i += 16;
                Main.sb.DrawString(Main.consoleFont, "Status: " + Status, new Vector2(4, i), mainColor); i += 16;
            }

            Main.sb.End();
        }

        public void end()
        {
            client.Shutdown("end");
            Console.WriteLine("Client ended");
        }
    }
}

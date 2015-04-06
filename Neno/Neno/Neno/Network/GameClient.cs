﻿using System;
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
        Starting_Game,
        In_Game
    }
    enum Viewing
    {
        Wordboard,
        Battleboard,
        Inventory,
        Players
    }
    enum ClientMsg
    {
        init, playerInfo, playerIsReady, playerLeft, starting, newBoard, connectionTest, letterTiles, readyToStart
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
        List<BattleBoard> boardList = new List<BattleBoard>();
        List<byte> letterTiles;
        Viewing view = Viewing.Wordboard;
        

        #region In-Game GUI
        TextBox buttonWordBoard;
        TextBox buttonInventory;
        TextBox buttonOtherplayers;

        //WordBoard
        Matrix WordBoardView;
        #endregion

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
                                MediaPlayer.Play(Main.music("lobby"));
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
                            case ClientMsg.newBoard: //Recieve init battle board
                                int width = inc.ReadByte();
                                int height = inc.ReadByte();
                                int p1 = inc.ReadByte();
                                int p2 = inc.ReadByte();
                                byte[] tiles = inc.ReadBytes(width * height);

                                int entityCount = inc.ReadInt32();
                                List<Entity> entitylist = new List<Entity>();
                                for (int i = 0; i < entityCount; i++ )
                                {
                                    string entityName = inc.ReadString();
                                    int packedLength = inc.ReadInt32();
                                    byte[] entityPacked = inc.ReadBytes(packedLength);
                                    entitylist.Add(new Entity(entityName, entityPacked));
                                }
                                boardList.Add(new BattleBoard()
                                {
                                    Width = width,
                                    Height = height,
                                    player1_ID = (byte)p1,
                                    player2_ID = (byte)p2
                                });

                                break;
                            case ClientMsg.connectionTest: //Testing connected
                                sendConnectionTest();
                                break;
                            case ClientMsg.letterTiles: //Letter tiles
                                int length = inc.ReadInt32();
                                var tilesArray = inc.ReadBytes(length);
                                letterTiles = tilesArray.ToList<byte>();
                                Console.WriteLine("Recieved tiles");
                                sendDone();
                                break;
                            case ClientMsg.readyToStart: //Server confirms that all players are ready to start game
                                Status = ClientStatus.In_Game;
                                Console.WriteLine("Game is ready!");
                                break;
                        }
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        switch ((NetConnectionStatus)inc.ReadByte())
                        {
                            case NetConnectionStatus.Disconnected:
                                string disconnectMsg = inc.ReadString();
                                if (disconnectMsg == "end")
                                    Console.WriteLine("Server was closed by server owner");
                                else
                                    Console.WriteLine("Connection was closed, " + disconnectMsg);
                                Status = ClientStatus.Disconnected;
                                break;
                        }
                        break;
                }
            }
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
        
        void Start()
        {
            //Create WordBoard
            wordBoard = new WordBoard();

            //Create buttons
            buttonWordBoard = new TextBox(4, 4, "Wordboard", 0.5f, Main.font, TextOrient.Left);
            buttonInventory = new TextBox(Main.windowWidth / 2, 4, "Inventory", 0.5f, Main.font, TextOrient.Middle);
            buttonOtherplayers = new TextBox(Main.windowWidth - 4, 4, "Other Players", 0.5f, Main.font, TextOrient.Right);
        }

        #region Send Messages
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
        void sendStart()
        {
            NetOutgoingMessage sendMsg = client.CreateMessage();

            sendMsg.Write((byte)ServerMsg.start);
            sendMsg.Write(playerID);

            client.SendMessage(sendMsg, client.ServerConnection, NetDeliveryMethod.ReliableOrdered);
        }
        void sendConnectionTest()
        {
            NetOutgoingMessage sendMsg = client.CreateMessage();

            sendMsg.Write((byte)ServerMsg.testResponse);

            client.SendMessage(sendMsg, client.ServerConnection, NetDeliveryMethod.ReliableOrdered);
        }
        void sendDone()
        {
            NetOutgoingMessage sendMsg = client.CreateMessage();

            sendMsg.Write((byte)ServerMsg.readyToStart);

            client.SendMessage(sendMsg, client.ServerConnection, NetDeliveryMethod.ReliableOrdered);
        }
        #endregion

        void GameStep()
        {
            buttonWordBoard.X = 4; buttonWordBoard.Y = 4;
            buttonInventory.X = Main.windowWidth / 2; buttonInventory.Y = 4;
            buttonOtherplayers.X = Main.windowWidth - 4; buttonOtherplayers.Y = 4;

            buttonWordBoard.CheckSelect();
            buttonInventory.CheckSelect();
            buttonOtherplayers.CheckSelect();

            if (buttonWordBoard.CheckClicked())
                view = Viewing.Wordboard;
            if (buttonInventory.CheckClicked())
                view = Viewing.Inventory;
            if (buttonOtherplayers.CheckClicked())
                view = Viewing.Players;
            if (Main.mouseScrollUp && wordBoard.Zoom < 8)
                wordBoard.Zoom *= 2f;
            if (Main.mouseScrollDown && wordBoard.Zoom > 1)
                wordBoard.Zoom /= 2f;
            if (Main.mouseLeftPressed)
            {
                wordBoard.movingX = wordBoard.viewX;
                wordBoard.movingY = wordBoard.viewY;
                wordBoard.movingMouseX = Main.mousePos.X;
                wordBoard.movingMouseY = Main.mousePos.Y;
            }
            if (Main.mouseLeftDown)
            {
                wordBoard.viewX = wordBoard.movingX + Main.mousePos.X - wordBoard.movingMouseX;
                wordBoard.viewY = wordBoard.movingY + Main.mousePos.Y - wordBoard.movingMouseY;
            }
            wordBoard.viewX = MathHelper.Clamp(wordBoard.viewX, 0, Main.img("Boards/Word").Width);
            wordBoard.viewY = MathHelper.Clamp(wordBoard.viewY, 0, Main.img("Boards/Word").Height);
            WordBoardView = Matrix.CreateTranslation(-wordBoard.viewX, -wordBoard.viewY, 0) * Matrix.CreateScale(wordBoard.Zoom);
        }
        void GameDraw()
        {


            Main.sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, WordBoardView);
            switch(view)
            {
                case Viewing.Wordboard:
                    Main.sb.Draw(Main.img("Boards/Word"), Vector2.Zero, Color.White);
                    break;
            }
            Main.sb.End();
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
                        if (startBox.CheckClicked() && checkAllReady() && playerList.Count > 1)
                            sendStart();
                    }
                    break;
                case ClientStatus.Starting_Game:

                    break;
                case ClientStatus.In_Game:
                    GameStep();
                    break;
            }
        }

        public void draw()
        {

            if (Status == ClientStatus.In_Game)
                GameDraw();

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
                    if (isOwner) startBox.Draw("", Main.sb);
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
                case ClientStatus.In_Game:
                    mainColor = Color.Black;
                    Main.sb.Draw(Main.img("bg"),
                        new Vector2(
                            (float)Math.Sin(Main.Time / 900f) * 100 - 100,
                            (float)Math.Sin(Main.Time / 900f + 20) * 100 - 100), Main.img("bg").Bounds, Color.BlanchedAlmond,
                            (float)(Math.Sin(Main.Time / 2000f)),
                            new Vector2(Main.img("bg").Bounds.Width / 2, Main.img("bg").Bounds.Height / 2), new Vector2(6, 6), SpriteEffects.None, 0);
                    
                    buttonWordBoard.Draw("", Main.sb);
                    buttonInventory.Draw("", Main.sb);
                    buttonOtherplayers.Draw("", Main.sb);
                    
                    break;
            }

            if (!Key.down(Keys.F1) && Key.down(Keys.F2))
            {
                int i = 26;

                Main.sb.DrawString(Main.consoleFont, "Neno Client", new Vector2(4, i), mainColor); i += 16;
                Main.sb.DrawString(Main.consoleFont, "Name: " + clientName, new Vector2(4, i), mainColor); i += 16;
                Main.sb.DrawString(Main.consoleFont, "Status: " + Status, new Vector2(4, i), mainColor); i += 16;
                Main.sb.DrawString(Main.consoleFont, "x: " + wordBoard.viewX, new Vector2(4, i), mainColor); i += 16;
                Main.sb.DrawString(Main.consoleFont, "y: " + wordBoard.viewY, new Vector2(4, i), mainColor); i += 16;
                Main.sb.DrawString(Main.consoleFont, "z: " + wordBoard.Zoom, new Vector2(4, i), mainColor); i += 16;
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

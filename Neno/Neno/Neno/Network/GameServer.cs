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
        start = 103,
        testResponse = 104,
        readyToStart = 105,
        placeTile = 106,
        doneWord = 107
    }
    enum ServerStatus
    {
        Starting, Playing, Lobby
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
        int playerCount;
        List<BattleBoard> battleBoards = new List<BattleBoard>();
        WordBoard wordBoard;
        ServerStatus Status = ServerStatus.Starting;
        Timer pingTimer = new Timer(30, true);
        Timer timeLeftTimer = new Timer(15, true);
        int turnNumber = 1;
        int turnTime = 0;

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
            ServerPlayer player;

            while ((inc = server.ReadMessage()) != null)
            {
                switch (inc.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        switch ((ServerMsg)inc.ReadByte())
                        {
                            case ServerMsg.init: //Recieve init data request

                                if (Status == ServerStatus.Lobby)
                                {
                                    //Get name and ID
                                    string name = inc.ReadString();
                                    playerID = (byte)(lastID + 1);
                                    lastID++;
                                    Console.WriteLine("<SERVER> " + "Player " + name + " (" + playerID + ") joined");

                                    //Search and fix name dupes
                                    if (playerList.Count > 0)
                                    {
                                        int add = -1;
                                        foreach (ServerPlayer Player in playerList)
                                        {
                                            if (Player.Name == name)
                                                add = 0;
                                            else
                                                if (Player.Name == name + "_" + add)
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
                                    foreach (ServerPlayer Player in playerList)
                                    {
                                        if (Player.ID != playerID) sendPlayerInfo(inc.SenderConnection, Player.ID);
                                    }
                                }
                                else
                                {
                                    if (Status == ServerStatus.Starting) inc.SenderConnection.Disconnect("server not started");
                                    if (Status == ServerStatus.Playing) inc.SenderConnection.Disconnect("game already in progress");
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
                                    Console.WriteLine("<SERVER> first turn is " + getPlayer(turn).Name);
                                    sendStarting();
                                    Create();
                                }
                                break;
                            case ServerMsg.testResponse:
                                player = getPlayer(inc.SenderConnection);
                                player.ping = DateTime.Now.Millisecond - player.lastResponse;
                                sendPing(inc.SenderConnection);
                                if (player.ping > Settings.timeOutPing)
                                    Console.WriteLine("<SERVER> " + player.Name + " timed out!");
                                break;
                            case ServerMsg.readyToStart:
                                player = getPlayer(inc.SenderConnection);
                                player.readyToStart = true;
                                int i = 0;
                                foreach (ServerPlayer nextPlayer in playerList)
                                {
                                    if (nextPlayer.readyToStart) i++;
                                }
                                
                                if (i == playerCount - 1)
                                {
                                    foreach (ServerPlayer nextPlayer in playerList)
                                    {
                                        nextPlayer.Status = PlayerStatus.Playing;
                                    }
                                    sendAllReadyToStart();
                                    turnTime = Settings.wordBoardTimeLimit + 60;
                                    Status = ServerStatus.Playing;
                                    Console.WriteLine("<SERVER> Everybody is ready!");
                                }
                                break;
                            case ServerMsg.placeTile: //Client placed a tile
                                player = getPlayer(inc.SenderConnection);
                                byte tile = inc.ReadByte();
                                byte x = inc.ReadByte();
                                byte y = inc.ReadByte();
                                byte inList = inc.ReadByte();
                                wordBoard.tiles[x, y] = tile;
                                sendAllTile(x, y, tile);
                                if (tile > 0)
                                    player.letterTiles.RemoveAt(inList);
                                else
                                    player.letterTiles.Add(inList);


                                Console.WriteLine("<SERVER> " + player.Name + " placed a tile");
                                break;
                            case ServerMsg.doneWord: //Client finished words
                                player = getPlayer(inc.SenderConnection);
                                int wordCount = inc.ReadByte();
                                List<string> words = new List<string>();

                                //Get words
                                for (int ii = 0; ii < wordCount; ii++)
                                {
                                    words.Add(inc.ReadString());
                                    player.wordsMade++;
                                }
                                Console.WriteLine("<SERVER> " + player.Name + " finished" + wordCount + "words");

                                
                                //Refill Letters
                                while(player.letterTiles.Count < 12)
                                {
                                    byte newTile = Main.randomLetter();
                                    player.letterTiles.Add(newTile);
                                    sendNewLetterTile(inc.SenderConnection, newTile);
                                }

                                //Next turn
                                turnNumber++;
                                nextTurn();
                                sendAllTurn();
                                break;
                        }
                        break;
                        case NetIncomingMessageType.StatusChanged:
                            switch ((NetConnectionStatus)inc.ReadByte())
                            {
                                case NetConnectionStatus.Disconnected:
                                    player = getPlayer(inc.SenderConnection);
                                    if (inc.ReadString() == "end")
                                        Console.WriteLine(player.Name + " exited to menu");
                                    else
                                        Console.WriteLine(player.Name + " disconnected");
                                    sendPlayerRemoved(player.ID);
                                    playerList.Remove(player);
                                    if (Status == ServerStatus.Playing)
                                        backToLobby();
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
            sendMsg.Write(Settings.wordBoardTimeLimit);

            server.SendToAll(sendMsg, NetDeliveryMethod.ReliableOrdered);
        }
        void sendBoard(NetConnection recipient, BattleBoard board)
        {
            NetOutgoingMessage sendMsg = server.CreateMessage();

            //Player is ready
            sendMsg.Write((byte)ClientMsg.newBoard);

            //Basic info
            sendMsg.Write((byte)board.Width);
            sendMsg.Write((byte)board.Height);
            sendMsg.Write(board.player1_ID);
            sendMsg.Write(board.player2_ID);

            //Tiles
            sendMsg.Write(board.tiles);

            //Entites
            sendMsg.Write(board.entityList.Count);
            foreach(Entity entity in board.entityList)
            {
                sendMsg.Write(entity.Name);
                var packed = entity.Pack();
                sendMsg.Write(packed.Length);
                sendMsg.Write(packed);
            }

            server.SendMessage(sendMsg, recipient, NetDeliveryMethod.ReliableOrdered);
            Console.WriteLine("<SERVER> " + "BattleBoard sent to " + getPlayer(recipient).Name);
        }
        void sendTestConnection()
        {
            foreach(ServerPlayer player in playerList)
            {
                NetOutgoingMessage sendMsg = server.CreateMessage();

                //Starting game
                sendMsg.Write((byte)ClientMsg.connectionTest);

                server.SendMessage(sendMsg, player.Connection, NetDeliveryMethod.ReliableOrdered);

                player.lastResponse = DateTime.Now.Millisecond;
            }

            
        }
        void sendLetterTiles(NetConnection recipient, ServerPlayer player)
        {
            byte[] tilesArray = player.letterTiles.ToArray();

            NetOutgoingMessage sendMsg = server.CreateMessage();

            sendMsg.Write((byte)ClientMsg.letterTiles);
            sendMsg.Write(tilesArray.Length);
            sendMsg.Write(tilesArray);

            server.SendMessage(sendMsg, recipient, NetDeliveryMethod.ReliableOrdered);
            Console.WriteLine("<SERVER> " + "Sent letter tiles to " + player.Name);
        }
        void sendAllReadyToStart()
        {
            NetOutgoingMessage sendMsg = server.CreateMessage();

            //Starting game
            sendMsg.Write((byte)ClientMsg.readyToStart);

            server.SendToAll(sendMsg, NetDeliveryMethod.ReliableOrdered);
            Console.WriteLine("<SERVER> All ready to start!");
        }
        void sendAllTile(byte x, byte y, byte tile)
        {
            NetOutgoingMessage sendMsg = server.CreateMessage();

            //Starting game
            sendMsg.Write((byte)ClientMsg.tilePlace);
            sendMsg.Write(tile);
            sendMsg.Write(x);
            sendMsg.Write(y);

            server.SendToAll(sendMsg, NetDeliveryMethod.ReliableOrdered);
        }
        void sendNewLetterTile(NetConnection recipient, byte tile)
        {
            NetOutgoingMessage sendMsg = server.CreateMessage();

            sendMsg.Write((byte)ClientMsg.newLetterTile);
            sendMsg.Write(tile);

            server.SendMessage(sendMsg, recipient, NetDeliveryMethod.ReliableOrdered);
            Console.WriteLine("<SERVER> " + "Sent new letter tile to " + getPlayer(recipient).Name);
        }
        void sendAllTurn()
        {
            NetOutgoingMessage sendMsg = server.CreateMessage();

            //Starting game
            sendMsg.Write((byte)ClientMsg.newTurn);
            sendMsg.Write(turn);
            sendMsg.Write(turnNumber);

            server.SendToAll(sendMsg, NetDeliveryMethod.ReliableOrdered);
        }
        void sendPing(NetConnection recipient)
        {
            NetOutgoingMessage sendMsg = server.CreateMessage();

            sendMsg.Write((byte)ClientMsg.ping);
            sendMsg.Write(getPlayer(recipient).ping);

            server.SendMessage(sendMsg, recipient, NetDeliveryMethod.UnreliableSequenced);
        }
        void sendBackToLobby()
        {
            NetOutgoingMessage sendMsg = server.CreateMessage();

            sendMsg.Write((byte)ClientMsg.backToLobby);

            server.SendToAll(sendMsg, NetDeliveryMethod.ReliableOrdered);
        }
        void sendCurrentTurnTime(NetConnection recipient)
        {
            NetOutgoingMessage sendMsg = server.CreateMessage();

            sendMsg.Write((byte)ClientMsg.currentTimeLeft);
            sendMsg.Write(turnTime);

            server.SendMessage(sendMsg, recipient, NetDeliveryMethod.ReliableSequenced);
        }
        void sendTimeUp(NetConnection recipient)
        {
            NetOutgoingMessage sendMsg = server.CreateMessage();

            sendMsg.Write((byte)ClientMsg.timeUp);

            server.SendMessage(sendMsg, recipient, NetDeliveryMethod.ReliableOrdered);
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
        void nextTurn()
        {
            //Get current turn player's index in list
            var index = playerList.IndexOf(getPlayer(turn));

            //Increment index by 1
            if (index < playerList.Count - 1)
                index++;
            else
                index = 0;

            //New turn is ID of next player
            turn = playerList[index].ID;

            //Time limit
            turnTime = Settings.wordBoardTimeLimit;

            Console.WriteLine("<SERVER> it's now " + playerList[index].Name + "'s turn!");
        }
        void backToLobby()
        {
            sendBackToLobby();
            foreach (ServerPlayer player in playerList)
            { 
                player.ready = false;
                player.readyToStart = false;
                player.Status = PlayerStatus.Lobby;
                player.letterTiles.Clear();
                player.wordsMade = 0;
            }
            turnNumber = 1;
            turn = 0;
            battleBoards.Clear();
            wordBoard = null;
            battleBoardCount = 0;
            Status = ServerStatus.Lobby;
        }

        void Create()
        {
            //Create wordboard
            wordBoard = new WordBoard();

            //Create and send battleboards
            playerCount = playerList.Count;
            battleBoardCount = ((playerCount - 1) * ((playerCount - 1) + 1)) / 2; //1,3,6,10,15...
            for (int ii = 0; ii < playerCount - 1; ii++)
            {
                int nextplayer = 0;
                for (int i = 0; i < MathHelper.Clamp(battleBoardCount - (ii + 1), 1, 15); i++)
                {
                    var nextPartner = nextplayer + (ii + 1);
                    var nextBoard = new BattleBoard(playerList[nextplayer].ID, playerList[nextPartner].ID);
                    battleBoards.Add(nextBoard);
                    sendBoard(playerList[nextplayer].Connection, nextBoard);
                    sendBoard(playerList[nextPartner].Connection, nextBoard);
                    nextplayer++;
                }
            }

            //Send player letter tiles
            foreach (ServerPlayer player in playerList)
            {
                sendLetterTiles(player.Connection, player);
            }

        }

        
        

        #endregion




        public void init()
        {
            startServer();
        }

        public void step()
        {
            if (Key.pressed(Keys.Home)) Main.Switch(Focus.Menu);

            switch(Status)
            {
                case ServerStatus.Starting:
                    if (server.Status == NetPeerStatus.Running)
                        Status = ServerStatus.Lobby;
                    break;
                case ServerStatus.Lobby:
                    if (pingTimer.tick)
                        sendTestConnection();
                    break;
                case ServerStatus.Playing:
                    if (pingTimer.tick)
                        sendTestConnection();
                    if (turnTime > 0)
                    {
                        turnTime -= 1;
                        if (timeLeftTimer.tick)
                            sendCurrentTurnTime(getPlayer(turn).Connection);
                        if (turnTime == 0)
                        { 
                            //Forced end turn
                            sendTimeUp(getPlayer(turn).Connection);
                            getPlayer(turn).letterTiles.Clear();
                            for (int i = 0; i < 12; i++)
                                getPlayer(turn).letterTiles.Add(Main.randomLetter());
                            sendLetterTiles(getPlayer(turn).Connection, getPlayer(turn));
                            turnNumber++;
                            nextTurn();
                            sendAllTurn();
                        }
                    }
                    break;
            }

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
                    if (Status == ServerStatus.Playing)
                    {
                        Main.sb.DrawString(Main.consoleFont, "Turn: " + getName(turn), new Vector2(4, i), Color.White); i += 16;
                        Main.sb.DrawString(Main.consoleFont, "Player Ping:", new Vector2(4, i), Color.White); i += 16;
                        foreach (ServerPlayer player in playerList)
                        {
                            Main.sb.DrawString(Main.consoleFont, player.Name + " = " + player.ping + "ms", new Vector2(4, i), Color.White); i += 16;
                        }
                    }
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

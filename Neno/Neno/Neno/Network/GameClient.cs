using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
        In_Game,
        None
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
        init, playerInfo, playerIsReady, playerLeft, starting, newBoard, connectionTest, letterTiles, readyToStart, tilePlace, newLetterTile,
        newTurn, newWordNumber, ping, backToLobby, currentTimeLeft, timeUp
    }
    enum Direction
    {
        Horizontal, Vertical, None
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
        TextBox timeLimitBox;
        bool ready = false;
        public bool isOwner = false;
        byte turn;
        WordBoard wordBoard;
        List<BattleBoard> boardList = new List<BattleBoard>();
        List<byte> letterTiles;
        List<string> wordsCreated = new List<string>();
        Viewing view = Viewing.Wordboard;
        bool canInteract = false;
        Direction direction = Direction.None;
        int turnNumber = 1;
        int wordsMade = 0;
        int ping = 0;
        bool choosingWild = false;
        int currentTurnTime = 0;

        #region In-Game GUI
        //Main Buttons
        TextBox buttonWordBoard;
        TextBox buttonInventory;
        TextBox buttonOtherplayers;

        //Button Groups
        List<TextBox> buttonsWordBoard = new List<TextBox>();

        //WordBoard
        Matrix WordBoardView;
        List<Vector2> placedTiles = new List<Vector2>();
        List<Vector2> placedWilds = new List<Vector2>();

        //Letter Tile Tray
        int trayWidth;
        int trayPixelWidth;
        int selectedLetter = -1;
        bool pickupLetter = false;
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
            byte ID, tile;

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
                                    timeLimitBox = new TextBox(Main.windowWidth - 4, 120, "WordBoard Time Limit: ", 0.5f, Main.font, TextOrient.Right, true, (Settings.wordBoardTimeLimit / 60).ToString());
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
                                if (Status == ClientStatus.In_Game)
                                    Status = ClientStatus.None;
                                break;
                            case ClientMsg.starting: //The game has been started
                                Status = ClientStatus.Starting_Game;
                                turn = inc.ReadByte();
                                Settings.wordBoardTimeLimit = inc.ReadInt32();
                                if (turn == playerID)
                                    canInteract = true;
                                else
                                    canInteract = false;
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
                                Console.WriteLine("It's " + getPlayer(turn).Name + "'s turn!");
                                break;
                            case ClientMsg.tilePlace: //Tile placed on board
                                tile = inc.ReadByte();
                                byte x = inc.ReadByte();
                                byte y = inc.ReadByte();
                                wordBoard.tiles[x, y] = tile;
                                break;
                            case ClientMsg.newLetterTile: //Get a new placeable tile
                                tile = inc.ReadByte();
                                letterTiles.Add(tile);
                                break;
                            case ClientMsg.newTurn: //New turn
                                turn = inc.ReadByte();
                                turnNumber = inc.ReadInt32();
                                Console.WriteLine("It's now " + getPlayer(turn).Name + "'s turn!");
                                if (turn == playerID)
                                    canInteract = true;
                                else
                                    canInteract = false;
                                break;
                            case ClientMsg.ping: //Connection speed
                                ping = inc.ReadInt32();
                                break;
                            case ClientMsg.backToLobby: //Someone disconnected during the game, go back to lobby
                                boardList.Clear();
                                wordBoard = null;
                                ready = false;
                                foreach(ClientPlayer player in playerList)
                                { player.Ready = false; }
                                Status = ClientStatus.Lobby;
                                wordsCreated.Clear();
                                wordsMade = 0;
                                turn = 0;
                                turnNumber = 0;
                                readyBox = new TextBox(Main.windowWidth / 2, 16, "I'm Ready!", 0.5f, Main.font, TextOrient.Middle);
                                if (isOwner)
                                {
                                    startBox = new TextBox(Main.windowWidth / 2, 80, "Start Game", 1f, Main.font, TextOrient.Middle);
                                    startBox.Description = "Start game, if all players are ready";
                                }
                                view = Viewing.Wordboard;
                                break;
                            case ClientMsg.currentTimeLeft: //Time left in turn
                                currentTurnTime = inc.ReadInt32();
                                break;
                            case ClientMsg.timeUp: //Time ran out for this turn
                                foreach(Vector2 pos in placedTiles)
                                {
                                    //Send to server
                                    sendTilePlace((int)pos.X, (int)pos.Y, 0, 1);

                                    //Edit board
                                    wordBoard.tiles[(int)pos.X, (int)pos.Y] = 0;

                                    //Remove from placed tiles list
                                    placedTiles.Remove(new Vector2(wordBoard.selectX, wordBoard.selectY));
                                }
                                EndTurn();
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

        bool checkWord(string word)
        {
            return (Main.wordsList.Contains(word));
        }
        void findAllWords()
        {
            wordsCreated.Clear();
            foreach (Vector2 pos in placedTiles)
            {
                var nextWords = findWords((int)pos.X, (int)pos.Y);
                if (nextWords.Count > 0) if (!wordsCreated.Contains(nextWords[0])) wordsCreated.Add(nextWords[0]);
                if (nextWords.Count > 1) if (!wordsCreated.Contains(nextWords[1])) wordsCreated.Add(nextWords[1]);
            }
            Console.WriteLine("Found " + wordsCreated.Count + " words");
        }
        List<string> findWords(int x, int y)
        {
            //Find words adjacent from an origin
            List<string> newWords = new List<string>();
            int originX = x; //Leftmost
            int originY = y; //Topmost
            int X = x; //Current X coord, used
            int Y = y; //Current Y coord, used

            //Find leftmost tile
            while (wordBoard.tiles[originX - 1, y] >= 1 && wordBoard.tiles[originX - 1, y] <= 26)
            {
                originX--;
            }
            X = originX;

            //Find topmost tile
            while (wordBoard.tiles[x, originY - 1] >= 1 && wordBoard.tiles[x, originY - 1] <= 26)
            {
                originY--;
            }
            Y = originY;

            //Find horizontal word
            string word1 = "";
            while (wordBoard.tiles[X, y] >= 1 && wordBoard.tiles[X, y] <= 26)
            {
                word1 += Main.wordTileLetter[wordBoard.tiles[X, y]];
                X++;
            }
            if (word1.Length > 1) newWords.Add(word1);

            //Find vertical word
            string word2 = "";
            while (wordBoard.tiles[x, Y] >= 1 && wordBoard.tiles[x, Y] <= 26)
            {
                word2 += Main.wordTileLetter[wordBoard.tiles[x, Y]];
                Y++;
            }
            if (word2.Length > 1) newWords.Add(word2);

            return newWords;
        }
        bool checkConnectedToOtherWords()
        {
            bool isConnected = false;

            foreach(Vector2 pos in placedTiles)
            {
                if ((wordBoard.tiles[(int)pos.X + 1, (int)pos.Y] > 0 && !placedTiles.Contains(new Vector2(pos.X + 1, pos.Y))) ||
                    (wordBoard.tiles[(int)pos.X - 1, (int)pos.Y] > 0 && !placedTiles.Contains(new Vector2(pos.X - 1, pos.Y))) ||
                    (wordBoard.tiles[(int)pos.X, (int)pos.Y + 1] > 0 && !placedTiles.Contains(new Vector2(pos.X, pos.Y + 1))) ||
                    (wordBoard.tiles[(int)pos.X, (int)pos.Y - 1] > 0 && !placedTiles.Contains(new Vector2(pos.X, pos.Y - 1))))
                    isConnected = true;
            }

            return isConnected;
        }
        bool checkCanPlaceTile(int x, int y)
        {
            bool canPlace = true;

            if (wordBoard.tiles[x, y] != 0)
                return false;

            if (x == 34 && y == 34)
                canPlace = true;
            else
            {
                if (direction == Direction.None)
                    canPlace = true;
                else
                    if (direction == Direction.Horizontal && wordBoard.tiles[x + 1, y] == 0 && wordBoard.tiles[x - 1, y] == 0)
                        canPlace = false;
                    else
                        if (direction == Direction.Vertical && wordBoard.tiles[x, y - 1] == 0 && wordBoard.tiles[x, y + 1] == 0)
                            canPlace = false;

                if (turnNumber == 1)
                if (wordBoard.tiles[x + 1, y] == 0 &&
                    wordBoard.tiles[x - 1, y] == 0 &&
                    wordBoard.tiles[x, y + 1] == 0 &&
                    wordBoard.tiles[x, y - 1] == 0)
                    return false;

                if (placedTiles.Count == 1)
                    if (placedTiles[0].X != x && placedTiles[0].Y != y)
                        return false;
            }

            return canPlace;
        }
        void placeTile(int x, int y, byte tile)
        {
            //Tile place
            sendTilePlace(x, y, tile, selectedLetter);
            wordBoard.tiles[x, y] = tile;

            //Remove from tiles
            letterTiles.RemoveAt(selectedLetter);

            //Add to pos list
            placedTiles.Add(new Vector2(x, y));

            //Decide direction
            if (placedTiles.Count == 2)
            {
                if (placedTiles[0].X > x)
                    direction = Direction.Horizontal;
                if (placedTiles[0].X < x)
                    direction = Direction.Horizontal;
                if (placedTiles[0].Y > y)
                    direction = Direction.Vertical;
                if (placedTiles[0].Y < y)
                    direction = Direction.Vertical;
            }

            //Reset
            pickupLetter = false;
            selectedLetter = -1;
        }

        
        void Start()
        {
            //Create WordBoard
            wordBoard = new WordBoard();

            //Create buttons
            buttonWordBoard = new TextBox(4, 4, "Wordboard", 0.5f, Main.font, TextOrient.Left);
            buttonInventory = new TextBox(Main.windowWidth / 2, 4, "Inventory", 0.5f, Main.font, TextOrient.Middle);
            buttonOtherplayers = new TextBox(Main.windowWidth - 4, 4, "Other Players", 0.5f, Main.font, TextOrient.Right);

            //WordBoard Buttons
            buttonsWordBoard.Add(new TextBox(0, Main.windowHeight - 106, "Submit Word", 0.5f, Main.font));
            buttonsWordBoard.Add(new TextBox(Main.windowWidth, Main.windowHeight / 2, "Choose Letter: ", 1f, Main.font, TextOrient.Middle, true, ""));
            buttonsWordBoard[1].boxColor.A = 180;
        }
        void EndTurn()
        {
            canInteract = false;
            direction = Direction.None;
            placedTiles.Clear();
            wordsCreated.Clear();
            placedWilds.Clear();
        }

        #region Send Messages
        //Pre-game
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
        

        //Game
        void sendTilePlace(int x, int y, byte tile, int tilePositionInList)
        {
            NetOutgoingMessage sendMsg = client.CreateMessage();

            sendMsg.Write((byte)ServerMsg.placeTile);
            sendMsg.Write(tile);
            sendMsg.Write((byte)x);
            sendMsg.Write((byte)y);
            sendMsg.Write((byte)tilePositionInList);

            client.SendMessage(sendMsg, client.ServerConnection, NetDeliveryMethod.ReliableOrdered);
        }
        void sendWords(List<string> words)
        {
            NetOutgoingMessage sendMsg = client.CreateMessage();

            sendMsg.Write((byte)ServerMsg.doneWord);
            sendMsg.Write((byte)words.Count);
            foreach (string Word in words)
            { 
                sendMsg.Write(Word);
                wordsMade++;
                Console.WriteLine("Checked and sent word: " + Word);
            }

            client.SendMessage(sendMsg, client.ServerConnection, NetDeliveryMethod.ReliableOrdered);
        }
        #endregion

        void GameStep()
        {
            //Buttons
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
            if (view != Viewing.Wordboard)
            {
                pickupLetter = false;
                selectedLetter = -1;
            }

            switch(view)
            { 
                case Viewing.Wordboard:
                #region WordBoard
            if (!pickupLetter)
            switch(view)
            {
                case Viewing.Wordboard:
                    foreach(TextBox nextBox in buttonsWordBoard)
                    {
                        switch(nextBox.Text)
                        {
                            case "Submit Word":
                                #region Submitting Words
                                nextBox.Y = Main.windowHeight - 106;
                                nextBox.CheckSelect();
                                if (canInteract)
                                {
                                    if (nextBox.CheckClicked())
                                    {
                                        if (checkConnectedToOtherWords() || turnNumber == 1)
                                        {
                                            findAllWords();
                                            List<string> realWords = new List<string>();
                                            foreach (string nextWord in wordsCreated)
                                            {
                                                if (checkWord(nextWord))
                                                {
                                                    realWords.Add(nextWord);
                                                }
                                                else
                                                {
                                                    Console.WriteLine(nextWord + " is not a word!");
                                                    return;
                                                }
                                            }
                                            if (realWords.Count > 0)
                                            {
                                                sendWords(realWords);
                                                EndTurn();
                                            }
                                        }
                                    }
                                }
                                #endregion
                                break;
                            case "Choose Letter: ":
                                #region Choose a letter for a wild
                                nextBox.Y = Main.windowHeight / 2;
                                nextBox.X = Main.windowWidth / 2;
                                nextBox.CheckSelect();
                                if (choosingWild)
                                { 
                                    nextBox.dontDraw = false;
                                    if (nextBox.typeText.Length == 1 && nextBox.clicked)
                                        if (nextBox.typeText == "a" || nextBox.typeText == "A" ||
                                            nextBox.typeText == "b" || nextBox.typeText == "B" ||
                                            nextBox.typeText == "c" || nextBox.typeText == "C" ||
                                            nextBox.typeText == "d" || nextBox.typeText == "D" ||
                                            nextBox.typeText == "e" || nextBox.typeText == "E" ||
                                            nextBox.typeText == "f" || nextBox.typeText == "F" ||
                                            nextBox.typeText == "g" || nextBox.typeText == "G" ||
                                            nextBox.typeText == "h" || nextBox.typeText == "H" ||
                                            nextBox.typeText == "i" || nextBox.typeText == "I" ||
                                            nextBox.typeText == "j" || nextBox.typeText == "J" ||
                                            nextBox.typeText == "k" || nextBox.typeText == "K" ||
                                            nextBox.typeText == "l" || nextBox.typeText == "L" ||
                                            nextBox.typeText == "m" || nextBox.typeText == "M" ||
                                            nextBox.typeText == "n" || nextBox.typeText == "N" ||
                                            nextBox.typeText == "o" || nextBox.typeText == "O" ||
                                            nextBox.typeText == "p" || nextBox.typeText == "P" ||
                                            nextBox.typeText == "q" || nextBox.typeText == "Q" ||
                                            nextBox.typeText == "r" || nextBox.typeText == "R" ||
                                            nextBox.typeText == "s" || nextBox.typeText == "S" ||
                                            nextBox.typeText == "t" || nextBox.typeText == "T" ||
                                            nextBox.typeText == "u" || nextBox.typeText == "U" ||
                                            nextBox.typeText == "v" || nextBox.typeText == "V" ||
                                            nextBox.typeText == "w" || nextBox.typeText == "W" ||
                                            nextBox.typeText == "x" || nextBox.typeText == "X" ||
                                            nextBox.typeText == "y" || nextBox.typeText == "Y" ||
                                            nextBox.typeText == "z" || nextBox.typeText == "Z")
                                    {
                                        placeTile((int)placedWilds[placedWilds.Count - 1].X, (int)placedWilds[placedWilds.Count - 1].Y, (byte)Array.IndexOf(Main.wordTileLetter, nextBox.typeText));
                                        choosingWild = false;
                                        canInteract = true;
                                        nextBox.typeText = "";
                                    }
                                    if (Key.pressed(Keys.Escape))
                                    {
                                        choosingWild = false;
                                        canInteract = true;
                                        placedWilds.RemoveAt(placedWilds.Count - 1);
                                    }
                                }
                                else
                                    nextBox.dontDraw = true;
                                #endregion
                                break;
                        }
                    }
                    break;
            }

            //WordBoard Scrolling
            #region Scrolling Board
            if (Main.mouseScrollUp && wordBoard.Zoom < 8)
            {
                wordBoard.Zoom *= 2f;
                wordBoard.viewX = wordBoard.viewX + (Main.mousePos.X / wordBoard.Zoom);
                wordBoard.viewY = wordBoard.viewY + (Main.mousePos.Y / wordBoard.Zoom);
            }
            if (Main.mouseScrollDown && wordBoard.Zoom > 1)
            {
                wordBoard.viewX = wordBoard.viewX - (Main.mousePos.X / wordBoard.Zoom);
                wordBoard.viewY = wordBoard.viewY - (Main.mousePos.Y / wordBoard.Zoom);
                wordBoard.Zoom /= 2f;
            }
            if (Main.mouseLeftPressed && !new Rectangle(0, Main.windowHeight - 110, Main.windowWidth, 110).Contains(Main.mousePosP))
            {
                wordBoard.movingX = wordBoard.viewX;
                wordBoard.movingY = wordBoard.viewY;
                wordBoard.movingMouseX = Main.mousePos.X;
                wordBoard.movingMouseY = Main.mousePos.Y;
                wordBoard.Moving = true;
            }
            if (Main.mouseLeftDown && wordBoard.Moving)
            {
                wordBoard.viewX = wordBoard.movingX + (-Main.mousePos.X + wordBoard.movingMouseX) / wordBoard.Zoom;
                wordBoard.viewY = wordBoard.movingY + (-Main.mousePos.Y + wordBoard.movingMouseY) / wordBoard.Zoom;
            }
            else
                wordBoard.Moving = false;
            wordBoard.viewX = MathHelper.Clamp(wordBoard.viewX, 0, Main.img("Boards/Word").Width - (Main.windowWidth / wordBoard.Zoom));
            wordBoard.viewY = MathHelper.Clamp(wordBoard.viewY, 0, Main.img("Boards/Word").Height - (Main.windowHeight / wordBoard.Zoom));
            WordBoardView = Matrix.CreateTranslation(-wordBoard.viewX, -wordBoard.viewY, 0) * Matrix.CreateScale(wordBoard.Zoom);
            #endregion

            //WordBoard Select
            wordBoard.selectX = (int)MathHelper.Clamp((float)(Math.Floor((wordBoard.viewX + Main.mousePos.X / wordBoard.Zoom) / 8f)), 0, wordBoard.Size);
            wordBoard.selectY = (int)MathHelper.Clamp((float)(Math.Floor((wordBoard.viewY + Main.mousePos.Y / wordBoard.Zoom) / 8f)), 0, wordBoard.Size);
            if (pickupLetter)
            {
                //Place a tile
                if (Main.mouseRightPressed 
                    && !new Rectangle(0, Main.windowHeight - 110, Main.windowWidth, 110).Contains(Main.mousePosP)
                    && checkCanPlaceTile((int)wordBoard.selectX, (int)wordBoard.selectY))
                {
                    int x = wordBoard.selectX;
                    int y = wordBoard.selectY;

                    if (letterTiles[selectedLetter] != 27)
                    {
                        placeTile(x, y, letterTiles[selectedLetter]);
                        choosingWild = false;
                    }
                    else
                    //Add to wild list if wild
                    {
                        wordBoard.tiles[x, y] = 27;
                        placedWilds.Add(new Vector2(x, y));
                        choosingWild = true;
                        canInteract = false;
                        pickupLetter = false;
                        buttonsWordBoard[1].clicked = true;
                    }
                }
                else
                    if ((Main.mouseRightPressed || (Main.mouseLeftPressed)
                    && new Rectangle(0, Main.windowHeight - 110, Main.windowWidth, 110).Contains(Main.mousePosP)))
                    {
                        pickupLetter = false;
                        selectedLetter = -1;
                    }
            }
            else
            {
                //Pick back up a tile
                if (placedTiles.Contains(new Vector2(wordBoard.selectX, wordBoard.selectY)))
                if (Main.mouseRightPressed && !new Rectangle(0, Main.windowHeight - 110, Main.windowWidth, 110).Contains(Main.mousePosP) && wordBoard.tiles[wordBoard.selectX, wordBoard.selectY] != 0)
                {
                    //Send to server
                    if (!placedWilds.Contains(new Vector2(wordBoard.selectX, wordBoard.selectY)))
                        sendTilePlace(wordBoard.selectX, wordBoard.selectY, 0, wordBoard.tiles[wordBoard.selectX, wordBoard.selectY]);
                    else
                        sendTilePlace(wordBoard.selectX, wordBoard.selectY, 0, 27);

                    //Add back to tiles tray
                    if (!placedWilds.Contains(new Vector2(wordBoard.selectX, wordBoard.selectY)))
                        letterTiles.Add(wordBoard.tiles[wordBoard.selectX, wordBoard.selectY]);
                    else
                        letterTiles.Add(27);

                    //Remove from wild list if wild
                    if (placedWilds.Contains(new Vector2(wordBoard.selectX, wordBoard.selectY)))
                        placedWilds.Remove(new Vector2(wordBoard.selectX, wordBoard.selectY));
                    
                    //Edit board
                    wordBoard.tiles[wordBoard.selectX, wordBoard.selectY] = 0;

                    //Remove from placed tiles list
                    placedTiles.Remove(new Vector2(wordBoard.selectX, wordBoard.selectY));


                    if (placedTiles.Count <= 1)
                        direction = Direction.None;
                }
            }

            //Tray
            trayWidth = (int)Math.Ceiling(letterTiles.Count / 2f);
            trayPixelWidth = trayWidth * Main.img("trayMiddle").Width;
            if (canInteract)
            {
                if (!pickupLetter)
                {
                    selectedLetter = -1;
                    if (new Rectangle(0, Main.windowHeight - 110, Main.windowWidth, 110).Contains(Main.mousePosP))
                        for (int i = 0; i < letterTiles.Count; i++)
                        {
                            if ((new Rectangle(
                                (int)(Main.windowWidth / 2 - ((letterTiles.Count / 2) * 64) + i * 64),
                                (int)(Main.windowHeight - 74),
                                64, 64)).Contains((int)Main.mousePos.X, (int)Main.mousePos.Y))
                                selectedLetter = i;
                        }
                }
                if (Main.mouseLeftPressed && selectedLetter != -1)
                {
                    pickupLetter = true;
                }
            }
            #endregion
                    break;
            }
        }
        void GameDraw()
        {
            //Background
            Main.sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            #region BG
            Color color = Color.BlanchedAlmond;
            if (view == Viewing.Inventory) color = Color.DarkTurquoise;
            if (view == Viewing.Players) color = Color.DarkSlateBlue;

            Main.sb.Draw(Main.img("bg"),
                new Vector2(
                    (float)Math.Sin(Main.Time / 900f) * 100 - 100,
                    (float)Math.Sin(Main.Time / 900f + 20) * 100 - 100), Main.img("bg").Bounds, color,
                    (float)(Math.Sin(Main.Time / 2000f)),
                    new Vector2(Main.img("bg").Bounds.Width / 2, Main.img("bg").Bounds.Height / 2), new Vector2(6, 6), SpriteEffects.None, 0);
            #endregion
            Main.sb.End();

            //Relative to Board
            if (view == Viewing.Wordboard)
            {
                Main.sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, WordBoardView);
                #region Board
                switch (view)
                {
                    case Viewing.Wordboard:
                        Main.sb.Draw(Main.img("Boards/Word"), Vector2.Zero, Color.White);
                        break;
                }

                //Draw Tiles
                for (int x = 0; x < wordBoard.Size; x++)
                {
                    for (int y = 0; y < wordBoard.Size; y++)
                    {
                        if (wordBoard.tiles[x, y] > 0 && wordBoard.tiles[x, y] <= 27)
                        {
                            color = Color.White;
                            if (placedTiles.Contains(new Vector2(x, y)))
                                color = Color.Goldenrod;
                            Main.sb.Draw(Main.img(Main.wordTileImg[wordBoard.tiles[x, y]]), new Rectangle(x * 8, y * 8, 8, 8), color);
                        }
                    }
                }
                #endregion
                Main.sb.End();
            }

            //Top GUI
            Main.sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            #region Top GUI
            int xx = 0; int yy = 0;

            //Darken
            if (choosingWild)
                Main.DrawRectangle(new Rectangle(0, 0, Main.windowWidth, Main.windowHeight), new Color(0f, 0f, 0f, 0.4f));

            //Views
            switch(view)
            {
                case Viewing.Wordboard:
                    #region WordBoard GUI
                    //Tiles
                    Main.DrawRectangle(0, Main.windowHeight - 110, Main.windowWidth, 110, new Color(0, 0, 0, 0.3f), false);
                    int i = 0;
                    foreach (byte Tile in letterTiles)
                    {
                        var nextImg = Main.img(Main.wordTileImg[Tile]);
                        if (selectedLetter != i)
                            Main.sb.Draw(nextImg, new Vector2(
                                Main.windowWidth / 2 - ((letterTiles.Count / 2) * 64) + i * 64, Main.windowHeight - 74),
                                nextImg.Bounds, Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0);
                        else
                            Main.sb.Draw(nextImg, new Rectangle(
                                (int)(Main.windowWidth / 2 - ((letterTiles.Count / 2) * 64) + i * 64 - 8),
                                (int)(Main.windowHeight - 74 - 8),
                                (int)(64 + 16), (int)(64 + 16)), Color.Turquoise);
                        i++;
                    }

                    //Picked up tile
                    if (pickupLetter && !choosingWild)
                    {
                        Main.sb.Draw(Main.img(Main.wordTileImg[letterTiles[selectedLetter]]), new Rectangle((int)Main.mousePos.X - 32, (int)Main.mousePos.Y - 32, 64, 64), Color.White);
                        Main.drawText(Main.consoleFont, "Right click to place", new Vector2(Main.mousePos.X, Main.mousePos.Y - 85), Color.Black, 1, TextOrient.Middle);
                    }

                    //Other
                    foreach (TextBox nextBox in buttonsWordBoard)
                    {
                        nextBox.Draw("", Main.sb);
                    }
                    Main.drawText(Main.font, "Words Made: " + wordsMade, new Vector2(4, Main.windowHeight - 140), Color.Black, 0.5f, TextOrient.Left);
                    Main.drawText(Main.font, "Turn #: " + turnNumber, new Vector2(4, Main.windowHeight - 170), Color.Black, 0.5f, TextOrient.Left);
                    #endregion
                    break;
                case Viewing.Inventory:
                    #region Inventory
                    xx = 4; yy = 72;
                    Main.drawText(Main.consoleFont, "Name: " + clientName, new Vector2(xx, yy), Color.White, 1f, TextOrient.Left); yy += 18;
                    Main.drawText(Main.consoleFont, "Server Name: " + serverName, new Vector2(xx, yy), Color.White, 1f, TextOrient.Left); yy += 18;
                    Main.drawText(Main.consoleFont, "Players: " + playerList.Count, new Vector2(xx, yy), Color.White, 1f, TextOrient.Left); yy += 18;
                    Main.drawText(Main.consoleFont, "Ping: " + ping, new Vector2(xx, yy), Color.White, 1f, TextOrient.Left); yy += 18;
                    Main.drawText(Main.consoleFont, "Words Made: " + wordsMade, new Vector2(xx, yy), Color.White, 1f, TextOrient.Left); yy += 18;
                    Main.drawText(Main.consoleFont, "Turn Number: " + turnNumber, new Vector2(xx, yy), Color.White, 1f, TextOrient.Left); yy += 18;
                    Main.drawText(Main.consoleFont, "Time Limit: " + (Settings.wordBoardTimeLimit / 60) + " seconds", new Vector2(xx, yy), Color.White, 1f, TextOrient.Left); yy += 18;
                    #endregion
                    break;
                case Viewing.Players:
                    #region Players
                    xx = 4; yy = 72;
                    Main.drawText(Main.consoleFont, "Players", new Vector2(xx, yy), Color.Black, 1f, TextOrient.Left); yy += 18;
                    foreach(ClientPlayer next in playerList)
                    {
                        color = Color.White;
                        if (turn == next.ID) color = Color.Orange;
                        Main.drawText(Main.consoleFont, "(" + next.ID + ") " + next.Name, new Vector2(xx, yy), color, 1f, TextOrient.Left); yy += 18;
                    }
                    #endregion
                    break;
            }

            //Who's Turn
            if (turn == playerID)
            {
                Color turnTimeColor = Color.Black;
                if (currentTurnTime < Settings.wordBoardTimeLimit / 2) turnTimeColor = Color.BlueViolet;
                if (currentTurnTime < Settings.wordBoardTimeLimit / 3) turnTimeColor = Color.DarkOrange;
                if (currentTurnTime < Settings.wordBoardTimeLimit / 4) turnTimeColor = Color.DarkRed;
                if (currentTurnTime < Settings.wordBoardTimeLimit / 5) turnTimeColor = new Color((float)Math.Sin(Main.Time / 3), (float)Math.Sin(Main.Time / 3), 0f);
                Main.drawText(Main.font, "Time left: " + Math.Floor((double)currentTurnTime / 60), new Vector2(Main.windowWidth / 2, 152), new Color(0f, 0f, 0f, 0.5f), 0.75f, TextOrient.Middle);
                Main.drawText(Main.font, "Time left: " + Math.Floor((double)currentTurnTime / 60), new Vector2(Main.windowWidth / 2, 150), turnTimeColor, 0.75f, TextOrient.Middle);
                Main.drawText(Main.font, "It's Your Turn!", new Vector2(Main.windowWidth / 2, (float)(74 + Math.Sin(Main.Time / 16) * 6)), Color.Black, 0.75f, TextOrient.Middle);
                Main.drawText(Main.font, "It's Your Turn!", new Vector2(Main.windowWidth / 2, (float)(72 + Math.Sin(Main.Time / 16) * 6)), Color.DarkTurquoise, 0.75f, TextOrient.Middle);
            }
            else
                Main.drawText(Main.font, getPlayer(turn).Name + "'s turn", new Vector2(Main.windowWidth / 2, 72), Color.Black, 0.75f, TextOrient.Middle);

            #endregion
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
            if (Key.pressed(Keys.Escape) && !choosingWild) Main.Switch(Focus.Menu);

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
                        {
                            float read = Settings.defaultWordBoardTimeLimit;
                            try
                            { read = Convert.ToInt32(timeLimitBox.typeText); }
                            catch (FormatException)
                            { read = Settings.defaultWordBoardTimeLimit; }
                            Settings.wordBoardTimeLimit = (int)MathHelper.Clamp(read * 60, 10 * 60, 1000 * 60);
                            timeLimitBox.typeText = (Settings.wordBoardTimeLimit / 60f).ToString();
                            sendStart(); 
                        }

                        timeLimitBox.X = Main.windowWidth - 4;
                        timeLimitBox.CheckSelect();
                        if (!timeLimitBox.clicked)
                        {
                            float read = Settings.defaultWordBoardTimeLimit;
                            try
                            { read = Convert.ToInt32(timeLimitBox.typeText); }
                            catch (FormatException)
                            { read = Settings.defaultWordBoardTimeLimit; }
                            Settings.wordBoardTimeLimit = (int)MathHelper.Clamp(read * 60, 10 * 60, 1000 * 60);
                            timeLimitBox.typeText = (Settings.wordBoardTimeLimit / 60f).ToString();
                        }
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
            {

                GameDraw(); 
            }

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
                case ClientStatus.None:
                    Main.drawText(Main.consoleFont, "A player disconnected, ending the game.", new Vector2(Main.windowWidth / 2, Main.windowHeight / 2), Color.White, 1, TextOrient.Middle);
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
                    if (isOwner) 
                    {
                        startBox.Draw("", Main.sb);
                        timeLimitBox.Draw("", Main.sb); 
                    }
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
                if (Status == ClientStatus.In_Game)
                {
                    Main.sb.DrawString(Main.consoleFont, "Viewing: " + view, new Vector2(4, i), mainColor); i += 16;
                    Main.sb.DrawString(Main.consoleFont, "x: " + wordBoard.selectX, new Vector2(4, i), mainColor); i += 16;
                    Main.sb.DrawString(Main.consoleFont, "y: " + wordBoard.selectY, new Vector2(4, i), mainColor); i += 16;
                    Main.sb.DrawString(Main.consoleFont, "canInteract: " + canInteract, new Vector2(4, i), mainColor); i += 16;
                    Main.sb.DrawString(Main.consoleFont, "direction: " + direction, new Vector2(4, i), mainColor); i += 16;
                    Main.sb.DrawString(Main.consoleFont, "choosing wild: " + choosingWild, new Vector2(4, i), mainColor); i += 16;
                }
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

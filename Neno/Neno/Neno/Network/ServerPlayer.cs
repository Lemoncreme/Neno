using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace Neno
{
    public enum PlayerStatus
    {
        Playing, Lost, Won, Init, Lobby
    }
    public class ServerPlayer
    {
        public string Name = "";
        public PlayerStatus Status = PlayerStatus.Init;
        public byte ID;
        public bool ready = false;
        public NetConnection Connection;
        public int ping = 0;
        public int lastResponse;
        public List<byte> letterTiles = new List<Byte>();
        public bool readyToStart = false;
        public int wordsMade = 0;
        public List<string> words = new List<string>();
        public int coins = 0;

        public ServerPlayer(string name, byte id, NetConnection connection)
        {
            Name = name;
            ID = id;
            Connection = connection;
            for(int i = 0; i < 12; i++)
            {
                letterTiles.Add(Main.randomLetter());
            }
        }
    }
}

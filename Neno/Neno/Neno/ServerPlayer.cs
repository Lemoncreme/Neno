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

        public ServerPlayer(string name, byte id, NetConnection connection)
        {
            Name = name;
            ID = id;
            Connection = connection;
        }
    }
}

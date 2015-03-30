using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public ServerPlayer(string name, byte id)
        {
            Name = name;
            ID = id;
        }
    }
}

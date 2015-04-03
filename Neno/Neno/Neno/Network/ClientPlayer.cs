using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neno
{
    public class ClientPlayer
    {
        public string Name = "";
        public byte ID;
        public bool Ready = false;

        public ClientPlayer(string name, byte id, bool ready)
        {
            Name = name;
            ID = id;
            Ready = ready;
        }
    }
}

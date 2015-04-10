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

namespace Neno
{
    public static class Settings
    {
        public const int defaultPort = 25565; //Default network port
        public const int timeOutPing = 10000; //Amount of ms before timeout
        public const string version = "Version 0.0"; //Game version, displayed in window
        public const int defaultWordBoardTimeLimit = 60 * 45; //Time limit for turns in WordBoard

        public static int wordBoardTimeLimit = 60 * 45; //Time limit for turns in WordBoard
    }
}
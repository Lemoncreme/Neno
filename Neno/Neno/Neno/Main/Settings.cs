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
        public const string version = "Version: Slime 1.0"; //Game version, displayed in window
        public const int defaultWordBoardTimeLimit = 60 * 45; //Time limit for turns in WordBoard
        public const int defaultWordBoardRounds = 4; //Rounds per player for a full wordboard round
        public const int defaultBattleTimeLimit = 60 * 45; //Time limit for turns in WordBoard
        public const int defaultBattleBoardRounds = 4; //Rounds per player per board for a full battleboard round (i.e. 4 with 3 players = 3 boards = 4*3=12 per player total)
        public const bool isDebug = true; //Allows some cheaty stuff

        public static int battleRounds = 4; //Rounds per player per board for a full battleboard round (i.e. 4 with 3 players = 3 boards = 4*3=12 per player total)
        public static int battleTimeLimit = 60 * 45; //Time limit for turns in WordBoard
        public static int wordBoardTimeLimit = 60 * 45; //Time limit for turns in WordBoard
        public static int wordBoardRounds = 4; //Rounds per player for a full wordboard round
    }
}
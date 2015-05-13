using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Neno
{
    public class WordBoard
    {
        public byte[,] tiles = new byte[69, 69];
        /* Tile Type Reference
         * 0 nothing
         * 1-26 words
         * 27 wild
         * 28 Green - Letter x3
         * 29 Blue - Word x2
         * 30 Red - +20 Coins
         * 31 Yellow - +5 tiles
        */
        public int Size = 69;
        public float viewX = 0;
        public float viewY = 0;
        public float Zoom = 1;
        public float movingX = 0;
        public float movingY = 0;
        public float movingMouseX = 0;
        public float movingMouseY = 0;
        public int selectX = -1;
        public int selectY = -1;
        public bool Moving = false;

        public WordBoard()
        {
            //Generate
            Color[] boardImage = new Color[Main.img("Boards/Word").Width * Main.img("Boards/Word").Height];
            Main.img("Boards/Word").GetData<Color>(boardImage);
            for(int x = 0; x < 69; x++)
            {
                for (int y = 0; y < 69; y++)
                {
                    var color = boardImage[(x * 8 + 4) * (y * 8 + 4)];

                    //Green Tiles
                    if (color == new Color(117, 174, 113))
                        tiles[x, y] = 28;

                    //Blue Tiles
                    if (color == new Color(153, 152, 197))
                        tiles[x, y] = 29;

                    //Red Tiles
                    if (color == new Color(185, 145, 145))
                        tiles[x, y] = 30;

                    //Yellow Tiles
                    if (color == new Color(218, 229, 125))
                        tiles[x, y] = 31;
                }
            }

            Console.WriteLine("WordBoard created");
            boardImage = null;
        }

        public bool isEmpty()
        {
            foreach(byte next in tiles)
            {
                if (next >= 1 && next <= 26)
                    return false;
            }
            return true;
        }
    }
}

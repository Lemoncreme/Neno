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
    public class BattleBoard
    {
        int width = 48;
        int height = 96;
        byte[,] tiles; //Tile grid
        List<Entity> entityList = new List<Entity>();
        /* Game Tiles Reference
         * 0 nothing
         * 1-4 rocks
         * 5-8 grass
        */
        byte player1_ID;
        byte player2_ID;

        public BattleBoard(byte p1, byte p2)
        {
            tiles = new byte[width, height];

            //Generate
            for(int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (Main.chance(4))
                        tiles[x, y] = (byte)Main.rInt(5, 8);
                    if (Main.chance(24))
                        tiles[x, y] = (byte)Main.rInt(1, 4);
                }
            }

            //Add players
            entityList.Add(new Entity("Broderick1", 18, 1, 30, p1));
            entityList.Add(new Entity("Broderick2", 22, 1, 30, p1));
            entityList.Add(new Entity("Broderick3", 25, 1, 30, p1));
            entityList.Add(new Entity("Broderick4", 29, 1, 30, p1));
            entityList.Add(new Entity("Anya1", 18, 94, 30, p2));
            entityList.Add(new Entity("Anya2", 22, 94, 30, p2));
            entityList.Add(new Entity("Anya3", 25, 94, 30, p2));
            entityList.Add(new Entity("Anya4", 29, 94, 30, p2));

            Console.WriteLine("BattleBoard generated for " + Main.Server.getPlayer(p1) + " and " + Main.Server.getPlayer(p2));
        }
    }
}

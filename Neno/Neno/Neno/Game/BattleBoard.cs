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
        public int width = 48;
        public int height = 96;
        public byte[] tiles; //Tile grid
        public List<Entity> entityList = new List<Entity>();
        /* Game Tiles Reference
         * 0 nothing
         * 1-4 rocks
         * 5-8 grass
        */
        public byte player1_ID;
        public byte player2_ID;

        public BattleBoard(byte p1, byte p2)
        {
            tiles = new byte[width * height];
            player1_ID = p1;
            player2_ID = p2;

            //Generate
            for(int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (Main.chance(4))
                        setTile(x, y, (byte)Main.rInt(5, 8));
                    if (Main.chance(24))
                        setTile(x, y, (byte)Main.rInt(1, 4));
                }
            }

            //Add players
            entityList.Add(new Entity(NameGen.CreateName(), 18, 1, 30, p1));
            entityList.Add(new Entity(NameGen.CreateName(), 22, 1, 30, p1));
            entityList.Add(new Entity(NameGen.CreateName(), 25, 1, 30, p1));
            entityList.Add(new Entity(NameGen.CreateName(), 29, 1, 30, p1));
            entityList.Add(new Entity(NameGen.CreateName(), 18, 94, 30, p2));
            entityList.Add(new Entity(NameGen.CreateName(), 22, 94, 30, p2));
            entityList.Add(new Entity(NameGen.CreateName(), 25, 94, 30, p2));
            entityList.Add(new Entity(NameGen.CreateName(), 29, 94, 30, p2));

            Console.WriteLine("BattleBoard generated for " + Main.Server.getPlayer(p1).Name + " and " + Main.Server.getPlayer(p2).Name);
        }

        byte getTile(int x, int y)
        {
            return tiles[(y * width) + x];
        }
        void setTile(int x, int y, byte value)
        {
            tiles[(y * width) + x] = value;
        }
    }
}

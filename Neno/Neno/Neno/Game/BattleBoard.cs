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
        public int Width = 48;
        public int Height = 96;
        public byte[] tiles; //Tile grid
        public List<Entity> entityList = new List<Entity>();
        /* Game Tiles Reference
         * 0 nothing
         * 1-4 rocks
         * 5-8 grass
        */
        public byte player1_ID;
        public byte player2_ID;
        public byte otherPlayer;
        public byte turn;
        public byte turnNumber = 0;
        public int time = Settings.battleTimeLimit;
        public List<Point> changeList = new List<Point>(); //List of edited entities for each round (entity ID, property)

        #region Camera Movement
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
        #endregion

        public BattleBoard(byte p1, byte p2)
        {
            tiles = new byte[Width * Height];
            player1_ID = p1;
            player2_ID = p2;
            turn = player1_ID;

            //Generate
            for(int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
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
        public BattleBoard()
        {
            Console.WriteLine("BattleBoard loaded");
        }

        public byte getTile(int x, int y)
        {
            return tiles[(y * Width) + x];
        }
        public void setTile(int x, int y, byte value)
        {
            tiles[(y * Width) + x] = value;
        }
        public Entity findEntity(int x, int y, EntityType type)
        {
            foreach(Entity ent in entityList)
            {
                if (ent.Type == type && ent.Prop(PropType.X) == x && ent.Prop(PropType.Y) == y)
                {
                    return ent;
                }
            }
            return null;
        }
        public Entity findEntity(int ID)
        {
            foreach (Entity ent in entityList)
            {
                if (ent.ID == ID)
                {
                    return ent;
                }
            }
            return null;
        }
    }
}

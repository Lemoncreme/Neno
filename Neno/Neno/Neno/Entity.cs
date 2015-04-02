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
    enum EntityType
    {
        person, //Interactable characters controlled by players
        creature, //NPC
        inventory //Bags, chests, single dropped items, etc.
    }
    public class Entity
    {
        //Describes any moving, updating entity
        string Name;
        int X;
        int Y;
        int HP;
        byte ownerID;

        public Entity(string name, int x, int y, int hp, byte ownerid)
        {
            Name = name;
            X = x;
            Y = y;
            HP = hp;
            ownerID = ownerid;
            Console.WriteLine("Entity created; name = " + Name + " location = " + x + "," + y + " owner = " + Main.Server.getPlayer(ownerID).Name);
        }
    }
}

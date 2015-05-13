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
    public enum EntityType
    {
        person, //Interactable characters controlled by players
        creature, //NPC
        inventory //Bags, chests, single dropped items, etc.
    }
    public class Entity
    {
        //Describes any moving, updating entity
        public string Name;
        public int X;
        public int Y;
        public int MaxHP;
        public int HP;
        public byte ownerID;
        public EntityType Type;

        #region player type
        public Color hairColor;
        public Color skinColor;
        public int MaxStamina;
        public int Stamina;
        #endregion

        public Entity(string name, int x, int y, int hp, byte ownerid, EntityType Type = EntityType.person)
        {
            Name = name;
            X = x;
            Y = y;
            MaxHP = hp;
            HP = hp;
            MaxStamina = (int)(0.1f + ((float)MaxHP) * Main.rFloat(0.6f));
            Stamina = MaxStamina;
            ownerID = ownerid;
            int mix = Main.rInt(-8, 15);
            skinColor = Main.choose<Color>(new List<Color>(){
                new Color(Main.rInt(219, 255), Main.rInt(180, 220), Main.rInt(60, 100)),
                new Color(Main.rInt(90, 142) + mix, Math.Abs(mix), Main.rInt(8, 72) + mix)
            });
            mix = Main.rInt(-8, 15);
            hairColor = Main.choose<Color>(new List<Color>(){
                new Color(9 + mix, 8 + mix, 6 + mix),
                new Color(Main.rInt(240, 255), 245 + mix, Main.rInt(210, 225)),
                new Color(59 + mix, 48 + mix, 36 + mix),
                new Color(183 + mix, 166 + mix, 158 + mix),
                new Color(184 + mix, 151 + mix, 120 + mix),
                new Color(106 + mix, 78 + mix, 66 + mix)
            });
            if (Main.chance(10))
                hairColor = new Color(Main.rInt(50, 200), Main.rInt(50, 200), Main.rInt(50, 200));
            Console.WriteLine("Entity created; name = " + Name + " location = " + x + "," + y + " owner = " + ownerID);
        }
        public Entity(string name, byte[] packed)
        {
            Name = name;
            Unpack(packed);

            Console.WriteLine("Entity unpacked; name = " + Name + " location = " + X + "," + Y + " owner = " + ownerID);
        }

        public byte[] Pack()
        {
            return new byte[]{
            (byte)X, (byte)Y, (byte)HP, ownerID, (byte)Type, 
            hairColor.R, hairColor.G, hairColor.B,
            skinColor.R, skinColor.G, skinColor.B,
            (byte)MaxHP, (byte)MaxStamina, (byte)Stamina
            };
        }
        public void Unpack(byte[] packed)
        {
            X = (int)packed[0];
            Y = (int)packed[1];
            HP = (int)packed[2];
            ownerID = packed[3];
            Type = (EntityType)packed[4];
            hairColor = new Color(packed[5], packed[6], packed[7]);
            skinColor = new Color(packed[8], packed[9], packed[10]);
            MaxHP = (int)packed[11];
            MaxStamina = (int)packed[12];
            Stamina = (int)packed[13];
        }

    }
}

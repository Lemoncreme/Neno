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
    public enum PropType
    {
        X, Y, MaxHp, Hp, MaxStamina, Stamina, Owner,
        HairR, HairG, HairB,
        SkinR, SkinG, SkinB,
        EyeR, EyeG, EyeB,
        ShirtR, ShirtG, ShirtB,
        PantsR, PantsG, PantsB
    }
    public class EntProp
    {
        public PropType Type;
        public int Value;

        public EntProp(PropType type, int value)
        {
            Type = type; Value = value;
        }
    }
    public class Entity
    {
        //Describes any moving, updating entity
        public string Name;
        public EntityType Type;
        public int ID;

        public List<EntProp> propList = new List<EntProp>();

        public Entity(string name, int x, int y, int hp, byte ownerid, EntityType Type = EntityType.person)
        {
            ID = GameServer.entityIDinc; GameServer.entityIDinc++;
            Name = name;
            Type = EntityType.person;

            propList.Add(new EntProp(PropType.X, x));
            propList.Add(new EntProp(PropType.Y, y));
            propList.Add(new EntProp(PropType.MaxHp, hp));
            propList.Add(new EntProp(PropType.Hp, hp));
            var val = (int)(6f + ((float)hp) * Main.rFloat(0.4f));
            propList.Add(new EntProp(PropType.MaxStamina, val));
            propList.Add(new EntProp(PropType.Stamina, val));
            propList.Add(new EntProp(PropType.Owner, ownerid));

            #region Colors

            //Skin
            int mix = Main.rInt(-8, 15);
            Color color = Main.choose<Color>(new List<Color>(){
                new Color(Main.rInt(219, 255) + mix, Main.rInt(180, 220) + mix, Main.rInt(60, 100) + mix),
                new Color(72 + Main.rInt(-8, 8), Main.rInt(20, 60), Main.rInt(32, 50))
            });
            propList.Add(new EntProp(PropType.SkinR, color.R));
            propList.Add(new EntProp(PropType.SkinG, color.G));
            propList.Add(new EntProp(PropType.SkinB, color.B));

            //Hair
            mix = Main.rInt(-8, 15);
            color = Main.choose<Color>(new List<Color>(){
                new Color(9 + mix, 8 + mix, 6 + mix),
                new Color(Main.rInt(240, 255), 245 + mix, Main.rInt(210, 225)),
                new Color(59 + mix, 48 + mix, 36 + mix),
                new Color(183 + mix, 166 + mix, 158 + mix),
                new Color(184 + mix, 151 + mix, 120 + mix),
                new Color(106 + mix, 78 + mix, 66 + mix)
            });
            if (Main.chance(10))
                color = new Color(Main.rInt(50, 200), Main.rInt(50, 200), Main.rInt(50, 200));
            propList.Add(new EntProp(PropType.HairR, color.R));
            propList.Add(new EntProp(PropType.HairG, color.G));
            propList.Add(new EntProp(PropType.HairB, color.B));

            //Eyes
            color = Main.choose<Color>(new List<Color>(){
                Color.PowderBlue,
                Color.AliceBlue,
                Color.CornflowerBlue,
                Color.SandyBrown,
                Color.Brown,
                Color.RosyBrown,
                Color.SaddleBrown,
                Color.GreenYellow,
                Color.SeaGreen,
                Color.LightSeaGreen
            });
            if (Main.chance(65))
                color = Color.Black;
            propList.Add(new EntProp(PropType.EyeR, color.R));
            propList.Add(new EntProp(PropType.EyeG, color.G));
            propList.Add(new EntProp(PropType.EyeB, color.B));

            //Shirt
            color = new Color(Main.rInt(50, 200), Main.rInt(50, 200), Main.rInt(50, 200));
            propList.Add(new EntProp(PropType.ShirtR, color.R));
            propList.Add(new EntProp(PropType.ShirtG, color.G));
            propList.Add(new EntProp(PropType.ShirtB, color.B));

            //Pants
            mix = Main.rInt(-50, 50);
            color = new Color((int)(color.R + mix * Main.rFloat(0.5f)), (int)(color.G + mix * Main.rFloat(0.5f)), (int)(color.B + mix * Main.rFloat(0.5f)));
            propList.Add(new EntProp(PropType.PantsR, color.R));
            propList.Add(new EntProp(PropType.PantsG, color.G));
            propList.Add(new EntProp(PropType.PantsB, color.B));

            #endregion

            Console.WriteLine("Entity created; name = " + Name + " location = " + x + "," + y + " owner = " + ownerid);
        }
        public Entity(string name, byte[] packed, int id, EntityType type)
        {
            Name = name;
            Unpack(packed);
            ID = id;
            Type = type;

            Console.WriteLine("Entity unpacked; name = " + Name + " location = " + Prop(PropType.X) + "," + Prop(PropType.Y) + " owner = " + Prop(PropType.Owner));
        }

        public byte[] Pack()
        {
            byte[] array = new byte[propList.Count * 2 + 1]; int i = 1;
            array[0] = (byte)(propList.Count * 2 + 1);
            foreach(EntProp prop in propList)
            {
                array[i] = (byte)prop.Type;
                array[i + 1] = (byte)prop.Value;
                i += 2;
            }
            return array;
        }
        public void Unpack(byte[] packed)
        {
            propList.Clear();
            int count = packed[0];

            for(int i = 1; i < count; i += 2)
            {
                propList.Add(new EntProp((PropType)packed[i], packed[i + 1]));
            }
        }

        public int Prop(PropType type)
        {
            foreach(EntProp prop in propList)
            {
                if (type == prop.Type)
                    return prop.Value;
            }
            return -1;
        }

        public Color getColor(PropType R, PropType G, PropType B)
        {
            return new Color(Prop(R), Prop(G), Prop(B));
        }

        public void EditProp(PropType type, int newValue)
        {
            foreach (EntProp prop in propList)
            {
                if (type == prop.Type)
                    prop.Value = newValue;
            }
        }

        public Vector2 getPos()
        {
            return new Vector2(Prop(PropType.X), Prop(PropType.Y));
        }
    }
}

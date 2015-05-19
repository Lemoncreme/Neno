using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Neno
{
    public enum ItemType
    {
        melee, ranged, consumable, equip
    }
    public enum ItemSubtype
    {
        sword, axe, bow, crossbow, food, potion, armor, trinket
    }
    public class ItemProp
    {
        public PropType Type;
        public int Value;

        public ItemProp(PropType type, int value)
        {
            Type = type; Value = value;
        }
    }
    public class Item
    {
        public int ID;
        public string Name; //Only used in random gen
        public List<ItemProp> propList = new List<ItemProp>();
        public ItemType Type;
        public ItemSubtype SubType;
        public int Frame = 0; //Image frame

        public static List<Item> itemList = new List<Item>();

        public Item(int id)
        {
            ID = id;
        }
        public static Item RandomGen(string nameStart)
        {
            Item item = new Item(0);

            //Basic Properties
            item.propList.Add(new ItemProp(PropType.Hp, 10 + Main.rgInt(220)));
            item.propList.Add(new ItemProp(PropType.MaxHp, 15 + Main.rgInt(220)));
            item.Type = ItemType.melee;
            item.SubType = ItemSubtype.sword;

            #region Name
            item.Name = nameStart;

            //Adjective (30% chance)
            if (Main.chance(30f))
                item.Name += " " + Main.choose<string>(new List<string>() { "Damaged", "Sharpened", "Hardened", "Twice-folded", "Thrice-folded", "Broken", "Greasy",
            "Sinister-looking", "Moldy", "Standard", "Rare", "Blessed", "Hammer-hardened", "Very Exquisite", "Decorated", "Molded", "Skillfully Crafted"});
            else
                item.Name += " Common";

            //Material
            #region Materials
            item.Name += " " + Main.chooseG<string>(new List<string>() {
                "Wooden",
                "Flint", 
                "Quartz", 
                "Rose Quartz", 
                "Pewter", 
                "Plastic", 
                "Tin",
                "Copper", 
                "Bronze", 
                "Brass", 
                "Solder", 
                "Rose Metal", 
                "Iron", 
                "Lead", 
                "Pig Iron", 
                "Bismuth", 
                "Wrought Iron", 
                "Nickel", 
                "Jade", 
                "Tungsten", 
                "Silver", 
                "Obsidian", 
                "Invar", 
                "Gold", 
                "Cerrosafe", 
                "Aluminum", 
                "White Gold", 
                "Rose Gold", 
                "Steel", 
                "Hepitazon", 
                "Prince's Metal", 
                "Stainless Steel",
                "Titanium", 
                "Solarium", 
                "Damascus Steel", 
                "Diamond", 
                "Duralumin", 
                "Chrome", 
                "Orichalcum", 
                "Molybdochalkos", 
                "Vitallium", 
                "Platinum", 
                "Promethium", 
                "Gilt Bronze", 
                "Iridium", 
                "Soulbound Steel", 
                "Borazon", 
                "Dilithium", 
                "Sky Iron", 
                "Phazon", 
                "Mythril",
                "Adamantium", 
                "Supernova Metal", 
                "Awesomium"});
            #endregion

            //Blade type
            if (item.SubType == ItemSubtype.sword)
                item.Name += " " + Main.choose<string>(new List<string>() { "Broadsword", "Shortsword", "Longsword", "Sword", "Greatsword", "Rapier", "Cutlass",
            "Zweihander", "Smallsword", "Knife", "Katana", "Blade", "Foil", "Hunting Sword", "Balisword", "Dagger", "Swiss Dagger", "Side-sword", "Claymore"});
            #endregion

            return item;
        }

        public byte[] Pack()
        {
            byte[] array = new byte[propList.Count * 2 + 1]; int i = 1;
            array[0] = (byte)(propList.Count * 2 + 1);
            foreach (ItemProp prop in propList)
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

            for (int i = 1; i < count; i += 2)
            {
                propList.Add(new ItemProp((PropType)packed[i], packed[i + 1]));
            }
        }

        public static Item Load(string String)
        {
            Item item = new Item(0);

            //Open
            var stream = File.OpenText("./ToAdd/" + String + ".txt");
            bool define = false;
            string line;

            //Line 1 should be "NENO"
            line = stream.ReadLine();
            if (line != "NENO")
            {
                Console.WriteLine("<ERROR> Item file in wrong format");
                return null;
            }

            //Line 2 should be "ITEM_DEFINITION" or "ITEM_LIST"
            line = stream.ReadLine();
            if (line == "ITEM_DEFINITION")
            {
                Console.WriteLine("<DEBUG> Item definition");
                define = true;
            }

            //Tags and values
            while(!stream.EndOfStream)
            {
                var next = stream.ReadLine().Split(new char[]{':'});
                string tag = next[0];
                string value = next[1];
                string value2 = "0";
                if (tag == "prop")
                value2 = next[2];
                switch(tag)
                {
                    case "name":
                        item.Name = value;
                        break;
                    case "id":
                        item.ID = Convert.ToInt32(value);
                        break;
                    case "img":
                        if (value != "img")
                        item.Frame = Convert.ToInt32(value);
                        break;
                    case "type":
                        item.Type = (ItemType)Enum.Parse(typeof(ItemType), value);
                        break;
                    case "subtype":
                        item.SubType = (ItemSubtype)Enum.Parse(typeof(ItemSubtype), value);
                        break;
                    case "prop":
                        item.propList.Add(new ItemProp((PropType)Enum.Parse(typeof(PropType), value), Convert.ToInt32(value2)));
                        break;
                }
            }

            //Close
            stream.Close();
            stream.Dispose();

            //Add new definition
            if (define)
                WriteNewDefinition(item);

            return item;
        }
        public static void WriteNewDefinition(Item item)
        {
            int number = 1;
            //TODO: make this find a workable file
            while(File.OpenText(Main.itemDirectory + "items" + number + ".txt").ReadLine())
            {
                number++;
            }
            var stream = File.Open(Main.itemDirectory + "items" + number + ".txt");
        }
    }
}

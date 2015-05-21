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
        melee, ranged, consumable, equip, entity
    }
    public enum ItemSubtype
    {
        sword, axe, bow, crossbow, food, potion, armor, trinket,
        edit, place, bludgeon, drink
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
        public bool isEquipped = false;
        public int equippedBy = 0;

        public static List<Item> itemList = new List<Item>();

        public Item(int id)
        {
            ID = id;
        }
        public Item(string name, ItemType type, ItemSubtype subtype, byte[] packed)
        {
            Name = name;
            Type = type;
            SubType = subtype;
            Unpack(packed);
        }
        public static Item RandomGen(string nameStart)
        {
            Item item = new Item(0);

            //Basic Properties
            item.propList.Add(new ItemProp(PropType.Hp, 3));
            item.propList.Add(new ItemProp(PropType.MaxHp, 6));
            item.propList.Add(new ItemProp(PropType.Weight, Main.rgInt(17)));
            item.Type = ItemType.melee;
            item.SubType = Main.choose<ItemSubtype>(ItemSubtype.sword, ItemSubtype.axe);
            if (item.SubType == ItemSubtype.sword) item.Frame = Main.choose<int>(1, 10, 33, 34, 35);
            if (item.SubType == ItemSubtype.axe) item.Frame = Main.choose<int>(2, 14, 32);

            //Values
            item.Name = nameStart;
            KeyValuePair<string, ItemProp> effect;
            int materialQuality = 1;

            #region Attributes

            #region Quality
            if (Main.chance(30f))
                effect = Main.choose<KeyValuePair<string, ItemProp>>(
                    new KeyValuePair<string, ItemProp>("Damaged", new ItemProp(PropType.Hp, -5)), 
                    new KeyValuePair<string, ItemProp>("Sharpened", new ItemProp(PropType.DmgSharp, 2)), 
                    new KeyValuePair<string, ItemProp>("Hardened", new ItemProp(PropType.Hp, 10)), 
                    new KeyValuePair<string, ItemProp>("Twice-folded", new ItemProp(PropType.Hp, 15)), 
                    new KeyValuePair<string, ItemProp>("Thrice-folded", new ItemProp(PropType.Hp, 20)), 
                    new KeyValuePair<string, ItemProp>("Broken", new ItemProp(PropType.Hp, -20)), 
                    new KeyValuePair<string, ItemProp>("Greasy", null),
                    new KeyValuePair<string, ItemProp>("Sinister-looking", new ItemProp(PropType.DmgMax, 3)), 
                    new KeyValuePair<string, ItemProp>("Moldy", null), 
                    new KeyValuePair<string, ItemProp>("Standard", null), 
                    new KeyValuePair<string, ItemProp>("Rare", new ItemProp(PropType.Value, 25)), 
                    new KeyValuePair<string, ItemProp>("Blessed", new ItemProp(PropType.Weight, -1)), 
                    new KeyValuePair<string, ItemProp>("Hammer-hardened", new ItemProp(PropType.Hp, 12)), 
                    new KeyValuePair<string, ItemProp>("Very Exquisite", new ItemProp(PropType.Value, 30)), 
                    new KeyValuePair<string, ItemProp>("Decorated", new ItemProp(PropType.Value, 15)), 
                    new KeyValuePair<string, ItemProp>("Molded", new ItemProp(PropType.Value, 10)),
                    new KeyValuePair<string, ItemProp>("Skillfully Crafted", new ItemProp(PropType.Value, 12)));
            else
                effect = new KeyValuePair<string, ItemProp>("Common", null);
            item.Name += effect.Key;
            item.AddProp(effect.Value.Type, effect.Value.Value);
            #endregion

            #region Materials
            var material = Main.chooseG<KeyValuePair<string, int>>(
                new KeyValuePair<string, int>("Wooden", 1),
                new KeyValuePair<string, int>("Flint", 2),
                new KeyValuePair<string, int>("Quartz", 2),
                new KeyValuePair<string, int>("Rose Quartz", 2),
                new KeyValuePair<string, int>("Pewter", 3),
                new KeyValuePair<string, int>("Plastic", 3),
                new KeyValuePair<string, int>("Tin", 3),
                new KeyValuePair<string, int>("Copper", 4), 
                new KeyValuePair<string, int>("Bronze", 4), 
                new KeyValuePair<string, int>("Brass", 4), 
                new KeyValuePair<string, int>("Solder", 4), 
                new KeyValuePair<string, int>("Rose Metal", 5), 
                new KeyValuePair<string, int>("Iron", 5), 
                new KeyValuePair<string, int>("Lead", 5), 
                new KeyValuePair<string, int>("Pig Iron", 5), 
                new KeyValuePair<string, int>("Bismuth", 5), 
                new KeyValuePair<string, int>("Wrought Iron", 5), 
                new KeyValuePair<string, int>("Nickel", 6), 
                new KeyValuePair<string, int>("Jade", 6), 
                new KeyValuePair<string, int>("Tungsten", 7), 
                new KeyValuePair<string, int>("Silver", 7), 
                new KeyValuePair<string, int>("Obsidian", 7), 
                new KeyValuePair<string, int>("Invar", 7), 
                new KeyValuePair<string, int>("Gold", 8), 
                new KeyValuePair<string, int>("Cerrosafe", 8), 
                new KeyValuePair<string, int>("Aluminum", 8), 
                new KeyValuePair<string, int>("White Gold", 9), 
                new KeyValuePair<string, int>("Rose Gold", 9), 
                new KeyValuePair<string, int>("Steel", 10), 
                new KeyValuePair<string, int>("Hepitazon", 10), 
                new KeyValuePair<string, int>("Prince's Metal", 11), 
                new KeyValuePair<string, int>("Stainless Steel", 10),
                new KeyValuePair<string, int>("Titanium", 12), 
                new KeyValuePair<string, int>("Solarium", 13), 
                new KeyValuePair<string, int>("Damascus Steel", 13), 
                new KeyValuePair<string, int>("Diamond", 16), 
                new KeyValuePair<string, int>("Duralumin", 14), 
                new KeyValuePair<string, int>("Chrome", 14), 
                new KeyValuePair<string, int>("Orichalcum", 14), 
                new KeyValuePair<string, int>("Molybdochalkos", 14), 
                new KeyValuePair<string, int>("Vitallium", 15), 
                new KeyValuePair<string, int>("Platinum", 16), 
                new KeyValuePair<string, int>("Promethium", 17), 
                new KeyValuePair<string, int>("Gilt Bronze", 5), 
                new KeyValuePair<string, int>("Iridium", 18), 
                new KeyValuePair<string, int>("Soulbound Steel", 19), 
                new KeyValuePair<string, int>("Borazon", 21), 
                new KeyValuePair<string, int>("Dilithium", 23), 
                new KeyValuePair<string, int>("Sky Iron", 26), 
                new KeyValuePair<string, int>("Phazon", 30), 
                new KeyValuePair<string, int>("Mythril", 40),
                new KeyValuePair<string, int>("Adamantium", 50), 
                new KeyValuePair<string, int>("Supernova Metal", 75),
                new KeyValuePair<string, int>("Awesomium", 99));
            item.Name += material.Key;
            materialQuality = material.Value;
            #endregion

            #region Blade type
            KeyValuePair<string, int> blade;
            if (item.SubType == ItemSubtype.sword)
                blade = Main.choose<KeyValuePair<string, int>>(
                    new KeyValuePair<string, int>("Broadsword", 4), 
                    new KeyValuePair<string, int>("Shortsword", 2), 
                    new KeyValuePair<string, int>("Longsword", 4), 
                    new KeyValuePair<string, int>("Sword", 3), 
                    new KeyValuePair<string, int>("Greatsword", 5), 
                    new KeyValuePair<string, int>("Rapier", 1), 
                    new KeyValuePair<string, int>("Cutlass", 2),
                    new KeyValuePair<string, int>("Zweihander", 5), 
                    new KeyValuePair<string, int>("Smallsword", 2), 
                    new KeyValuePair<string, int>("Knife", 1), 
                    new KeyValuePair<string, int>("Katana", 3), 
                    new KeyValuePair<string, int>("Blade", 3), 
                    new KeyValuePair<string, int>("Foil", 1), 
                    new KeyValuePair<string, int>("Hunting Sword", 3), 
                    new KeyValuePair<string, int>("Balisword", 2), 
                    new KeyValuePair<string, int>("Dagger", 1), 
                    new KeyValuePair<string, int>("Swiss Dagger", 1), 
                    new KeyValuePair<string, int>("Side-sword", 2), 
                    new KeyValuePair<string, int>("Claymore", 4));
            if (item.SubType == ItemSubtype.axe)
                blade = Main.choose<KeyValuePair<string, int>>(
                    new KeyValuePair<string, int>("Axe", 3), 
                    new KeyValuePair<string, int>("Greataxe", 5), 
                    new KeyValuePair<string, int>("Waraxe", 4), 
                    new KeyValuePair<string, int>("Double-sided Axe", 4), 
                    new KeyValuePair<string, int>("Ax", 3), 
                    new KeyValuePair<string, int>("Hatchet", 2),
                    new KeyValuePair<string, int>("Fireaxe", 2));
            item.Name += blade.Key;
            item.AddProp(PropType.Weight, blade.Value);
            #endregion

            #endregion

            int dmg = 1 + Main.rgInt(10);
            int dmg2 = dmg + 1 + Main.rgInt(10);
            item.propList.Add(new ItemProp(PropType.DmgMin, dmg));
            item.propList.Add(new ItemProp(PropType.DmgMax, dmg2));
            item.propList.Add(new ItemProp(PropType.DmgCap, dmg2 + 1 + Main.rgInt(10)));
            item.propList.Add(new ItemProp(PropType.DmgSharp, Main.rgInt(10)));
            item.propList.Add(new ItemProp(PropType.DmgBlunt, Main.rgInt(10)));
            if (Main.chance(30f))
            item.propList.Add(new ItemProp(PropType.DmgMagic, Main.rgInt(10)));
            item.propList.Add(new ItemProp(PropType.Value, dmg + dmg2 + Main.rgInt(20)));

            return item;
        }

        public int Prop(PropType type)
        {
            foreach (ItemProp prop in propList)
            {
                if (type == prop.Type)
                    return prop.Value;
            }
            return -1;
        }
        public void EditProp(PropType type, int newValue)
        {
            foreach (ItemProp prop in propList)
            {
                if (type == prop.Type)
                { 
                    prop.Value = (int)MathHelper.Clamp(newValue, 0, 255);
                    return;
                }
            }
        }
        public void AddProp(PropType type, int add)
        {
            EditProp(type, Prop(type) + add);
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

        public static void LoadAll()
        {
            var files = Directory.EnumerateFiles("./ToAdd/", "*.txt");
            foreach(string file in files)
            {
                Load(file.Replace("./ToAdd/", "").Replace(".txt", ""));
            }
        }
        public static Item Load(string String)
        {
            Item item = new Item(0);

            //Open
            var stream = File.OpenText("./ToAdd/" + String + ".txt");
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
            if (line != "ITEM_DEFINITION")
            {
                Console.WriteLine("<ERROR> Item file in wrong format");
                return null;
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
            WriteNewDefinition(item);

            //Stitch
            //TODO: Stitch new images to spritesheet

            return item;
        }
        public static void WriteNewDefinition(Item item)
        {
            //Define
            int number = 1;
            bool keepFinding = true;
            string path;

            //Find usable item list file
            while(keepFinding)
            {
                path = Main.itemDirectory + "items" + number + ".txt";
                if (File.Exists(path))
                {
                    var stream = File.ReadAllLines(path);
                    if (stream[0] == "NENO")
                    {
                        if (stream[1] == "ITEM_LIST")
                        {
                            var countLine = stream[2];
                            if (countLine.Contains("terms"))
                            {
                                int count = Convert.ToInt32(countLine.Split(new char[] { ':' })[1]);
                                if (count < 500)
                                    keepFinding = false;
                            }
                        }
                    }
                    stream = null;
                }
                else
                    keepFinding = false;
                if (keepFinding)
                    number++;
            }

            //Create file if it doesnt exist
            path = Main.itemDirectory + "items" + number + ".txt";
            if (!File.Exists(path))
            {
                File.WriteAllLines(path, new string[] { "NENO", "ITEM_LIST", "terms:0" });
            }

            //Get terms
            string[] lines = File.ReadAllLines(path);
            int terms = Convert.ToInt32(lines[2].Split(new char[] { ':' })[1]);
            int index = terms;
            Console.WriteLine("<DEBUG> {0} terms", terms);
            foreach(string line in lines)
            {
                if (line.Contains("name:" + item.Name))
                {
                    Console.WriteLine("<ERROR> Item {0} already defined", item.Name);
                    return; 
                }
            }

            //Write new
            StreamWriter write; write = File.AppendText(path);
            write.WriteLine("term_{0}:name:{1}", terms, item.Name);
            write.WriteLine("term_{0}:img:{1}", terms, item.Frame);
            write.WriteLine("term_{0}:id:{1}", terms, terms + 1);
            write.WriteLine("term_{0}:type:{1}", terms, item.Type.ToString());
            write.WriteLine("term_{0}:subtype:{1}", terms, item.SubType.ToString());
            foreach (ItemProp prop in item.propList)
            {
                write.WriteLine("term_{0}:{1}:{2}", terms, prop.Type.ToString(), prop.Value.ToString()); 
            }
            write.Close();

            //Edit terms
            lines[2] = "terms:" + (terms + 1);
            File.WriteAllLines(path, lines);
        }
    }
}

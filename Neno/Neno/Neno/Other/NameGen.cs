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
    public static class NameGen
    {

        public static string CreateName()
        {
            StringBuilder name = new StringBuilder();
            bool firstIsVowel = false;
            int range = 0;
            if (Main.chance(2)) range = 1;
            if (Main.chance(7)) range = 2;
            if (Main.chance(16)) range = 3;

            //First letter
            if (Main.chance(1))
            {
                if (Main.chance(5))
                    name.Insert(name.Length, NameGen.Consanants()); 
                else
                    name.Insert(name.Length, NameGen.Consanant()); 
            }
            else
            {
                if (Main.chance(5))
                    name.Insert(name.Length, NameGen.Vowels());
                else
                    name.Insert(name.Length, NameGen.Vowel());
                firstIsVowel = true;
            }
            name[0] = Convert.ToChar(Convert.ToString(name[0]).ToUpper());

            //Next letters
            if (firstIsVowel)
                for (int i = 0; i < Main.rInt(1, range); i++)
                {
                    if (Main.chance(5))
                        name.Insert(name.Length, NameGen.Consanants());
                    else
                        name.Insert(name.Length, NameGen.Consanant()); 

                    if (Main.chance(5))
                        name.Insert(name.Length, NameGen.Vowels());
                    else
                        name.Insert(name.Length, NameGen.Vowel()); 
                }
            else
                for (int i = 0; i < Main.rInt(1, range); i++)
                {
                    if (Main.chance(5))
                        name.Insert(name.Length, NameGen.Vowels());
                    else
                        name.Insert(name.Length, NameGen.Vowel());

                    if (Main.chance(5))
                        name.Insert(name.Length, NameGen.Consanants());
                    else
                        name.Insert(name.Length, NameGen.Consanant());
                }

            //Last letter
            if (firstIsVowel)
                name.Insert(name.Length, NameGen.Consanant()); 
            else
            {
                if (Main.chance(5))
                    name.Insert(name.Length, NameGen.Vowels());
                else
                    name.Insert(name.Length, NameGen.Vowel()); 
            }


            return name.ToString();
        }

        private static string Consanant()
        {
            return Main.choose<string>(new List<string>() { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y", "z" });
        }
        private static string Vowel()
        {
            return Main.choose<string>(new List<string>() { "a", "e", "i", "o", "u" });
        }
        private static string Vowels()
        {
            return Main.choose<string>(new List<string>() { "ee", "oo", "ae", "io", "oi", "uo", "ou", "ue", "ei", "ie", "ai", "ia", "ay", "y", "oyo", "oyu", "oe", "eo" });
        }
        private static string Consanants()
        {
            return Main.choose<string>(new List<string>() { "th", "sh", "bh", "rh", "ph", "gh", "kh", "yv", "sw", "spl", "thr", "gr", "tr", "pr", "scr", "tw", "kw", "fr",
                                                            "br", "sph", "sn", "dr", "fl" });
        }
    }
}

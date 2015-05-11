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
    public static class Sound
    {
        private static Dictionary<string, SoundEffectInstance> instanceList = new Dictionary<string, SoundEffectInstance>();

        public static void Play(string name)
        {
            Main.sound(name).Play();
        }
        public static void Loop(string name)
        {
            var next = Main.sound(name).CreateInstance();
            next.IsLooped = true;
            next.Play();
            instanceList.Add(name, next);
        }
        public static void Stop(string name)
        {
            if (instanceList.ContainsKey(name))
            { 
                instanceList[name].Stop();
                instanceList.Remove(name);
            }
        }
        public static void StopAll()
        {
            foreach(KeyValuePair<string, SoundEffectInstance> entry in instanceList)
            {
                entry.Value.Stop();
            }
            instanceList.Clear();
        }
    }
}

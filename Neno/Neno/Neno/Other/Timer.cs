using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Neno
{
    public class Timer
    {
        public int Time;
        int maxTime;
        public bool tick = false;
        public bool running = true;
        bool DoLoop;
        private static List<Timer> Timers = new List<Timer>();

        public Timer(int time, bool doLoop)
        {
            Time = time;
            maxTime = time;
            DoLoop = doLoop;
            Timers.Add(this);
        }
        public Timer(int time, bool doLoop, int startTime)
        {
            Time = (int)MathHelper.Clamp(startTime, 1, time);
            maxTime = time;
            DoLoop = doLoop;
            Timers.Add(this);
        }

        private void doTick()
        {
            tick = false;
            Time--;
            if (Time <= 0)
            {
                tick = true;
                if (DoLoop)
                {
                    Time = maxTime;
                }
                else
                {
                    Timers.Remove(this);
                    running = false;
                }
            }
        }

        public void Stop()
        {
            Timers.Remove(this);
            tick = false;
        }

        public static void Update()
        {
            for(int i = 0; i < Timers.Count; i ++)
            {
                Timers[i].doTick();
            }
        }

        public static void stopAll()
        {
            Timers = new List<Timer>();
        }
    }
}

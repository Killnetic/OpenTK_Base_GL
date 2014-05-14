using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TK_TestBed.Engine
{
    public static class FPSCounter
    {
        private static float delta = 0f;
        private static double frameRate = 0;
        private static double[] frames = new double[30];
        private static uint fIndex = 0;
        private static DateTime deltaTime = DateTime.Now;
        public static void Update()
        {
            delta = (float)(DateTime.Now - deltaTime).TotalSeconds;

            ++fIndex;
            if (fIndex >= frames.Length)
                fIndex = 0;
            frames[fIndex] = delta;

            deltaTime = DateTime.Now;
        }
        public static double GetFPS()
        {
            // calc fps
            //
            frameRate = 0;
            for (int i = 0; i < frames.Length; i++)
            {
                frameRate += frames[i];
            }
            frameRate /= frames.Length;
            frameRate = 1 / frameRate;
            return frameRate;
        }
    }
}

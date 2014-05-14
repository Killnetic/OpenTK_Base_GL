using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;

namespace TK_TestBed
{
    class Program
    {
        static void Main(string[] args)
        {
            ////
            // TODO: Check for valid color sets
            //
            using (Game game = new Game(1280, 720, GraphicsMode.Default, "(TK) Testbed"))
            {
                // update rate, fps rate
                //game.Icon = 
                //game.CursorVisible = false;
                game.VSync = VSyncMode.Adaptive;
                game.Run(60);
                Console.WriteLine("\r\nGamewindow Exit   ");
            }
        }
    }
}

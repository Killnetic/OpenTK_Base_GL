using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using TK_TestBed.Engine;
using TK_TestBed.Engine.Scene;

namespace TK_TestBed
{
    /// <summary>
    /// Dispatches 
    /// </summary>
    public class Game : GameWindow
    {
        private Keybinder keyBinder;
        private Core core;
        public Game(int width, int height, GraphicsMode m, string title) : base(width, height, m, title)
        {
            VSync = VSyncMode.Adaptive;
            core = new Core();
        }
        #region OpenTK event overrides
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // opengl rendering specific updates ONLY please
            //
            FPSCounter.Update();

            core.RenderFrame();

            
            SwapBuffers();


            base.OnRenderFrame(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // game simulation updates
            //

            // debug output
            //
            Console.Clear();
            Console.WriteLine("{0} fps", Math.Round(FPSCounter.GetFPS(), 1).ToString("00000.000"));
            Console.WriteLine("[V]sync: {0}", VSync);
            Console.WriteLine("[Q]uit");
            Console.WriteLine("[Alt+Enter] Toggle Windowed Fullscreen");

            base.OnUpdateFrame(e);
        }

        protected override void OnResize(EventArgs e)
        {
            // rebuild viewport, projection matrix
            //
            core.UpdateScreenSize(ClientRectangle.Width, ClientRectangle.Height);

            base.OnResize(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            // initialize minimal necessary GPU resources
            //
            WindowBorder = WindowBorder.Fixed;

            keyBinder = new Keybinder(Keyboard);

            keyBinder.SubscribeListener(Key.V, VsyncToggle);
            keyBinder.SubscribeListener(Key.Q, Quit);
            keyBinder.SubscribeListener(Key.F4, Quit);
            keyBinder.SubscribeListener(Key.Enter, ToggleWindowedFullscreen);

            base.OnLoad(e);
        }
        
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // free all GPU resources
            //
            core.FreeAll();

            base.OnClosing(e);
        }
        #endregion

        private bool windowedFullscreen = false;
        private Size oldSize;
        private void ToggleWindowedFullscreen(KeyModifiers modifiers)
        {
            if (!modifiers.Equals(KeyModifiers.Alt))
                return;

            windowedFullscreen = !windowedFullscreen;
            if (windowedFullscreen)
            {
                if (oldSize.IsEmpty)
                    oldSize = ClientSize;
                WindowBorder = WindowBorder.Hidden;
                WindowState = WindowState.Fullscreen;
            }
            else
            {
                WindowBorder = WindowBorder.Fixed;
                WindowState = WindowState.Normal;
                ClientSize = oldSize;
            }
        }

        private void VsyncToggle(KeyModifiers modifiers)
        {
            VSync = VSync == VSyncMode.Adaptive ? VSync = VSyncMode.Off : VSync = VSyncMode.Adaptive;
        }

        private void Quit(KeyModifiers modifiers)
        {
            Exit();
        }
    }
}

using System;
using System.ComponentModel;
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
            // OpenTK configuration
            //
            VSync = VSyncMode.Adaptive;

            // Our initialization
            //
            core = new Core(Color.FromArgb(255, 2, 0, 8));
            core.LoadScene(new TestScene("test scene", new Vector2(ClientSize.Width, ClientSize.Height)));
            core.ActiveScene = "test scene";

            // Subscribe to OpenTK events
            //
            RenderFrame += OnRenderFrame;
            UpdateFrame += OnUpdateFrame;
            Resize += OnResize;
            Load += OnLoad;
            Closing += OnClosing;

            // Bind key event handlers
            //
            keyBinder = new Keybinder(Keyboard);

            keyBinder.SubscribeListener(Key.V, VsyncToggle);
            keyBinder.SubscribeListener(Key.Q, Quit);
            keyBinder.SubscribeListener(Key.F4, Quit);
            keyBinder.SubscribeListener(Key.Enter, ToggleWindowedFullscreen);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// 
        #region OpenTK events

        private void OnRenderFrame(object sender, FrameEventArgs frameEventArgs)
        {
            // opengl rendering specific updates ONLY please
            //
            FPSCounter.Update();

            core.RenderFrame();

            SwapBuffers();
        }

        private void OnUpdateFrame(object sender, FrameEventArgs frameEventArgs)
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
        }


        private void OnResize(object sender, EventArgs eventArgs)
        {
            // rebuild viewport, projection matrix
            //
            core.UpdateScreenSize(ClientRectangle.Width, ClientRectangle.Height);
        }

        private void OnLoad(object sender, EventArgs eventArgs)
        {
            // initialize minimal necessary GPU resources
            //
            WindowBorder = WindowBorder.Fixed;
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            // free all GPU resources
            //
            core.FreeAll();
        }
        
        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// 
        #region key binding event handlers

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

        #endregion
    }
}

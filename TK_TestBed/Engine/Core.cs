using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using TK_TestBed.Engine.Scene;

namespace TK_TestBed.Engine
{
    public class Core
    {
        // The scene should really be the one to hold the camera
        //
        
        private Dictionary<string,BaseScene> scenes;
        private string _ActiveScene;
        public string ActiveScene {
            get { return _ActiveScene; }
            set
            {
                if (scenes.ContainsKey(value))
                {
                    _ActiveScene = value;
                }
                else
                {
                    throw new NullReferenceException("Scene '" + value + "' not loaded");
                }
            }
        }
        public Core(Color clearColor)
        {

            GL.ClearColor(clearColor);
            GL.PushAttrib(AttribMask.DepthBufferBit);
            GL.Disable(EnableCap.DepthTest);

            UpdateScreenSize(1280,720);

            GL.Enable(EnableCap.Texture2D);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            

            scenes = new Dictionary<string,BaseScene>();
            /*
            LoadScene(new TestScene("test scene"));

            ActiveScene = "test scene";*/
        }

        /// <summary>
        /// Loads a scene where the sceneName does not exist in the current list of scenes
        /// </summary>
        /// <param name="scene">scene to be loaded</param>
        /// <returns>true if scene was loaded, false if scene is already loaded</returns>
        public bool LoadScene(BaseScene scene)
        {
            // TODO: support async loading of scenes, they could be complex and we don't want to be blocking
            // the UI threads. Should also provide progress updates for the UI thread to display progress
            //
            if (! scenes.ContainsKey(scene.SceneName))
            {
                scenes.Add(scene.SceneName,scene);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Unloads a scene where the sceneName matches a scene's name in the current list of scenes
        /// </summary>
        /// <param name="sceneName">name of scene to unload</param>
        /// <returns>true if scene was found and unloaded</returns>
        public bool UnloadScene(string sceneName)
        {
            if (scenes.ContainsKey(sceneName))
            {
                scenes[sceneName].FreeObjects();
                scenes.Remove(sceneName);
                return true;
            }
            return false;
        }

        public void RenderFrame()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //GL.Enable(EnableCap.CullFace);
            
            // TODO: Draw objects
            //

            scenes[ActiveScene].DrawObjects();

            //GL.Disable(EnableCap.CullFace);
        }

        public void UpdateScreenSize(int width, int height)
        {
            if (ActiveScene != null)
                scenes[ActiveScene].UpdateScreenSize(width,height);
        }

        public void FreeAll()
        {
            foreach (var scene in scenes.Values)
            {
                scene.FreeObjects();
            }
        }
    }
}

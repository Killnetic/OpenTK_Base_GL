using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using TK_TestBed.Engine.Scene;

namespace TK_TestBed.Engine
{
    public class Core
    {
        private Matrix4 projectionMatrix;
        private Matrix4 modelView;
        private Camera camera = new Camera(70f, 0f, 10000f);

        Color clearColor = Color.FromArgb(255, 2, 0, 8);
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
        public Core()
        {
            GL.ClearColor(clearColor);
            GL.PushAttrib(AttribMask.DepthBufferBit);
            GL.Disable(EnableCap.DepthTest);

            UpdateScreenSize(1280,720);

            GL.Enable(EnableCap.Texture2D);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            modelView = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);

            scenes = new Dictionary<string,BaseScene>();

            LoadScene(new TestScene("test scene"));

            ActiveScene = "test scene";
        }

        /// <summary>
        /// Loads a scene where the sceneName does not exist in the current list of scenes
        /// </summary>
        /// <param name="scene">scene to be loaded</param>
        /// <returns>true if scene was loaded</returns>
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

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
           
           

           
            
            GL.Enable(EnableCap.CullFace);
            modelView = Matrix4.LookAt(camera.position, camera.targetPosition, Vector3.UnitY);
            GL.LoadMatrix(ref modelView);
            // TODO: Draw objects
            //
            //scenes[ActiveScene].DrawObjects();

            GL.Disable(EnableCap.CullFace);
            GL.End();
        }

        public void UpdateScreenSize(int width, int height)
        {
            GL.Viewport(0, 0, width, height);
            float aspectRatio = (float)width / height;

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            
            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(camera.FovY), aspectRatio, camera.ZNear, camera.ZFar);
            GL.LoadMatrix(ref projectionMatrix);
            
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
